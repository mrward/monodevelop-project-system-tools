//
// BinaryLogProcessor.cs
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
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;
using MonoDevelop.Core;

namespace MonoDevelop.ProjectSystem.Tools
{
	class BinaryLogProcessor : IDisposable
	{
		readonly FilePath binLogFileName;
		readonly BinaryLogReplayEventSource replayEventSource = new BinaryLogReplayEventSource ();
		readonly HashSet<FilePath> projectFileNames = new HashSet<FilePath> ();

		public BinaryLogProcessor (FilePath binLogFileName)
		{
			this.binLogFileName = binLogFileName;

			replayEventSource.ProjectStarted += ProjectStarted;
		}

		public IEnumerable<FilePath> ProjectFileNames {
			get { return projectFileNames; }
		}

		public void Dispose ()
		{
			replayEventSource.ProjectStarted -= ProjectStarted;
		}

		public void Process ()
		{
			replayEventSource.Replay (binLogFileName);
		}

		void ProjectStarted (object sender, ProjectStartedEventArgs e)
		{
			if (!string.IsNullOrEmpty (e.ProjectFile)) {
				projectFileNames.Add (e.ProjectFile);
			}

			if (e.Properties == null) {
				return;
			}

			foreach (DictionaryEntry entry in e.Properties) {
				var key = entry.Key as string;
				if (key == "MSBuildProjectFullPath") {
					var fileName = new FilePath ((string)entry.Value);
					projectFileNames.Add (fileName);
				}
			}
		}
	}
}
