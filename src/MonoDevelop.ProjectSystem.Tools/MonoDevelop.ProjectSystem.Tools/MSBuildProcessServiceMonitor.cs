//
// MSBuildProcessServiceMonitor.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2019 Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;
using MonoDevelop.Ide;
using MonoDevelop.Projects.MSBuild;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildProcessServiceMonitor : IDisposable
	{
		readonly Func<string, string, string, TextWriter, TextWriter, EventHandler, ProcessWrapper> originalHandler;
		readonly Dictionary<int, MSBuildTarget> buildTargets = new Dictionary<int, MSBuildTarget> ();

		static int sessionId;

		public MSBuildProcessServiceMonitor ()
		{
			originalHandler = MSBuildProcessService.StartProcessHandler;
			MSBuildProcessService.StartProcessHandler = StartProcess;
		}

		public void Dispose ()
		{
			MSBuildProcessService.StartProcessHandler = originalHandler;
		}

		ProcessWrapper StartProcess (string command, string arguments, string workingDirectory, TextWriter outWriter, TextWriter errorWriter, EventHandler exited)
		{
			int currentSessionId = Interlocked.Increment (ref sessionId);

			var msbuildProcessArguments = new MSBuildProcessArguments (arguments);

			var buildTarget = new MSBuildTarget {
				ProjectName = GettextCatalog.GetString ("Solution"),
				ProjectFileName = GettextCatalog.GetString ("Solution"),
				Targets = msbuildProcessArguments.Targets,
				BuildType = msbuildProcessArguments.BuildType,
				Dimensions = GetDimensions (msbuildProcessArguments.Targets)
			};

			buildTarget.Start ();

			arguments = MSBuildProcessArguments.AddVerbosity (arguments, Runtime.Preferences.MSBuildVerbosity.Value);
			arguments = MSBuildProcessArguments.AddBinLogFileName (arguments, buildTarget.BinLogFileName);

			lock (buildTargets) {
				buildTargets [currentSessionId] = buildTarget;
			}

			ProjectSystemService.OnTargetStarted (buildTarget);

			var monitor = new MSBuildProcessProgressMonitor (outWriter, errorWriter, buildTarget.LogFileName);

			ProcessWrapper process = Runtime.ProcessService.StartProcess (
				command,
				arguments,
				workingDirectory,
				monitor.Log,
				monitor.ErrorLog,
				exited);

			monitor.Process = process;

			process.Task.ContinueWith (_ => {
				OnMSBuildProcessExited (currentSessionId, monitor);
			});

			return process;
		}

		string GetDimensions (string targets)
		{
			if (StringComparer.OrdinalIgnoreCase.Equals ("GenerateRestoreGraphFile", targets)) {
				return IdeApp.ProjectOperations.CurrentSelectedSolution.GetDimensions ();
			}

			return string.Empty;
		}

		void OnMSBuildProcessExited (int currentSessionId, MSBuildProcessProgressMonitor monitor)
		{
			MSBuildTarget buildTarget = null;

			using (monitor) {
				lock (buildTargets) {
					if (buildTargets.TryGetValue (currentSessionId, out buildTarget)) {
						buildTargets.Remove (currentSessionId);
					} else {
						return;
					}
				}

				if (monitor.Process.Task.IsFaulted) {
					buildTarget.OnException (monitor.Process.Task.Exception);
				} else if (monitor.Process.Task.IsCanceled) {
					buildTarget.OnResult (MSBuildTargetStatus.Failed);
				} else if (monitor.Process.ProcessAsyncOperation.ExitCode == 0) {
					buildTarget.OnResult (MSBuildTargetStatus.Finished);
				} else {
					buildTarget.OnResult (MSBuildTargetStatus.Failed);
				}
			}

			ProjectSystemService.OnTargetFinished (buildTarget);
		}
	}
}
