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
using Gtk;
using MonoDevelop.Components;
using MonoDevelop.Components.Docking;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	public class BuildLoggingPad : PadContent
	{
		BuildLoggingWidget widget;
		XwtControl control;
		Button startButton;
		Button stopButton;
		Button clearButton;

		public override Control Control {
			get {
				if (control == null) {
					widget = new BuildLoggingWidget ();
					control = new XwtControl (widget);
				}
				return control;
			}
		}

		public override void Dispose ()
		{
			startButton.Clicked -= OnStartButtonClicked;
			stopButton.Clicked -= OnStopButtonClicked;
			clearButton.Clicked -= OnClearButtonClicked;

			BuildLoggingService.MSBuildTargetStarted -= MSBuildTargetStarted;
			BuildLoggingService.MSBuildTargetFinished -= MSBuildTargetFinished;

			widget.Dispose ();

			base.Dispose ();
		}

		protected override void Initialize (IPadWindow window)
		{
			var toolbar = window.GetToolbar (DockPositionType.Top);

			startButton = new Button (new ImageView (Ide.Gui.Stock.RunProgramIcon, IconSize.Menu));
			startButton.Clicked += OnStartButtonClicked;
			startButton.TooltipText = GettextCatalog.GetString ("Enable build logging");
			toolbar.Add (startButton);

			stopButton = new Button (new ImageView (Ide.Gui.Stock.Stop, IconSize.Menu));
			stopButton.Clicked += OnStopButtonClicked;
			stopButton.TooltipText = GettextCatalog.GetString ("Stop build logging");
			stopButton.Sensitive = false;
			toolbar.Add (stopButton);

			clearButton = new Button (new ImageView (Ide.Gui.Stock.Broom, IconSize.Menu));
			clearButton.Clicked += OnClearButtonClicked;
			clearButton.TooltipText = GettextCatalog.GetString ("Clear");
			toolbar.Add (clearButton);

			toolbar.ShowAll ();

			BuildLoggingService.MSBuildTargetStarted += MSBuildTargetStarted;
			BuildLoggingService.MSBuildTargetFinished += MSBuildTargetFinished;
		}

		void OnStartButtonClicked (object sender, EventArgs e)
		{
			BuildLoggingService.IsEnabled = true;
			startButton.Sensitive = false;
			stopButton.Sensitive = true;
		}

		void OnStopButtonClicked (object sender, EventArgs e)
		{
			BuildLoggingService.IsEnabled = false;
			startButton.Sensitive = true;
			stopButton.Sensitive = false;
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
	}
}
