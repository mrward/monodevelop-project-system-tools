# Project System Tools for MonoDevelop and Visual Studio for Mac

Inspired by [Project System Tools for Visual Studio on Windows](https://github.com/dotnet/project-system-tools).

## Build Logging Window

To open the Build Logging Window

  - Select View - Pads - Build Logging

Click the green arrow button to enable the logging of the MSBuild targets for builds and design-time builds.

![Build Logging Window](doc/images/BuildLoggingWindow.png)

To stop the logging click the red square stop button.

To filter the targets either use the combo box to restrict the targets to builds or design time builds, or use the search, on the right hand side of the window, to filter the targets displayed.

To open the log output for an MSBuild target

  - Double click the row.
  - Right click the row and select Open Log File

![Open Log File context menu](doc/images/BuildLoggingOpenLogFileMenu.png)

![MSBuild Log File](doc/images/MSBuildLogFile.png)

To open a binary log into the binary log viewer:

  - Right click the row and Open Binary Log File

Note that not all MSBuild targets currently support generating a binary log.

![MSBuild Binary Log File - Build tab](doc/images/MSBuildBinaryLogFileBuildTab.png)

Three tabs are available when a binlog file is opened.

  - Build
    - Tree view of the binary log targets and tasks
    - Properties for a target or task are shown in the Properties window.
  - Target Summary
     - Target name
     - Source filename
     - Number of calls
     - Time taken
     - Percentage of total time taken
  - Task Summary
     - Task name
     - Source filename
     - Number of calls
     - Time taken
     - Percentage of total time taken

![MSBuild Binary Log File - Target Summary tab](doc/images/TargetSummaryTab.png)

![MSBuild Binary Log File - Task Summary tab](doc/images/TaskSummaryTab.png)

Selecting a tree node in the Build tab will show properties for the node in the Properties window.

![MSBuild Binary Log File - Build tab - Properties window](doc/images/BuildTabProperties.png)

