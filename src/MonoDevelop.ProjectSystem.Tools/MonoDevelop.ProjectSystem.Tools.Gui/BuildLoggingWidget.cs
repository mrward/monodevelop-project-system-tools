//
// BuildLoggingWidget.cs
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
using System.IO;
using MonoDevelop.Components;
using MonoDevelop.Components.Commands;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;
using Xwt;

namespace MonoDevelop.ProjectSystem.Tools.Gui
{
	partial class BuildLoggingWidget
	{
		BuildType buildType = BuildType.All;
		string searchFilter = string.Empty;
		List<MSBuildTarget> msbuildTargets = new List<MSBuildTarget> ();

		public BuildLoggingWidget ()
		{
			Build ();

			listView.RowActivated += ListViewRowActivated;
			listView.ButtonPressed += ListViewButtonPressed;
		}

		public void ClearItems ()
		{
			DeleteMSBuildOutputFiles ();

			msbuildTargets.Clear ();
			listStore.Clear ();
		}

		public void AddMSBuildTarget (MSBuildTarget target)
		{
			msbuildTargets.Add (target);

			if (IsAllowedByFilter (target)) {
				AddMSBuildTargetToListView (target);
			}
		}

		bool IsAllowedByFilter (MSBuildTarget target)
		{
			if (buildType != BuildType.All &&
				buildType != target.BuildType) {
				return false;
			}

			return IsAllowedBySearchFilter (target);
		}

		void AddMSBuildTargetToListView (MSBuildTarget target)
		{
			int row = listStore.AddRow ();

			listStore.SetValues (
				row,

				projectDataField,
				target.ProjectFileName.FileName,

				dimensionsDataField,
				target.Dimensions,

				elapsedDataField,
				GetDisplayText (target.Duration),

				targetsDataField,
				target.Targets,

				typeDataField,
				target.BuildType.ToString (),

				startDataField,
				GetDisplayText (target.StartTime),

				statusDataField,
				target.Status.GetDisplayText (),

				msbuildTargetDataField,
				target);
		}

		static string GetDisplayText (TimeSpan? duration)
		{
			if (!duration.HasValue) {
				return string.Empty;
			}

			return duration.Value.TotalSeconds.ToString ("N3");
		}

		static string GetDisplayText (DateTime time)
		{
			return time.ToString ("s");
		}

		public void UpdateMSBuildTarget (MSBuildTarget target)
		{
			for (int row = listStore.RowCount - 1; row >= 0; --row) {
				MSBuildTarget currentTarget = listStore.GetValue (row, msbuildTargetDataField);
				if (target == currentTarget) {
					listStore.SetValues (
						row,

						elapsedDataField,
						GetDisplayText (target.Duration),

						statusDataField,
						target.Status.GetDisplayText ());

					return;
				}
			}
		}

		protected override void Dispose (bool disposing)
		{
			base.Dispose (disposing);

			if (disposing) {
				listView.RowActivated -= ListViewRowActivated;
				listView.ButtonPressed -= ListViewButtonPressed;

				DeleteMSBuildOutputFiles ();
			}
		}

		void DeleteMSBuildOutputFiles ()
		{
			MSBuildTarget target = null;

			for (int row = 0; row < listStore.RowCount; ++row) {
				try {
					target = listStore.GetValue (row, msbuildTargetDataField);

					if (target.LogFileName.IsNotNull) {
						File.Delete (target.LogFileName);
					}

					if (target.BinLogFileName.IsNotNull) {
						File.Delete (target.BinLogFileName);
					}
				} catch (Exception ex) {
					LoggingService.LogError (
						string.Format ("Unable to remove msbuild output file {0}", target),
						ex);
				}
			}
		}

		void ListViewRowActivated (object sender, ListViewRowEventArgs e)
		{
			OpenLogFileForRow (e.RowIndex);
		}

		void OpenLogFileForRow (int row, bool binLog = false)
		{
			MSBuildTarget target = GetMSBuildTargetForRow (row);

			FilePath fileNameToOpen = binLog ? target.BinLogFileName : target.LogFileName;

			if (fileNameToOpen.IsNotNull && File.Exists (fileNameToOpen)) {
				IdeApp.Workbench.OpenDocument (fileNameToOpen, (Project)null)
					.Ignore ();
			}
		}

		MSBuildTarget GetMSBuildTargetForSelectedRow ()
		{
			return GetMSBuildTargetForRow (listView.SelectedRow);
		}

		MSBuildTarget GetMSBuildTargetForRow (int row)
		{
			if (row < 0) {
				return null;
			}

			return listStore.GetValue (row, msbuildTargetDataField);
		}

		void ListViewButtonPressed (object sender, ButtonEventArgs e)
		{
			if (!e.IsContextMenuTrigger) {
				return;
			}

			var commands = IdeApp.CommandService.CreateCommandEntrySet ("/MonoDevelop/BuildLoggingPad/ContextMenu");
			IdeApp.CommandService.ShowContextMenu (listView.ToGtkWidget (), (int)e.X, (int)e.Y, commands, this);
		}

		[CommandUpdateHandler (BuildLoggingCommands.OpenLogFile)]
		void OnUpdateOpenLogFile (CommandInfo info)
		{
			info.Enabled = MSBuildLogFileExists ();
		}

		[CommandHandler (BuildLoggingCommands.OpenLogFile)]
		void OpenLogFile ()
		{
			if (IsMSBuildTargetSelected ()) {
				OpenLogFileForRow (listView.SelectedRow);
			}
		}

		[CommandUpdateHandler (BuildLoggingCommands.OpenBinLogFile)]
		void OnUpdateOpenBinLogFile (CommandInfo info)
		{
			info.Enabled = MSBuildBinLogFileExists ();
		}

		[CommandHandler (BuildLoggingCommands.OpenBinLogFile)]
		void OpenBinLogFile ()
		{
			if (IsMSBuildTargetSelected ()) {
				OpenLogFileForRow (listView.SelectedRow, binLog: true);
			}
		}

		bool MSBuildLogFileExists ()
		{
			MSBuildTarget target = GetMSBuildTargetForSelectedRow ();

			return target != null &&
				target.LogFileName.IsNotNull &&
				File.Exists (target.LogFileName);
		}

		bool MSBuildBinLogFileExists ()
		{
			MSBuildTarget target = GetMSBuildTargetForSelectedRow ();

			if (target == null) {
				return false;
			}

			target.CopyBinLogFile ();

			return target.BinLogFileName.IsNotNull &&
				File.Exists (target.BinLogFileName);
		}

		bool IsMSBuildTargetSelected ()
		{
			return listView.SelectedRow >= 0;
		}

		public BuildType BuildType {
			get { return buildType; }
			set {
				buildType = value;
				ApplyFilter ();
			}
		}

		public string SearchFilter {
			get { return searchFilter; }
			set {
				searchFilter = value;
				ApplyFilter ();
			}
		}

		void ApplyFilter ()
		{
			listStore.Clear ();

			foreach (MSBuildTarget target in msbuildTargets) {
				if (IsAllowedByFilter (target)) {
					AddMSBuildTargetToListView (target);
				}
			}
		}

		bool IsAllowedBySearchFilter (MSBuildTarget target)
		{
			if (string.IsNullOrEmpty (searchFilter)) {
				return true;
			}

			return
				IsSearchFilterMatch (target.BuildType.ToString ()) ||
				IsSearchFilterMatch (target.Dimensions) ||
				IsSearchFilterMatch (target.ProjectFileName.FileName) ||
				IsSearchFilterMatch (target.ProjectName) ||
				IsSearchFilterMatch (target.Status.GetDisplayText ()) ||
				IsSearchFilterMatch (target.Targets) ||
				IsSearchFilterMatch (GetDisplayText (target.Duration)) ||
				IsSearchFilterMatch (GetDisplayText (target.StartTime));
		}

		bool IsSearchFilterMatch (string text)
		{
			if (string.IsNullOrEmpty (text)) {
				return false;
			}

			return text.IndexOf (searchFilter, StringComparison.OrdinalIgnoreCase) >= 0;
		}
	}
}
