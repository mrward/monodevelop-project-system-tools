//
// BinaryLogSummaryViewBase.cs
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
using Xwt;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	abstract class BinaryLogSummaryViewBase : Widget
	{
		protected ListView listView;
		protected ListStore listStore;

		protected void Build (params IDataField[] fields)
		{
			listView = new ListView ();
			listView.BorderVisible = false;
			listView.HeadersVisible = true;

			listStore = new ListStore (fields);
			listView.DataSource = listStore;

			Content = listView;
		}

		protected void AddTextColumn (DataField<string> dataField, string columnTitle, IDataField sortDataField = null)
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

		protected void SetTimeValue (int row, TimeSpan time, DataField<string> dataField, DataField<long> sortField)
		{
			listStore.SetValue (row, dataField, time.ToString (@"hh\:mm\:ss\:ffff"));
			listStore.SetValue (row, sortField, time.Ticks);
		}

		protected void SetPercentageValue (int row, double percentage, DataField<string> dataField, DataField<double> sortField)
		{
			listStore.SetValue (row, dataField, percentage.ToString ("P2"));
			listStore.SetValue (row, sortField, percentage);
		}
	}
}
