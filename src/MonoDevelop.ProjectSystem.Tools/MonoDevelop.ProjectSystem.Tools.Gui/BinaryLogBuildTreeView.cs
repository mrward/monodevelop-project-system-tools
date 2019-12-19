//
// BinaryLogBuildTreeView.cs
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.VisualStudio.ProjectSystem.LogModel;
using Microsoft.VisualStudio.ProjectSystem.Tools.BinaryLogEditor.ViewModel;
using MonoDevelop.DesignerSupport;
using MonoDevelop.Ide;
using MonoDevelop.Ide.Gui;
using Xwt;
using Xwt.Drawing;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	class BinaryLogBuildTreeView : Widget, IPropertyPadProvider
	{
		ObservableCollection<BaseViewModel> rootViewModels;
		TreeView treeView;
		TreeStore treeStore;
		DataField<string> textDataField = new DataField<string> ();
		DataField<Image> imageDataField = new DataField<Image> ();
		DataField<BaseViewModel> viewModelDataField = new DataField<BaseViewModel> ();
		BaseViewModel selectedViewModel;

		public BinaryLogBuildTreeView (ObservableCollection<BaseViewModel> rootViewModels)
		{
			this.rootViewModels = rootViewModels;

			Build ();
			AddTreeViewItems ();

			treeView.SelectionChanged += TreeViewSelectionChanged;
		}

		void Build ()
		{
			treeView = new TreeView ();
			treeView.HeadersVisible = false;

			treeStore = new TreeStore (textDataField, imageDataField, viewModelDataField);
			treeView.DataSource = treeStore;

			var column = new ListViewColumn ("Node");
			column.Views.Add (new ImageCellView (imageDataField));

			var cellView = new TextCellView ();
			cellView.MarkupField = textDataField;
			column.Views.Add (cellView);
			treeView.Columns.Add (column);

			Content = treeView;
		}

		void AddTreeViewItems ()
		{
			foreach (BaseViewModel viewModel in rootViewModels) {
				TreeNavigator navigator = treeStore.AddNode ();
				SetTreeNodeValues (navigator, viewModel);
				AddChildTreeNodes (navigator, viewModel);
			}
		}

		void AddChildTreeNodes (TreeNavigator navigator, BaseViewModel viewModel)
		{
			foreach (BaseViewModel childViewModel in viewModel.Children) {
				TreeNavigator childNavigator = navigator.AddChild ();
				SetTreeNodeValues (childNavigator, childViewModel);
				AddChildTreeNodes (childNavigator, childViewModel);

				navigator.MoveToParent ();
			}
		}

		void SetTreeNodeValues (TreeNavigator navigator, BaseViewModel viewModel)
		{
			if (viewModel is NodeViewModel nodeViewModel) {
				navigator.SetValue (textDataField, GetTreeNodeText (nodeViewModel));
				navigator.SetValue (imageDataField, GetTreeNodeImage (nodeViewModel));
			} else {
				navigator.SetValue (textDataField, GLib.Markup.EscapeText (viewModel.Text));
			}

			navigator.SetValue (viewModelDataField, viewModel);
		}

		static string GetTreeNodeText (NodeViewModel nodeViewModel)
		{
			string markup = GLib.Markup.EscapeText (nodeViewModel.Text);

			switch (nodeViewModel.Result) {
				case Result.Failed:
				case Result.Succeeded:
					if (nodeViewModel.IsPrimary) {
						markup = "<b>" + markup + "</b>";
					}
					return markup + " (" + nodeViewModel.Elapsed + ")";

				case Result.Skipped:
					return "<span weight='ultralight'>" + markup + "</span>";

				default:
					return markup;
			}
		}

		static Image GetTreeNodeImage (NodeViewModel nodeViewModel)
		{
			switch (nodeViewModel.Result) {
				case Result.Failed:
					return ImageService.GetIcon (Stock.Error, Gtk.IconSize.Menu);
				case Result.Succeeded:
					return ImageService.GetIcon ("md-done", Gtk.IconSize.Menu);

				default:
					return null;
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				treeView.SelectionChanged -= TreeViewSelectionChanged;
			}
			base.Dispose (disposing);
		}

		void TreeViewSelectionChanged (object sender, EventArgs e)
		{
			selectedViewModel = GetBaseViewModel (treeView.SelectedRow);
		}

		BaseViewModel GetBaseViewModel (TreePosition position)
		{
			if (position == null)
				return null;

			TreeNavigator navigator = treeStore.GetNavigatorAt (position);
			if (navigator == null)
				return null;

			return navigator.GetValue (viewModelDataField);
		}

		public object GetActiveComponent ()
		{
			return selectedViewModel?.Properties;
		}

		public object GetProvider ()
		{
			return GetActiveComponent ();
		}

		public void OnEndEditing (object obj)
		{
		}

		public void OnChanged (object obj)
		{
		}
	}
}
