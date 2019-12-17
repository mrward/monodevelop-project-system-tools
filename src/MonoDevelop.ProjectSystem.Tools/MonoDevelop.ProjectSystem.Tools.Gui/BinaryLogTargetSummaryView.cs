//
// BinaryLogTargetSummaryView.cs
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

using System.Collections.ObjectModel;
using Microsoft.VisualStudio.ProjectSystem.Tools.BinaryLogEditor.ViewModel;
using MonoDevelop.Core;
using Xwt;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	class BinaryLogTargetSummaryView : Widget
	{
		readonly ObservableCollection<TargetListViewModel> targetListViewItems;
		ListView listView;
		ListStore listStore;
		DataField<string> targetNameDataField = new DataField<string> ();
		DataField<string> sourceFileNameDataField = new DataField<string> ();
		DataField<string> callsDataField = new DataField<string> ();
		DataField<int> callsSortField = new DataField<int> ();
		DataField<string> timeDataField = new DataField<string> ();
		// Cannot sort on TimeSpan. Unsupported data type.
		DataField<long> timeSortField = new DataField<long> ();
		DataField<string> percentageDataField = new DataField<string> ();

		// Cannot sort on a double field. Unsupported data type.
		DataField<int> percentageSortField = new DataField<int> ();

		public BinaryLogTargetSummaryView (ObservableCollection<TargetListViewModel> targetListViewItems)
		{
			this.targetListViewItems = targetListViewItems;

			Build ();

			AddListViewItems ();
		}

		void Build ()
		{
			listView = new ListView ();
			listView.BorderVisible = false;
			listView.HeadersVisible = true;
			listStore = new ListStore (
				targetNameDataField,
				sourceFileNameDataField,
				callsDataField,
				callsSortField,
				timeDataField,
				timeSortField,
				percentageDataField,
				percentageSortField);
			listView.DataSource = listStore;

			AddTextColumn (targetNameDataField, GettextCatalog.GetString ("Target Name"));
			AddTextColumn (sourceFileNameDataField, GettextCatalog.GetString ("Source"));
			AddTextColumn (callsDataField, GettextCatalog.GetString ("Number"), callsSortField);
			AddTextColumn (timeDataField, GettextCatalog.GetString ("Time"), timeSortField);
			AddTextColumn (percentageDataField, GettextCatalog.GetString ("Percentage"));

			Content = listView;
		}

		void AddTextColumn (DataField<string> dataField, string columnTitle, IDataField sortDataField = null)
		{
			var column = new ListViewColumn ();
			column.Title = columnTitle;
			column.SortDataField = sortDataField ?? dataField;

			var textViewCell = new TextCellView ();
			textViewCell.TextField = dataField;
			column.Views.Add (textViewCell);
			column.CanResize = true;
			listView.Columns.Add (column);
		}

		void AddListViewItems ()
		{
			foreach (TargetListViewModel viewModel in targetListViewItems) {
				int row = listStore.AddRow ();
				listStore.SetValues (
					row,
					// Target Name
					targetNameDataField,
					viewModel.Name,
					// Source
					sourceFileNameDataField,
					viewModel.SourceFilePath,
					// Calls
					callsDataField,
					viewModel.Number.ToString (),
					callsSortField,
					viewModel.Number,
					// Time
					timeDataField,
					viewModel.Time.ToString (@"hh\:mm\:ss\:ffff"),
					timeSortField,
					viewModel.Time.Ticks,
					// Percentage
					percentageDataField,
					viewModel.Percentage.ToString ("P2"),
					percentageSortField,
					(int)(viewModel.Percentage * 100)
				);
			}
		}
	}
}
