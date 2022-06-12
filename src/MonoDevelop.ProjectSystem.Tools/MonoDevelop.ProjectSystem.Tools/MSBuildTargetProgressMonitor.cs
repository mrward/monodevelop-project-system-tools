//
// MSBuildTargetProgressMonitor.cs
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

using System.IO;
using System.Text;
using MonoDevelop.Core;
using MonoDevelop.Core.PooledObjects;
using MonoDevelop.Core.ProgressMonitoring;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildTargetProgressMonitor : ProgressMonitor
	{
		PooledStringBuilder logBuilder = PooledStringBuilder.GetInstance ();
		readonly FilePath logFileName;
		bool disposed;

		public MSBuildTargetProgressMonitor (FilePath logFileName)
		{
			this.logFileName = logFileName;
		}

		public MonitorAction Actions {
			get { return MonitorAction.WriteLog | MonitorAction.Dispose; }
		}

		protected override void OnWriteLog (string message)
		{
			lock (logBuilder) {
				logBuilder.Append (message);
			}
		}

		protected override void OnWriteErrorLog (string message)
		{
			lock (logBuilder) {
				logBuilder.Append (message);
			}
		}

		protected override void OnDispose (bool disposing)
		{
			base.OnDispose (disposing);

			if (disposing && !disposed) {
				lock (logBuilder) {
					disposed = true;
					SaveLogOutputToFile ();
				}
			}
		}

		void SaveLogOutputToFile ()
		{
			string text = logBuilder.ToStringAndFree ();
			File.WriteAllText (logFileName, text);
		}
	}
}
