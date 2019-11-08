//
// MSBuildProcessProgressMonitor.cs
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
using MonoDevelop.Core;
using MonoDevelop.Core.Execution;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildProcessProgressMonitor : MSBuildTargetProgressMonitor
	{
		public MSBuildProcessProgressMonitor (
			TextWriter outWriter,
			TextWriter errorWriter,
			FilePath logFileName)
			: base (logFileName)
		{
			// Setting Log and ErrorLog will chain these writers to the
			// ProgressMonitor's Log and ErrorLog writers.
			Log = outWriter;
			ErrorLog = errorWriter;
		}

		public ProcessWrapper Process { get; set; }
	}
}
