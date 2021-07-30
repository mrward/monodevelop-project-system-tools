//
// BuildLoggingWidget.UI.cs
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

using AppKit;
using MonoDevelop.Core;
using Xwt;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	partial class BuildLoggingWidget : Widget
	{
		ListView listView;
		ListStore listStore;
		DataField<string> projectDataField = new DataField<string> ();
		DataField<string> dimensionsDataField = new DataField<string> ();
		DataField<string> targetsDataField = new DataField<string> ();
		DataField<string> typeDataField = new DataField<string> ();
		DataField<string> startDataField = new DataField<string> ();
		DataField<string> elapsedDataField = new DataField<string> ();
		DataField<string> statusDataField = new DataField<string> ();
		DataField<MSBuildTarget> msbuildTargetDataField = new DataField<MSBuildTarget> ();

		const int ProjectColumnIndex = 0;
		const int DimensionsColumnIndex = 1;
		const int TargetsColumnIndex = 2;
		const int TypeColumnIndex = 3;
		const int StartColumnIndex = 4;
		const int ElapsedColumnIndex = 5;
		const int StatusColumnIndex = 6;

		void Build ()
		{
			listView = new ListView ();
			listView.BorderVisible = false;
			listView.HeadersVisible = true;
			listStore = new ListStore (
				projectDataField,
				dimensionsDataField,
				targetsDataField,
				typeDataField,
				startDataField,
				elapsedDataField,
				statusDataField,
				msbuildTargetDataField);
			listView.DataSource = listStore;

			AddTextColumn (projectDataField, GettextCatalog.GetString ("Project"));
			AddTextColumn (dimensionsDataField, GettextCatalog.GetString ("Dimensions"));
			AddTextColumn (targetsDataField, GettextCatalog.GetString ("Targets"));
			AddTextColumn (typeDataField, GettextCatalog.GetString ("Type"));
			AddTextColumn (startDataField, GettextCatalog.GetString ("Start"));
			AddTextColumn (elapsedDataField, GettextCatalog.GetString ("Elapsed"));
			AddTextColumn (statusDataField, GettextCatalog.GetString ("Status"));

			SetInitialListViewColumnWidths ();

			Content = listView;
		}

		void AddTextColumn (DataField<string> dataField, string columnTitle)
		{
			var column = new ListViewColumn ();
			column.Title = columnTitle;
			var textViewCell = new TextCellView ();
			textViewCell.TextField = dataField;
			column.Views.Add (textViewCell);
			column.CanResize = true;
			listView.Columns.Add (column);
		}

		void SetInitialListViewColumnWidths ()
		{
			var view = listView.Surface.NativeWidget as NSView;
			if (view is NSScrollView scroll) {
				view = scroll.DocumentView as NSView;
			}

			var tableView = view as NSTableView;
			if (tableView != null) {
				var columns = tableView.TableColumns ();

				columns[ProjectColumnIndex].Width = 250;
				columns[DimensionsColumnIndex].Width = 150;
				columns[TargetsColumnIndex].Width = 450;
				columns[TypeColumnIndex].Width = 100;
				columns[StartColumnIndex].Width = 150;
				columns[ElapsedColumnIndex].Width = 60;
				columns[StatusColumnIndex].Width = 70;
			}
		}
	}
}
