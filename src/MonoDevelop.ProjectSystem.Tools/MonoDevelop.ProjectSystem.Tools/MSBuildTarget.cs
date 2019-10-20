//
// MSBuildTarget.cs
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
using MonoDevelop.Core;
using MonoDevelop.Projects;

namespace MonoDevelop.ProjectSystem.Tools
{
	class MSBuildTarget
	{
		public string Targets { get; set; }
		public string ProjectName { get; set; }
		public string ProjectFileName { get; set; }
		public string BuildType { get; set; }
		public string Dimensions { get; set; }

		public DateTime StartTime { get; set; }
		public TimeSpan Duration { get; set; }

		public MSBuildTargetStatus Status { get; set; }

		public FilePath LogFileName { get; private set; }

		public FilePath BuildSessionBinLogFileName { get; set; }
		public FilePath BinLogFileName { get; private set; }

		public static string GetBuildType (string target)
		{
			if (string.IsNullOrEmpty (target)) {
				return string.Empty;
			}

			if (target == ProjectService.BuildTarget ||
				target == ProjectService.CleanTarget ||
				target == "Pack") {
				return "Build";
			}

			return "DesignTimeBuild";
		}

		public void GenerateLogFileName ()
		{
			string fileNameWithoutFileExtension = Path.Combine (Path.GetTempPath (), GetLogFileNamePrefix ());

			LogFileName = fileNameWithoutFileExtension + ".msbuild.log";
			BinLogFileName = fileNameWithoutFileExtension + ".binlog";
		}

		/// <summary>
		/// Get a unique log file name. Based on:
		/// https://github.com/dotnet/project-system-tools/blob/7eb2f653890c159f750781de56bc7d261e7f4255/src/ProjectSystemTools/BuildLogging/Model/LoggerBase.cs#L22
		/// </summary>
		string GetLogFileNamePrefix ()
		{
			string fileName = string.Format (
				"{0}_{1}{2}_{3}",
				ProjectName,
				GetDimensionLogFileNamePart (),
				BuildType,
				StartTime.ToString ("o")
			);
			return fileName.Replace (':', '_');
		}

		string GetDimensionLogFileNamePart ()
		{
			if (string.IsNullOrEmpty (Dimensions)) {
				return string.Empty;
			}

			return Dimensions.Replace ('|', '_') + "_";
		}

		public void CopyBinLogFile ()
		{
			if (BuildSessionBinLogFileName.IsNull) {
				return;
			}

			try {
				File.Copy (BuildSessionBinLogFileName, BinLogFileName);
			} catch (Exception ex) {
				LoggingService.LogError (
					string.Format ("Unable to copy bin log file {0}", BuildSessionBinLogFileName),
					ex);
			}

			BuildSessionBinLogFileName = FilePath.Null;
		}
	}
}
