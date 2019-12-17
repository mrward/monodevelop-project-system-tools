//
// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.
//
// Based on:
// https://github.com/dotnet/project-system-tools/blob/master/src/ProjectSystemTools/BinaryLogEditor/BinaryLogDocumentData.cs

using System;
using System.Collections.Immutable;
using Microsoft.Build.Logging;
using Microsoft.VisualStudio.ProjectSystem.LogModel;
using Microsoft.VisualStudio.ProjectSystem.LogModel.Builder;

namespace Microsoft.VisualStudio.ProjectSystem.Tools.BinaryLogEditor
{
	internal class BinaryLogDocumentData
	{
		public Log Log { get; private set; }

		public event EventHandler Loaded;

		public void Load (string fileName)
		{
			try {
				var replayEventSource = new BinaryLogReplayEventSource ();
				var builder = new ModelBuilder (replayEventSource);
				replayEventSource.Replay (fileName);
				Log = builder.Finish ();
			} catch (Exception ex) {
				if (ex is AggregateException aggregateException) {
					Log = new Log (null, ImmutableList<Evaluation>.Empty, aggregateException.InnerExceptions.ToImmutableList ());
				} else {
					Log = new Log (null, ImmutableList<Evaluation>.Empty, new[] { ex }.ToImmutableList ());
				}
			}

			Loaded?.Invoke (this, new EventArgs ());
		}
	}
}
