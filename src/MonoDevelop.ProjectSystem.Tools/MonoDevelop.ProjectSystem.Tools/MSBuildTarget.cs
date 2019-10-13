//
// MSBuildTarget.cs
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
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildTarget
	{
		public string Targets { get; set; }
		public string ProjectFileName { get; set; }
		public string BuildType { get; set; }
		public string Dimensions { get; set; }

		public DateTime StartTime { get; set; }
		public TimeSpan Duration { get; set; }

		public MSBuildTargetStatus Status { get; set; }

		public static string GetBuildType (string target)
		{
			if (string.IsNullOrEmpty (target)) {
				return string.Empty;
			}

			if (target == ProjectService.BuildTarget ||
				target == ProjectService.CleanTarget ||
				target == "Pack") {
				return "Build";
			}

			return "DesignTimeBuild";
		}
	}
}
