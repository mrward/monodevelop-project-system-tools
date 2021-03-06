﻿//
// BuildLoggingProjectExtension.cs
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
using System.Threading.Tasks;
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.ProjectSystem.Tools
{
	class BuildLoggingProjectExtension : ProjectExtension
	{
		protected override Task<TargetEvaluationResult> OnRunTarget (
			ProgressMonitor monitor,
			string target,
			ConfigurationSelector configuration,
			TargetEvaluationContext context)
		{
			if (ProjectSystemService.IsEnabled) {
				return OnMonitorRunTarget (monitor, target, configuration, context);
			}
			return base.OnRunTarget (monitor, target, configuration, context);
		}

		async Task<TargetEvaluationResult> OnMonitorRunTarget (
			ProgressMonitor monitor,
			string target,
			ConfigurationSelector configuration,
			TargetEvaluationContext context)
		{
			using (var buildMonitor = new MSBuildTargetMonitor (Project, target, configuration, context)) {
				try {
					ProgressMonitor modifiedMonitor = buildMonitor.GetProgressMonitor (monitor);
					TargetEvaluationResult result = await base.OnRunTarget (modifiedMonitor, target, configuration, context);
					buildMonitor.OnResult (result);
					return result;
				} catch (Exception ex) {
					buildMonitor.OnException (ex);
					throw;
				}
			}
		}
	}
}
