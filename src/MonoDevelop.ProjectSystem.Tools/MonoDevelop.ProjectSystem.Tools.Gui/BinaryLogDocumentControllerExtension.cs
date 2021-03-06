﻿//
// BinaryLogDocumentControllerExtension.cs
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
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem.Tools.BinaryLogEditor;
using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.DesignerSupport;
using MonoDevelop.Ide.Gui.Documents;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	class BinaryLogDocumentControllerExtension : DocumentControllerExtension, IPropertyPadProvider
	{
		DocumentView mainView;
		BinaryLogBuildTreeView buildTreeView;
		DocumentViewContent buildTreeDocumentView;
		BinaryLogTargetSummaryView targetSummaryView;
		BinaryLogTaskSummaryView taskSummaryView;
		//BinaryLogEvaluationSummaryView evaluationSummaryView;
		BinaryLogDocumentData binaryLogDocument;
		BinaryLogViewModels viewModels;
		FilePath originalFileName;

		public override Task<bool> SupportsController (DocumentController controller)
		{
			bool supported = false;
			if (controller is FileDocumentController fileController &&
				fileController.FilePath.IsNotNull) {
				supported = fileController.FilePath.HasExtension (".binlog");
			}

			return Task.FromResult (supported);
		}

		public override Task Initialize (Properties status)
		{
			var fileController = Controller as FileDocumentController;
			if (fileController != null) {
				return Initialize (fileController, status);
			}

			return base.Initialize (status);
		}

		async Task Initialize (FileDocumentController fileController, Properties status)
		{
			await base.Initialize (status);

			Controller.DocumentTitleChanged += OnDocumentTitleChanged;

			originalFileName = fileController.FilePath;

			binaryLogDocument = new BinaryLogDocumentData ();
			viewModels = new BinaryLogViewModels (binaryLogDocument);
			await Task.Run (() => {
				binaryLogDocument.Load (fileController.FilePath);
				viewModels.Build ();
			});
		}

		public override void Dispose ()
		{
			base.Dispose ();

			if (Controller != null) {
				Controller.DocumentTitleChanged -= OnDocumentTitleChanged;
			}

			mainView = null;
		}

		void OnDocumentTitleChanged (object sender, EventArgs e)
		{
			if (Controller is FileDocumentController fileController) {
				// Workaround the BuildOutputView setting the document title to be the full filepath.
				fileController.DocumentTitle = fileController.FilePath.FileName;
			}
		}

		protected override async Task<DocumentView> OnInitializeView ()
		{
			mainView = await base.OnInitializeView ();

			buildTreeView = new BinaryLogBuildTreeView (viewModels.BuildTreeViewItems);
			buildTreeDocumentView = AttachView (buildTreeView, GettextCatalog.GetString ("Build"));

			targetSummaryView = new BinaryLogTargetSummaryView (viewModels.TargetListViewItems);
			AttachView (targetSummaryView, GettextCatalog.GetString ("Target Summary"));

			taskSummaryView = new BinaryLogTaskSummaryView (viewModels.TaskListViewItems);
			AttachView (taskSummaryView, GettextCatalog.GetString ("Task Summary"));

			// Not seen any evaluations so not adding this view since it will always be empty.
			//evaluationSummaryView = new BinaryLogEvaluationSummaryView (viewModels.EvaluationListViewItems);
			//AttachView (evaluationSummaryView, GettextCatalog.GetString ("Evaluation Summary"));

			return mainView;
		}

		DocumentViewContent AttachView (Xwt.Widget childView, string title)
		{
			var content = new DocumentViewContent (() => new XwtControl (childView)) {
				Title = title
			};
			mainView.AttachedViews.Add (content);

			return content;
		}

		/// <summary>
		/// Workaround SaveAs dialog being shown twice by the default BuildOutputViewContent.
		/// </summary>
		public override Task OnSave ()
		{
			if (Controller is FileDocumentController fileController) {
				FilePath fileName = fileController.FilePath;
				if (originalFileName.IsNotNull) {
					File.Copy (originalFileName, fileName, true);
					originalFileName = fileName;
					return Task.CompletedTask;
				}
			}
			return base.OnSave ();
		}

		object IPropertyPadProvider.GetActiveComponent ()
		{
			if (!IsBuildTreeViewActive ())
				return null;

			return buildTreeView.GetActiveComponent ();
		}

		object IPropertyPadProvider.GetProvider ()
		{
			if (!IsBuildTreeViewActive ())
				return null;

			return buildTreeView.GetProvider ();
		}

		bool IsBuildTreeViewActive ()
		{
			return mainView.ActiveViewInHierarchy == buildTreeDocumentView;
		}

		void IPropertyPadProvider.OnEndEditing (object obj)
		{
		}

		void IPropertyPadProvider.OnChanged (object obj)
		{
		}
	}
}
