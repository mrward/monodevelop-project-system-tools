//
// MSBuildTargetMonitor.cs
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
using System.Diagnostics;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Projects;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildTargetMonitor : IDisposable
	{
		MSBuildTarget buildTarget;
		Stopwatch stopwatch;
		MSBuildTargetProgressMonitor progressMonitor;

		public MSBuildTargetMonitor (
			Project project,
			string target,
			ConfigurationSelector configuration,
			TargetEvaluationContext context)
		{
			// Ensure log verbosity is set for non-build targets.
			context.LogVerbosity = Runtime.Preferences.MSBuildVerbosity.Value;

			buildTarget = new MSBuildTarget {
				ProjectName = project.Name,
				ProjectFileName = project.FileName.FileName,
				Targets = target ?? string.Empty,
				BuildType = MSBuildTarget.GetBuildType (target),
				Dimensions = project.GetDimensions (configuration)
			};

			buildTarget.StartTime = DateTime.UtcNow;
			stopwatch = Stopwatch.StartNew ();

			ProjectSystemService.OnTargetStarted (buildTarget);
		}

		public ProgressMonitor GetProgressMonitor (ProgressMonitor monitor)
		{
			var aggregatedMonitor = monitor as AggregatedProgressMonitor;
			if (aggregatedMonitor == null) {
				aggregatedMonitor = new AggregatedProgressMonitor (monitor);
			}

			buildTarget.GenerateLogFileName ();

			progressMonitor = new MSBuildTargetProgressMonitor (buildTarget.LogFileName);
			aggregatedMonitor.AddFollowerMonitor (progressMonitor, progressMonitor.Actions);

			return aggregatedMonitor;
		}

		public void Dispose ()
		{
			stopwatch.Stop ();

			progressMonitor?.Dispose ();
		}

		public void OnResult (TargetEvaluationResult result)
		{
			stopwatch.Stop ();

			progressMonitor?.Dispose ();

			buildTarget.Status = result.GetMSBuildTargetStatus ();
			buildTarget.Duration = stopwatch.Elapsed;
			buildTarget.BuildSessionBinLogFileName = ProjectSystemService.BuildSessionBinLogFileName;

			ProjectSystemService.OnTargetFinished (buildTarget);
		}

		public void OnException (Exception ex)
		{
			stopwatch.Stop ();

			progressMonitor?.Dispose ();

			buildTarget.Duration = stopwatch.Elapsed;
			buildTarget.Status = MSBuildTargetStatus.Exception;
			buildTarget.BuildSessionBinLogFileName = ProjectSystemService.BuildSessionBinLogFileName;

			ProjectSystemService.OnTargetFinished (buildTarget);
		}
	}
}
