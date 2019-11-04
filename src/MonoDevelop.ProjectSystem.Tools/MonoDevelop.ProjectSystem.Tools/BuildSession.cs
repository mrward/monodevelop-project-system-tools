//
// BuildSession.cs
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
	sealed class BuildSession
	{
		public BuildSession (BuildSessionStartedEvent buildSessionStarted)
		{
			Id = buildSessionStarted.SessionId;
			BinLogFileName = buildSessionStarted.LogFile;
			IsRunning = true;
			ProjectFileNames = new List<FilePath> ();
		}

		public int Id { get; set; }
		public FilePath BinLogFileName { get; set; }

		public bool IsRunning { get; set; }

		/// <summary>
		/// Populated at the end of the solution build session after the binlog is processed.
		/// </summary>
		public List<FilePath> ProjectFileNames { get; private set; }

		public Task ProcessBuildSessionAsync ()
		{
			if (Runtime.IsMainThread) {
				return Task.Run (() => ProcessBuildSessionAsync ());
			}

			try {
				using (var processor = new BinaryLogProcessor (BinLogFileName)) {
					processor.Process ();
					ProjectFileNames = processor.ProjectFileNames.ToList ();
				}
			} catch (Exception ex) {
				LoggingService.LogError (string.Format ("Unable to process binlog '{0}'", BinLogFileName), ex);
			}

			return Task.CompletedTask;
		}
	}
}
