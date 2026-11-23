# Syncfusion UWP Gantt Chart - Data Binding and Event Handling

This sample shows how task data is supplied to the Syncfusion UWP Gantt Chart (`SfGantt`) and how the application receives events from it. It binds a three-level task hierarchy with dependencies, resources, milestones, critical path and a holiday. Every Gantt event raised by user interaction is written to a real-time Event Logger panel.

## Features

### 1. **Hierarchical Data Binding**
Tasks are supplied as an `ObservableCollection<TaskDetail>`. The hierarchy is built with each task's `Children` collection.
- **Code Snippet:**
  ```csharp
  Schedule[0].Children = ScheduleProcess;     // Product Launch -> phases
  ScheduleProcess[0].Children = Planning;     // Phase 1 -> tasks
  ```
  ```xml
  <gantt:SfGantt ItemsSource="{Binding TaskCollection}" />
  ```
- **File:** `MainPage.xaml`, `MainPage.xaml.cs` (`ProjectViewModel.GetData`)
- **Documentation:** [Getting Started with UWP Gantt Chart](https://help.syncfusion.com/uwp/gantt/getting-started)

### 2. **Task Dependencies**
Dependencies are added to a task's `Predecessors` collection (Finish to Start).
- **Code Snippet:**
  ```csharp
  Design[2].Predecessors.Add(new TaskRelationship() { ID = "7", Relationship = Relationship.FinishToStart });
  ```
- **File:** `MainPage.xaml.cs` (`ProjectViewModel.GetData`)
- **Documentation:** [Task relationships](https://help.syncfusion.com/uwp/gantt/getting-started)

### 3. **Resources**
Resources are bound to `ProjectResources` and assigned to tasks by resource ID.
- **Code Snippet:**
  ```csharp
  Resources.Add(new GanttResource { ID = "2", Name = "Developer" });
  Development[0].Resources.Add("2");
  ```
  ```xml
  <gantt:SfGantt ProjectResources="{Binding ResourceCollection}" />
  ```
- **File:** `MainPage.xaml.cs` (`GetResources`)
- **Documentation:** [Resources](https://help.syncfusion.com/uwp/gantt/getting-started)

### 4. **Critical Path**
Highlights the chain of linked tasks that defines the project finish date.
- **Code Snippet:**
  ```xml
  <gantt:SfGantt HighlightCriticalTasks="True" CriticalTaskBarBrush="Red" />
  ```
- **File:** `MainPage.xaml`
- **Documentation:** [Critical Path in UWP Gantt Chart](https://help.syncfusion.com/uwp/gantt/criticalpath)

### 5. **Holidays and Non-working Days**
Shades weekends and the defined holiday on the timeline. `ShowNonWorkingDays` must be enabled.
- **Code Snippet:**
  ```csharp
  Gantt.Holidays = new GanttHolidayCollection
  {
      new GanttHoliday { Day = new DateTime(2026, 11, 26), Background = new SolidColorBrush(Colors.CadetBlue) }
  };
  ```
  ```xml
  <gantt:SfGantt ShowNonWorkingDays="True" />
  ```
- **File:** `MainPage.xaml`, `MainPage.xaml.cs` (constructor)
- **Documentation:** [Holidays in UWP Gantt Chart](https://help.syncfusion.com/uwp/gantt/holidays)

### 6. **Timescale**
Weeks on the top tier and days on the bottom tier.
- **Code Snippet:**
  ```xml
  <gantt:TimescaleTier IntervalType="Weeks" />
  <gantt:TimescaleTier IntervalType="Days" />
  ```
- **File:** `MainPage.xaml`
- **Documentation:** [Timescale in UWP Gantt Chart](https://help.syncfusion.com/uwp/gantt/timescale)

### 7. **Event Handling and Event Logger**
Events are subscribed in XAML and in code. Each handler writes a log entry with Time, Event, Task ID, Task Name, and Old and New values where the event provides them. The panel has **Clear Log** and **Copy Log** buttons.
- **Code Snippet:**
  ```csharp
  Gantt.TaskBarDragCompleted += Gantt_TaskBarDragCompleted;

  private void Gantt_TaskBarDragCompleted(object sender, TaskBarDragCompletedEventArgs e)
  {
      Write("TaskBarDragCompleted", e.Task,
            D(e.OldStartDate) + " to " + D(e.OldFinishDate), Range(e.Task as TaskDetail));
  }
  ```
- **File:** `MainPage.xaml`, `MainPage.xaml.cs`
- **Documentation:** [Events in UWP Gantt Chart](https://help.syncfusion.com/uwp/gantt/events)

## Events Handled

| Area | Events |
|---|---|
| Selection | `SelectionChanging`, `SelectionChanged` |
| Cell editing | `BeginCellEdit`, `EndCellEdit` |
| Drag and drop | `TaskBarDragStart`, `TaskBarDragDelta`, `TaskBarDragCompleted` |
| Resize | `TaskBarResizeStart`, `TaskBarResizeDelta`, `TaskBarResizeCompleted` |
| Progress | `TaskBarProgressChanging`, `TaskBarProgressChanged` |
| Dependency | `TaskRelationshipEstablished` |
| Expand and collapse | `TaskExpanding`, `TaskExpanded`, `TaskCollapsing`, `TaskCollapsed` |

N> Only dependency creation raises an event. No separate events are documented for modifying or deleting a dependency.

## Project Structure

```
UWPGanttSample/
├── MainPage.xaml          # SfGantt and the Event Logger panel
├── MainPage.xaml.cs       # Event handlers, logger, holidays, and ProjectViewModel (tasks and resources)
├── App.xaml               # Application entry
├── Package.appxmanifest   # UWP package manifest
└── README.md              # This file
```

## Sample Data

| Level | Content |
|---|---|
| 1 | Product Launch (ID 1) |
| 2 | Five phases: Planning, Design, Development, Testing, Release |
| 3 | 12 tasks and milestones (Nov 2 - Dec 18, 2026) |

- Milestones: Planning complete, Design complete, Code complete, Go live.
- Resources: Project Manager, Developer, Tester.
- Dependencies: 12 Finish-to-Start links.

## Technology Stack

| Technology | Purpose |
|-----------|---------|
| Universal Windows Platform (UWP) | Application framework |
| C# | Application language |
| Visual Studio | IDE |
| Syncfusion.SfGantt.UWP (NuGet) | Gantt chart control |

## How to Run

### Prerequisites
- Visual Studio with the **Universal Windows Platform development** workload
- Syncfusion Essential Studio for UWP (license key registered if required)

### Steps
1. Open the solution in Visual Studio.
2. Restore NuGet packages (`Syncfusion.SfGantt.UWP` and its dependencies).
3. Select the **x64** (or your platform) configuration and **Local Machine**.
4. Press **F5** to build and run.

## Try It

| Action | Expected result in the Event Logger |
|---|---|
| Click a task | `SelectionChanging`, `SelectionChanged` |
| Edit a cell | `BeginCellEdit`, `EndCellEdit` |
| Drag a task bar | `TaskBarDragStart`, `TaskBarDragDelta`, `TaskBarDragCompleted` |
| Resize a task bar | `TaskBarResizeStart`, `TaskBarResizeDelta`, `TaskBarResizeCompleted` |
| Drag the progress handle | `TaskBarProgressChanging`, `TaskBarProgressChanged` |
| Link two tasks | `TaskRelationshipEstablished` |
| Expand or collapse a phase | `TaskExpanding`/`TaskExpanded`, `TaskCollapsing`/`TaskCollapsed` |

## License

Syncfusion Community License - See the Syncfusion website for details.

## Support

- Syncfusion UWP Gantt documentation: https://help.syncfusion.com/uwp/gantt/getting-started