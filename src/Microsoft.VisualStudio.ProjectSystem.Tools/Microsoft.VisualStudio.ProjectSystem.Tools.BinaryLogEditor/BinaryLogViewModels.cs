//
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
//
// Based on:
// https://github.com/dotnet/project-system-tools/blob/e5c00e55fe7f0d5cf0cdd249475c570b2a633d19/src/ProjectSystemTools/BinaryLogEditor/BinaryLogEditorPane.cs

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.ProjectSystem.LogModel;
using Microsoft.VisualStudio.ProjectSystem.Tools.BinaryLogEditor.ViewModel;

namespace Microsoft.VisualStudio.ProjectSystem.Tools.BinaryLogEditor
{
	internal class BinaryLogViewModels
	{
		public BinaryLogViewModels (BinaryLogDocumentData binaryLogDocument)
		{
			//buildTreeViewItems = new ObservableCollection<BaseViewModel> ();
			TargetListViewItems = new ObservableCollection<TargetListViewModel> ();
			TaskListViewItems = new ObservableCollection<TaskListViewModel> ();
			EvaluationListViewItems = new ObservableCollection<EvaluationListViewModel> ();

			BinaryLogDocument = binaryLogDocument;
		}

		public ObservableCollection<EvaluationListViewModel> EvaluationListViewItems { get; }
		public ObservableCollection<TargetListViewModel> TargetListViewItems { get; }
		public ObservableCollection<TaskListViewModel> TaskListViewItems { get; }
		public BinaryLogDocumentData BinaryLogDocument { get; }

		public void Build ()
		{
			//if (documentData.Log.Exceptions.Any ()) {
			//	buildTreeViewItems.Add (new ListViewModel<Exception> ("Exceptions", documentData.Log.Exceptions, ex => new ExceptionViewModel (ex)));
			//}

			//if (documentData.Log.Evaluations.Any ()) {
			//	evaluationTreeViewItems.Add (new ListViewModel<Evaluation> ($"Evaluations ({documentData.Log.Evaluations.SelectMany (e => e.EvaluatedProjects).Aggregate (TimeSpan.Zero, (t, p) => t + (p.EndTime - p.StartTime)):mm':'ss'.'ffff})", documentData.Log.Evaluations, e => e.EvaluatedProjects.Count == 1
			//			? (BaseViewModel)new EvaluatedProjectViewModel (e)
			//			: new EvaluationViewModel (e)));
			//}

			if (BinaryLogDocument.Log.Build?.Projects != null) {
				//buildTreeViewItems.Add (new BuildViewModel (documentData.Log.Build));

				var allTargets = CollectTargets (BinaryLogDocument.Log.Build.Projects);
				var groupedTargets = allTargets.GroupBy (target => Tuple.Create (target.Name, target.SourceFilePath));
				var totalTime = BinaryLogDocument.Log.Build.EndTime - BinaryLogDocument.Log.Build.StartTime;
				foreach (var groupedTarget in groupedTargets) {
					var time = groupedTarget.Aggregate (TimeSpan.Zero, (current, target) => current + (target.EndTime - target.StartTime));
					TargetListViewItems.Add (new TargetListViewModel (
						groupedTarget.Key.Item1,
						groupedTarget.Key.Item2,
						groupedTarget.Count (),
						time,
						time.Ticks / (double)totalTime.Ticks));
				}

				var allTasks = CollectTasks (BinaryLogDocument.Log.Build.Projects);
				var groupedTasks = allTasks.GroupBy (task => Tuple.Create (task.Name, task.SourceFilePath));
				foreach (var groupedTask in groupedTasks) {
					var time = groupedTask.Aggregate (TimeSpan.Zero, (current, task) => current + (task.EndTime - task.StartTime));
					TaskListViewItems.Add (new TaskListViewModel (
						groupedTask.Key.Item1,
						groupedTask.Key.Item2,
						groupedTask.Count (),
						time,
						time.Ticks / (double)totalTime.Ticks));
				}

				var allEvaluations = CollectEvaluations (
					BinaryLogDocument.Log.Evaluations
						.SelectMany (e => e.EvaluatedProjects)
						.Select (p => p.EvaluationProfile)
						.Where (p => p != null)
						.SelectMany (p => p.Passes)
						.SelectMany (p => p.Locations)).ToList ();
				var totalEvaluationTime = allEvaluations.Aggregate (TimeSpan.Zero, (current, e) => current + e.Time.ExclusiveTime);
				var groupedEvaluations =
					allEvaluations.GroupBy (e => Tuple.Create (e.ElementName, e.Kind, e.File, e.Line));
				foreach (var groupedEvaluation in groupedEvaluations) {
					var time = groupedEvaluation.Aggregate (TimeSpan.Zero, (current, e) => current + e.Time.ExclusiveTime);
					EvaluationListViewItems.Add (new EvaluationListViewModel (
						groupedEvaluation.Key.Item1,
						groupedEvaluation.First ().ElementDescription,
						groupedEvaluation.Key.Item2.ToString (),
						groupedEvaluation.Key.Item3,
						groupedEvaluation.Key.Item4,
						groupedEvaluation.Count (),
						time,
						time.Ticks / (double)totalEvaluationTime.Ticks));
				}
			}
		}

		static IEnumerable<Target> CollectTargets (IEnumerable<Project> projects)
		{
			var allTargets = new List<Target> ();
			foreach (var targets in projects.Select (p => CollectTargets (p))) {
				allTargets.AddRange (targets);
			}
			return allTargets;
		}

		static IEnumerable<Target> CollectTargets (Project project)
		{
			return project.Targets.Union (
				project.Targets.SelectMany (t => t.Tasks)
					.SelectMany (t => t.ChildProjects)
					.SelectMany (CollectTargets));
		}

		static IEnumerable<Task> CollectTasks (IEnumerable<Project> projects)
		{
			var allTasks = new List<Task> ();
			foreach (var tasks in projects.Select (p => CollectTasks (p))) {
				allTasks.AddRange (tasks);
			}
			return allTasks;
		}

		static IEnumerable<Task> CollectTasks (Project project)
		{
			return project.Targets.SelectMany (target => target.Tasks)
				.Union (
					project.Targets.SelectMany (t => t.Tasks)
					.SelectMany (t => t.ChildProjects)
					.SelectMany (CollectTasks));
		}

		static IEnumerable<EvaluatedLocation> CollectEvaluations (IEnumerable<EvaluatedLocation> locations)
		{
			if (!locations.Any ())
				return locations;

			return locations.Union (CollectEvaluations (locations.SelectMany (l => l.Children)));
		}
	}
}
