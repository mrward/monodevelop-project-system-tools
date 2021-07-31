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
using Gtk;
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
		PadToolbarButtonItem startButton;
		PadToolbarButtonItem stopButton;
		PadToolbarButtonItem clearButton;
		ComboBox buildTypeFilterComboBox;
		PadToolbarSearchItem searchEntry;
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
			buildTypeFilterComboBox.Changed -= BuildTypeFilterComboBoxChanged;
			searchEntry.TextChanged -= SearchEntryChanged;

			DisposeExistingTimer ();

			widget.Dispose ();

			base.Dispose ();
		}

		protected override void Initialize (IPadWindow window)
		{
			var toolbar = new PadToolbar ();

			startButton = new PadToolbarButtonItem (toolbar.Properties, nameof (startButton));
			startButton.Icon = Ide.Gui.Stock.RunProgramIcon;
			startButton.Clicked += OnStartButtonClicked;
			startButton.Tooltip = GettextCatalog.GetString ("Enable build logging");
			toolbar.AddItem (startButton);

			stopButton = new PadToolbarButtonItem (toolbar.Properties, nameof (stopButton));
			stopButton.Icon = Ide.Gui.Stock.Stop;
			stopButton.Clicked += OnStopButtonClicked;
			stopButton.Tooltip = GettextCatalog.GetString ("Stop build logging");
			stopButton.Enabled = false;
			toolbar.AddItem (stopButton);

			clearButton = new PadToolbarButtonItem (toolbar.Properties, nameof (clearButton));
			clearButton.Icon = Ide.Gui.Stock.Clear;
			clearButton.Clicked += OnClearButtonClicked;
			clearButton.Tooltip = GettextCatalog.GetString ("Clear");
			toolbar.AddItem (clearButton);

			//string[] buildTypeItems = buildTypes
			//	.Select (buildType => GetDisplayText (buildType))
			//	.ToArray ();
			//buildTypeFilterComboBox = new ComboBox (buildTypeItems);
			//buildTypeFilterComboBox.Active = 0;
			//buildTypeFilterComboBox.Changed += BuildTypeFilterComboBoxChanged;
			//toolbar.Add (buildTypeFilterComboBox);

			searchEntry = new PadToolbarSearchItem (toolbar.Properties, nameof (searchEntry));
			searchEntry.Position = PadToolbarItemPosition.Trailing;
			searchEntry.Label = GettextCatalog.GetString ("Search");
			searchEntry.TextChanged += SearchEntryChanged;
			toolbar.AddItem (searchEntry);

			window.SetToolbar (toolbar, DockPositionType.Top);

			ProjectSystemService.MSBuildTargetStarted += MSBuildTargetStarted;
			ProjectSystemService.MSBuildTargetFinished += MSBuildTargetFinished;
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

		void BuildTypeFilterComboBoxChanged (object sender, EventArgs e)
		{
			widget.BuildType = buildTypes [buildTypeFilterComboBox.Active];
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
