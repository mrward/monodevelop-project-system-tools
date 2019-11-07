//
// MSBuildProcessArguments.cs
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

using System.Text;
using MonoDevelop.Core;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildProcessArguments
	{
		string arguments;

		public MSBuildProcessArguments (string arguments)
		{
			this.arguments = arguments ?? string.Empty;
			Parse ();
		}

		public override string ToString ()
		{
			return arguments;
		}

		public string Targets { get; internal set; }
		public BuildType BuildType { get; internal set; }

		void Parse ()
		{
			const string targetArgument = "--target:";

			var targetsBuilder = StringBuilderCache.Allocate ();

			string[] parts = arguments.Split (' ');
			foreach (string part in parts) {
				if (part.StartsWith (targetArgument)) {
					AppendTargets (targetsBuilder, part.Substring (targetArgument.Length + 1));
				} else if (part.StartsWith ("/t") || part.StartsWith ("-t")) {
					AppendTargets (targetsBuilder, part.Substring (3));
				}
			}

			Targets = StringBuilderCache.ReturnAndFree (targetsBuilder);
			BuildType = MSBuildTarget.GetBuildType (Targets);
		}

		static void AppendTargets (StringBuilder targetsBuilder, string targets)
		{
			if (targetsBuilder.Length > 0) {
				targetsBuilder.Append (';');
			}

			targetsBuilder.Append (targets);
		}

		public string AddBinLogFileName (FilePath binLogFileName)
		{
			return arguments + GetQuotedBinLogArgument (binLogFileName);
		}

		string GetQuotedBinLogArgument (FilePath binLogFileName)
		{
			return " -bl:\"" + binLogFileName + "\"";
		}
	}
}
