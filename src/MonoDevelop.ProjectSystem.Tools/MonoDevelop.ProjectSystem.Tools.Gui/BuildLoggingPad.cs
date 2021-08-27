//
// BuildLoggingPad.cs
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
using System.Linq;
using MonoDevelop.Components;
using MonoDevelop.Components.Declarative;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	public class BuildLoggingPad : PadContent
	{
		BuildLoggingWidget widget;
		XwtControl control;
		ToolbarButtonItem startButton;
		ToolbarButtonItem stopButton;
		ToolbarButtonItem clearButton;
		ToolbarPopUpButtonItem buildTypeFilterComboBox;
		ContextMenu buildTypeFilterMenu;
		ToolbarSearchItem searchEntry;
		TimeSpan searchDelayTimeSpan = TimeSpan.FromMilliseconds (250);
		IDisposable searchTimer;

		BuildType[] buildTypes = new BuildType [] {
			BuildType.All,
			BuildType.Build,
			BuildType.DesignTimeBuild
		};

		public override Control Control {
			get {
				if (control == null) {
					widget = new BuildLoggingWidget ();
					control = new XwtControl (widget);
				}
				// Returning control does not work.
				return control.GetNativeWidget<AppKit.NSView> ();
			}
		}

		public override void Dispose ()
		{
			ProjectSystemService.MSBuildTargetStarted -= MSBuildTargetStarted;
			ProjectSystemService.MSBuildTargetFinished -= MSBuildTargetFinished;

			startButton.Clicked -= OnStartButtonClicked;
			stopButton.Clicked -= OnStopButtonClicked;
			clearButton.Clicked -= OnClearButtonClicked;
			searchEntry.TextChanged -= SearchEntryChanged;

			foreach (ContextMenuItem menuItem in buildTypeFilterMenu.Items) {
				menuItem.Clicked -= BuildTypeFilterComboBoxChanged;
			}

			DisposeExistingTimer ();

			widget.Dispose ();

			base.Dispose ();
		}

		protected override void Initialize (IPadWindow window)
		{
			var toolbar = new Toolbar ();

			startButton = new ToolbarButtonItem (toolbar.Properties, nameof (startButton));
			startButton.Icon = Ide.Gui.Stock.RunProgramIcon;
			startButton.Clicked += OnStartButtonClicked;
			startButton.Tooltip = GettextCatalog.GetString ("Enable build logging");
			toolbar.AddItem (startButton);

			stopButton = new ToolbarButtonItem (toolbar.Properties, nameof (stopButton));
			stopButton.Icon = Ide.Gui.Stock.Stop;
			stopButton.Clicked += OnStopButtonClicked;
			stopButton.Tooltip = GettextCatalog.GetString ("Stop build logging");
			// Cannot disable the button before the underlying NSView is created.
			//stopButton.Enabled = false;
			toolbar.AddItem (stopButton);

			clearButton = new ToolbarButtonItem (toolbar.Properties, nameof (clearButton));
			clearButton.Icon = Ide.Gui.Stock.Clear;
			clearButton.Clicked += OnClearButtonClicked;
			clearButton.Tooltip = GettextCatalog.GetString ("Clear");
			toolbar.AddItem (clearButton);

			buildTypeFilterComboBox = new ToolbarPopUpButtonItem (toolbar.Properties, nameof (buildTypeFilterComboBox));
			buildTypeFilterComboBox.Menu = CreateBuildTypeFilterMenu ();
			toolbar.AddItem (buildTypeFilterComboBox);

			searchEntry = new ToolbarSearchItem (toolbar.Properties, nameof (searchEntry));
			searchEntry.Position = ToolbarItemPosition.Trailing;
			searchEntry.Label = GettextCatalog.GetString ("Search");
			searchEntry.TextChanged += SearchEntryChanged;
			toolbar.AddItem (searchEntry);

			window.SetToolbar (toolbar, DockPositionType.Top);

			// Workaround being unable to set the button's Enabled state before the underlying
			// NSView is created. It is created after SetToolbar is called.
			stopButton.Enabled = false;

			ProjectSystemService.MSBuildTargetStarted += MSBuildTargetStarted;
			ProjectSystemService.MSBuildTargetFinished += MSBuildTargetFinished;
		}

		ContextMenu CreateBuildTypeFilterMenu ()
		{
			string[] buildTypeItems = buildTypes
				.Select (buildType => GetDisplayText (buildType))
				.ToArray ();

			buildTypeFilterMenu = new ContextMenu ();

			foreach (BuildType buildType in buildTypes) {
				var menuItem = new ContextMenuItem ();
				menuItem.Label = GetDisplayText (buildType);
				menuItem.Context = buildType;
				menuItem.Clicked += BuildTypeFilterComboBoxChanged;

				buildTypeFilterMenu.Items.Add (menuItem);
			}

			return buildTypeFilterMenu;
		}

		void OnStartButtonClicked (object sender, EventArgs e)
		{
			ProjectSystemService.IsEnabled = true;
			startButton.Enabled = false;
			stopButton.Enabled = true;
		}

		void OnStopButtonClicked (object sender, EventArgs e)
		{
			ProjectSystemService.IsEnabled = false;
			startButton.Enabled = true;
			stopButton.Enabled = false;
		}

		void OnClearButtonClicked (object sender, EventArgs e)
		{
			widget.ClearItems ();
		}

		void MSBuildTargetStarted (object sender, MSBuildTargetEventArgs e)
		{
			Runtime.AssertMainThread ();

			widget.AddMSBuildTarget (e.BuildTarget);
		}

		void MSBuildTargetFinished (object sender, MSBuildTargetEventArgs e)
		{
			Runtime.AssertMainThread ();

			widget.UpdateMSBuildTarget (e.BuildTarget);
		}

		static string GetDisplayText (BuildType buildType)
		{
			switch (buildType) {
				case BuildType.All:
					return GettextCatalog.GetString ("All");
				case BuildType.Build:
					return GettextCatalog.GetString ("Builds");
				case BuildType.DesignTimeBuild:
					return GettextCatalog.GetString ("Design-time Builds");
				default:
					return buildType.ToString ();
			}
		}

		void BuildTypeFilterComboBoxChanged (object sender, ContextMenuItemClickedEventArgs e)
		{
			widget.BuildType = (BuildType)e.Context;
		}

		void SearchEntryChanged (object sender, EventArgs e)
		{
			DisposeExistingTimer ();
			searchTimer = Xwt.Application.TimeoutInvoke (searchDelayTimeSpan, Search);
		}

		bool Search ()
		{
			widget.SearchFilter = searchEntry.Text;
			return false;
		}

		void DisposeExistingTimer ()
		{
			if (searchTimer != null) {
				searchTimer.Dispose ();
			}
		}
	}
}
