﻿//
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
	class BinaryLogTargetSummaryView : BinaryLogSummaryViewBase
	{
		readonly ObservableCollection<TargetListViewModel> targetListViewItems;

		DataField<string> targetNameDataField = new DataField<string> ();
		DataField<string> sourceFileNameDataField = new DataField<string> ();
		DataField<string> callsDataField = new DataField<string> ();
		DataField<int> callsSortField = new DataField<int> ();
		DataField<string> timeDataField = new DataField<string> ();
		// Cannot sort on TimeSpan. Unsupported data type.
		DataField<long> timeSortField = new DataField<long> ();
		DataField<string> percentageDataField = new DataField<string> ();
		DataField<double> percentageSortField = new DataField<double> ();

		public BinaryLogTargetSummaryView (ObservableCollection<TargetListViewModel> targetListViewItems)
		{
			this.targetListViewItems = targetListViewItems;

			Build (
				targetNameDataField,
				sourceFileNameDataField,
				callsDataField,
				callsSortField,
				timeDataField,
				timeSortField,
				percentageDataField,
				percentageSortField);

			AddListViewColumns ();

			AddListViewItems ();
		}

		void AddListViewColumns ()
		{
			AddTextColumn (targetNameDataField, GettextCatalog.GetString ("Target Name"));
			AddTextColumn (sourceFileNameDataField, GettextCatalog.GetString ("Source"));
			AddTextColumn (callsDataField, GettextCatalog.GetString ("Calls"), callsSortField);
			AddTextColumn (timeDataField, GettextCatalog.GetString ("Time"), timeSortField);
			AddTextColumn (percentageDataField, GettextCatalog.GetString ("Percentage"), percentageSortField);
		}

		void AddListViewItems ()
		{
			foreach (TargetListViewModel viewModel in targetListViewItems) {
				int row = listStore.AddRow ();

				listStore.SetValue (row, targetNameDataField, viewModel.Name);
				listStore.SetValue (row, sourceFileNameDataField, viewModel.SourceFilePath);
				listStore.SetValue (row, callsDataField, viewModel.Number.ToString ());
				listStore.SetValue (row, callsSortField, viewModel.Number);

				SetTimeValue (row, viewModel.Time, timeDataField, timeSortField);
				SetPercentageValue (row, viewModel.Percentage, percentageDataField, percentageSortField);
			}
		}
	}
}