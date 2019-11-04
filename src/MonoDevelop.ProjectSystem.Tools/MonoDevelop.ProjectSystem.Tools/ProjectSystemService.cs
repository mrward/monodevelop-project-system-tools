//
// ProjectSystemToolsService.cs
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
using System.Linq;
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.ProjectSystem.Tools
{
	static class ProjectSystemService
	{
		public static bool IsEnabled { get; set; }

		public static event EventHandler<MSBuildTargetEventArgs> MSBuildTargetStarted;
		public static event EventHandler<MSBuildTargetEventArgs> MSBuildTargetFinished;

		static readonly Dictionary<int, BuildSession> runningBuildSessions =
			new Dictionary<int, BuildSession> ();

		internal static void OnTargetStarted (MSBuildTarget target)
		{
			Runtime.RunInMainThread (() => {
				MSBuildTargetStarted?.Invoke (null, new MSBuildTargetEventArgs (target));
			}).Ignore ();
		}

		internal static void OnTargetFinished (MSBuildTarget target)
		{
			target.BuildSessions = GetBuildSessions (running: true);

			Runtime.RunInMainThread (() => {
				MSBuildTargetFinished?.Invoke (null, new MSBuildTargetEventArgs (target));
			}).Ignore ();
		}

		internal static void OnBuildSessionStarted (BuildSessionStartedEvent buildSessionStarted)
		{
			lock (runningBuildSessions) {
				var buildSession = new BuildSession (buildSessionStarted);
				runningBuildSessions [buildSession.Id] = buildSession;
			}
		}

		internal static void OnBuildSessionFinished (BuildSessionFinishedEvent buildSessionFinished)
		{
			lock (runningBuildSessions) {
				if (runningBuildSessions.TryGetValue (buildSessionFinished.SessionId, out BuildSession buildSession)) {
					buildSession.IsRunning = false;
					buildSession.ProcessBuildSessionAsync ()
						.Ignore ();
				}
			}
		}

		static List<BuildSession> GetBuildSessions (bool running)
		{
			var buildSessions = new List<BuildSession> ();

			lock (runningBuildSessions) {
				foreach (BuildSession buildSession in runningBuildSessions.Values) {
					if (buildSession.IsRunning == running) {
						buildSessions.Add (buildSession);
					}
				}
			}

			return buildSessions;
		}
	}
}
