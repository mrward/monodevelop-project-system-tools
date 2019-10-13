//
// BuildLoggingWidget.cs
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

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	partial class BuildLoggingWidget
	{
		public BuildLoggingWidget ()
		{
			Build ();
		}

		public void ClearItems ()
		{
			listStore.Clear ();
		}

		public void AddMSBuildTarget (MSBuildTarget target)
		{
			int row = listStore.AddRow ();

			listStore.SetValues (
				row,

				projectDataField,
				target.ProjectFileName,

				dimensionsDataField,
				target.Dimensions,

				targetsDataField,
				target.Targets,

				typeDataField,
				target.BuildType,

				startDataField,
				target.StartTime.ToString ("s"),

				statusDataField,
				target.Status.GetDisplayText (),

				msbuildTargetDataField,
				target);
		}

		public void UpdateMSBuildTarget (MSBuildTarget target)
		{
			for (int row = listStore.RowCount - 1; row >= 0; --row) {
				MSBuildTarget currentTarget = listStore.GetValue (row, msbuildTargetDataField);
				if (target == currentTarget) {
					listStore.SetValues (
						row,

						elapsedDataField,
						target.Duration.ToString ("g"),

						statusDataField,
						target.Status.GetDisplayText ());

					return;
				}
			}
		}
	}
}
