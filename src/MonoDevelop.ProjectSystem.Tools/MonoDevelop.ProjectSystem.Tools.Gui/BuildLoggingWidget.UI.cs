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
		DataField<string> startSortDataField = new DataField<string> ();
		DataField<string> elapsedDataField = new DataField<string> ();
		DataField<long> elapsedSortDataField = new DataField<long> ();
		DataField<string> statusDataField = new DataField<string> ();
		DataField<MSBuildTarget> msbuildTargetDataField = new DataField<MSBuildTarget> ();

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
				startSortDataField,
				elapsedDataField,
				elapsedSortDataField,
				statusDataField,
				msbuildTargetDataField);
			listView.DataSource = listStore;

			AddTextColumn (projectDataField, GettextCatalog.GetString ("Project"));
			AddTextColumn (dimensionsDataField, GettextCatalog.GetString ("Dimensions"));
			AddTextColumn (targetsDataField, GettextCatalog.GetString ("Targets"));
			AddTextColumn (typeDataField, GettextCatalog.GetString ("Type"));
			AddTextColumn (startDataField, GettextCatalog.GetString ("Start"), startSortDataField);
			AddTextColumn (elapsedDataField, GettextCatalog.GetString ("Elapsed"), elapsedSortDataField);
			AddTextColumn (statusDataField, GettextCatalog.GetString ("Status"));

			Content = listView;
		}

		void AddTextColumn (DataField<string> dataField, string columnTitle, IDataField sortDataField = null)
		{
			var column = new ListViewColumn ();
			column.Title = columnTitle;
			var textViewCell = new TextCellView ();
			textViewCell.TextField = dataField;
			column.SortDataField = sortDataField ?? dataField;
			column.Views.Add (textViewCell);
			column.CanResize = true;
			listView.Columns.Add (column);
		}
	}
}
