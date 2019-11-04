//
// BuildLoggingSolutionExtension.cs
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

using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Core.ProgressMonitoring;
using MonoDevelop.Projects;

namespace MonoDevelop.ProjectSystem.Tools
{
	class BuildLoggingSolutionExtension : SolutionExtension
	{
		protected override Task OnBeginBuildOperation (
			ProgressMonitor monitor,
			ConfigurationSelector configuration,
			OperationContext operationContext)
		{
			if (ProjectSystemService.IsEnabled) {
				monitor = AddMSBuildBinLogProgressMonitor (monitor);
			}
			return base.OnBeginBuildOperation (monitor, configuration, operationContext);
		}

		ProgressMonitor AddMSBuildBinLogProgressMonitor (ProgressMonitor monitor)
		{
			var aggregatedMonitor = monitor as AggregatedProgressMonitor;
			if (aggregatedMonitor == null) {
				aggregatedMonitor = new AggregatedProgressMonitor (monitor);
			}

			// Enable monitor for WriteLog only. This avoids errors due to a task being started before
			// the OnBeginBuildOperation was called which the bin log progress monitor is not aware of.
			var binLogMonitor = new MSBuildBinLogProgressMonitor ();
			aggregatedMonitor.AddFollowerMonitor (binLogMonitor, binLogMonitor.Actions);

			return aggregatedMonitor;
		}
	}
}
