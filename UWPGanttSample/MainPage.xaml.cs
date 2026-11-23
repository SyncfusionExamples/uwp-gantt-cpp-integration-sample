using System;
using System.Collections.ObjectModel;
using System.Text;
using Syncfusion.UI.Xaml.Gantt;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// Both Syncfusion.UI.Xaml.Gantt and Windows.UI.Xaml.Controls define these names; pick the Gantt ones.
using GanttSelectionChangedEventArgs = Syncfusion.UI.Xaml.Gantt.SelectionChangedEventArgs;
using GanttSelectionChangingEventArgs = Syncfusion.UI.Xaml.Gantt.SelectionChangingEventArgs;

namespace UWPGanttSample
{
    public class LogEntry { public string Text { get; set; } }

    public sealed partial class MainPage : Page
    {
        // Bound to the Event Logger ListView with x:Bind.
        public ObservableCollection<LogEntry> Log { get; } = new ObservableCollection<LogEntry>();

        public MainPage()
        {
            this.InitializeComponent();

            // Supplies TaskCollection / ResourceCollection to the Gantt bindings.
            this.DataContext = new ProjectViewModel();

            // Holidays are set in code: a GanttHoliday inside a collection is not in the visual tree,
            // so a {Binding} on its Day property would not resolve. Requires ShowNonWorkingDays="True".
            Gantt.Holidays = new GanttHolidayCollection
            {
                new GanttHoliday
                {
                    Day = new DateTime(2026, 11, 26),            // Thanksgiving
                    Background = new SolidColorBrush(Colors.CadetBlue)
                }
            };

            // Events subscribed in code (equivalent to the XAML attributes).
            // Handlers receive the task in e.Task / e.Item; put business logic there.
            Gantt.TaskBarDragStart += Gantt_TaskBarDragStart;
            Gantt.TaskBarDragDelta += Gantt_TaskBarDragDelta;
            Gantt.TaskBarDragCompleted += Gantt_TaskBarDragCompleted;
            Gantt.TaskBarResizeStart += Gantt_TaskBarResizeStart;
            Gantt.TaskBarResizeDelta += Gantt_TaskBarResizeDelta;
            Gantt.TaskBarResizeCompleted += Gantt_TaskBarResizeCompleted;
            Gantt.TaskBarProgressChanging += Gantt_TaskBarProgressChanging;
            Gantt.TaskBarProgressChanged += Gantt_TaskBarProgressChanged;
            Gantt.TaskRelationshipEstablished += Gantt_TaskRelationshipEstablished;
        }

        // ---- Logging helper: Time / Event / Task ID / Task Name / Old / New ----
        private void Write(string evt, object task, string oldValue = null, string newValue = null)
        {
            var t = task as TaskDetail;
            var sb = new StringBuilder();
            sb.AppendLine("Time: " + DateTime.Now.ToString("hh:mm:ss tt"));
            sb.AppendLine("Event: " + evt);
            sb.AppendLine("Task ID: " + (t?.ID ?? "-"));
            sb.AppendLine("Task Name: " + (t?.Name ?? "-"));
            if (oldValue != null) sb.AppendLine("Old Value: " + oldValue);
            if (newValue != null) sb.AppendLine("New Value: " + newValue);
            Log.Insert(0, new LogEntry { Text = sb.ToString() });
        }

        private static string D(DateTime d) { return d.ToString("dd-MMM-yyyy"); }
        private static string Range(TaskDetail t) { return t == null ? null : D(t.StartDate) + " to " + D(t.FinishDate); }

        // ---- Selection ----
        private void Gantt_SelectionChanging(object sender, GanttSelectionChangingEventArgs e)
        {
            Write("SelectionChanging (index " + e.Index + ")", e.Item);
        }
        private void Gantt_SelectionChanged(object sender, GanttSelectionChangedEventArgs e)
        {
            Write("SelectionChanged (index " + e.Index + ")", e.Item);
        }

        // ---- Cell editing ----
        private void Gantt_BeginCellEdit(object sender, BeginCellEditEventArgs e)
        {
            Write("BeginCellEdit [" + e.MappingName + "]", e.Item);
        }
        private void Gantt_EndCellEdit(object sender, EndCellEditEventArgs e)
        {
            Write("EndCellEdit [" + e.MappingName + "]", e.Item,
                  Range(e.OldItem as TaskDetail), Range(e.Item as TaskDetail));
        }

        // ---- Drag and drop ----
        private void Gantt_TaskBarDragStart(object sender, TaskBarDragStartEventArgs e)
        {
            Write("TaskBarDragStart", e.Task, Range(e.Task as TaskDetail));
        }
        private void Gantt_TaskBarDragDelta(object sender, TaskBarDragDeltaEventArgs e)
        {
            Write("TaskBarDragDelta", e.Task, null, D(e.NewStartDate) + " to " + D(e.NewFinishDate));
        }
        private void Gantt_TaskBarDragCompleted(object sender, TaskBarDragCompletedEventArgs e)
        {
            Write("TaskBarDragCompleted", e.Task,
                  D(e.OldStartDate) + " to " + D(e.OldFinishDate), Range(e.Task as TaskDetail));
        }

        // ---- Resize ----
        private void Gantt_TaskBarResizeStart(object sender, TaskBarResizeStartEventArgs e)
        {
            Write("TaskBarResizeStart (" + e.ResizingDirection + ")", e.Task, Range(e.Task as TaskDetail));
        }
        private void Gantt_TaskBarResizeDelta(object sender, TaskBarResizeDeltaEventArgs e)
        {
            Write("TaskBarResizeDelta (" + e.ResizingDirection + ")", e.Task, null, e.ModifiedValue.ToString());
        }
        private void Gantt_TaskBarResizeCompleted(object sender, TaskBarResizeCompletedEventArgs e)
        {
            Write("TaskBarResizeCompleted (" + e.ResizingDirection + ")", e.Task,
                  e.OldValue.ToString(), Range(e.Task as TaskDetail));
        }

        // ---- Progress ----
        private void Gantt_TaskBarProgressChanging(object sender, TaskBarProgressChangingEventArgs e)
        {
            Write("TaskBarProgressChanging", e.Task, null, e.NewValue.ToString());
        }
        private void Gantt_TaskBarProgressChanged(object sender, TaskBarProgressChangedEventArgs e)
        {
            Write("TaskBarProgressChanged", e.Task, e.OldValue.ToString(), e.NewValue.ToString());
        }

        // ---- Dependency (only creation is exposed as an event) ----
        private void Gantt_TaskRelationshipEstablished(object sender, TaskRelationshipEventArgs e)
        {
            var s = e.StartTask as TaskDetail;
            var t = e.EndTask as TaskDetail;
            Write("TaskRelationshipEstablished", e.StartTask, null,
                  (s?.ID) + " -> " + (t?.ID) + " (" + e.Relationship + ")");
        }

        // ---- Expand / collapse (e.Item is the parent task) ----
        private void Gantt_TaskExpanding(object sender, TaskExpandingEventArgs e) { Write("TaskExpanding", e.Item); }
        private void Gantt_TaskExpanded(object sender, TaskExpandedEventArgs e) { Write("TaskExpanded", e.Item); }
        private void Gantt_TaskCollapsing(object sender, TaskCollapsingEventArgs e) { Write("TaskCollapsing", e.Item); }
        private void Gantt_TaskCollapsed(object sender, TaskCollapsedEventArgs e) { Write("TaskCollapsed", e.Item); }

        // ---- Log actions ----
        private void ClearLog_Click(object sender, RoutedEventArgs e) { Log.Clear(); }
        private void CopyLog_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            foreach (var l in Log) sb.AppendLine(l.Text);
            var pkg = new DataPackage();
            pkg.SetText(sb.ToString());
            Clipboard.SetContent(pkg);
        }
    }

    public class ProjectViewModel
    {
        public ProjectViewModel()
        {
            _taskCollection = this.GetData();
            _resourceCollection = this.GetResources();
        }

        private ObservableCollection<TaskDetail> _taskCollection;

        /// <summary>
        /// Gets or sets the task item source.
        /// </summary>
        /// <value>The task item source.</value>
        public ObservableCollection<TaskDetail> TaskCollection
        {
            get { return _taskCollection; }
            set { _taskCollection = value; }
        }

        private GanttResourceCollection _resourceCollection;

        /// <summary>
        /// Gets or sets the SfGantt resources.
        /// </summary>
        /// <value>The SfGantt resources.</value>
        public GanttResourceCollection ResourceCollection
        {
            get { return _resourceCollection; }
            set { _resourceCollection = value; }
        }

        /// <summary>
        /// Gets the resources for the project.
        /// </summary>
        /// <returns></returns>
        private GanttResourceCollection GetResources()
        {
            GanttResourceCollection Resources = new GanttResourceCollection();
            Resources.Add(new GanttResource { ID = "1", Name = "Project Manager" });
            Resources.Add(new GanttResource { ID = "2", Name = "Developer" });
            Resources.Add(new GanttResource { ID = "3", Name = "Tester" });
            return Resources;
        }

        /// <summary>
        /// Gets the data.
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<TaskDetail> GetData()
        {
            // ---------------- Level 1: root task ----------------
            ObservableCollection<TaskDetail> Schedule = new ObservableCollection<TaskDetail>();

            Schedule.Add(new TaskDetail
            {
                Name = "Product Launch",
                ID = "1"
            });

            // ---------------- Level 2: phases ----------------
            ObservableCollection<TaskDetail> ScheduleProcess = new ObservableCollection<TaskDetail>();

            ScheduleProcess.Add(new TaskDetail
            {
                Name = "Phase 1 - Planning",
                ID = "2"
            });

            ScheduleProcess.Add(new TaskDetail
            {
                Name = "Phase 2 - Design",
                ID = "6"
            });

            ScheduleProcess.Add(new TaskDetail
            {
                Name = "Phase 3 - Development",
                ID = "10"
            });

            ScheduleProcess.Add(new TaskDetail
            {
                Name = "Phase 4 - Testing",
                ID = "14"
            });

            ScheduleProcess.Add(new TaskDetail
            {
                Name = "Phase 5 - Release",
                ID = "17"
            });

            Schedule[0].Children = ScheduleProcess;

            // ---------------- Level 3: Phase 1 - Planning ----------------
            ObservableCollection<TaskDetail> Planning = new ObservableCollection<TaskDetail>();

            Planning.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 2),
                FinishDate = new DateTime(2026, 11, 4),
                Name = "Define scope",
                ID = "3",
                Progress = 100
            });

            Planning.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 2),
                FinishDate = new DateTime(2026, 11, 6),
                Name = "Plan budget",
                ID = "4",
                Progress = 100
            });

            Planning.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 9),
                FinishDate = new DateTime(2026, 11, 9),
                Name = "Planning complete (Milestone)",
                ID = "5",
                Progress = 100
            });

            ScheduleProcess[0].Children = Planning;

            // ---------------- Level 3: Phase 2 - Design ----------------
            ObservableCollection<TaskDetail> Design = new ObservableCollection<TaskDetail>();

            Design.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 10),
                FinishDate = new DateTime(2026, 11, 13),
                Name = "UI design",
                ID = "7",
                Progress = 100
            });

            Design.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 10),
                FinishDate = new DateTime(2026, 11, 17),
                Name = "Technical design",
                ID = "8",
                Progress = 100
            });

            Design.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 18),
                FinishDate = new DateTime(2026, 11, 18),
                Name = "Design complete (Milestone)",
                ID = "9",
                Progress = 100
            });

            ScheduleProcess[1].Children = Design;

            // ---------------- Level 3: Phase 3 - Development ----------------
            ObservableCollection<TaskDetail> Development = new ObservableCollection<TaskDetail>();

            Development.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 19),
                FinishDate = new DateTime(2026, 12, 2),
                Name = "Backend development",
                ID = "11",
                Progress = 20
            });

            Development.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 11, 19),
                FinishDate = new DateTime(2026, 11, 27),
                Name = "Frontend development",
                ID = "12",
                Progress = 30
            });

            Development.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 12, 3),
                FinishDate = new DateTime(2026, 12, 3),
                Name = "Code complete (Milestone)",
                ID = "13"
            });

            ScheduleProcess[2].Children = Development;

            // ---------------- Level 3: Phase 4 - Testing ----------------
            ObservableCollection<TaskDetail> Testing = new ObservableCollection<TaskDetail>();

            Testing.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 12, 4),
                FinishDate = new DateTime(2026, 12, 10),
                Name = "Integration testing",
                ID = "15"
            });

            Testing.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 12, 11),
                FinishDate = new DateTime(2026, 12, 15),
                Name = "User acceptance testing",
                ID = "16"
            });

            ScheduleProcess[3].Children = Testing;

            // ---------------- Level 3: Phase 5 - Release ----------------
            ObservableCollection<TaskDetail> Release = new ObservableCollection<TaskDetail>();

            Release.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 12, 16),
                FinishDate = new DateTime(2026, 12, 17),
                Name = "Deployment",
                ID = "18"
            });

            Release.Add(new TaskDetail
            {
                StartDate = new DateTime(2026, 12, 18),
                FinishDate = new DateTime(2026, 12, 18),
                Name = "Go live (Milestone)",
                ID = "19"
            });

            ScheduleProcess[4].Children = Release;

            // ---------------- Task relationships (Finish to Start) ----------------
            // Planning complete <- Define scope, Plan budget
            Planning[2].Predecessors.Add(new TaskRelationship() { ID = "3", Relationship = Relationship.FinishToStart });
            Planning[2].Predecessors.Add(new TaskRelationship() { ID = "4", Relationship = Relationship.FinishToStart });

            // UI design, Technical design <- Planning complete
            Design[0].Predecessors.Add(new TaskRelationship() { ID = "5", Relationship = Relationship.FinishToStart });
            Design[1].Predecessors.Add(new TaskRelationship() { ID = "5", Relationship = Relationship.FinishToStart });

            // Design complete <- UI design, Technical design
            Design[2].Predecessors.Add(new TaskRelationship() { ID = "7", Relationship = Relationship.FinishToStart });
            Design[2].Predecessors.Add(new TaskRelationship() { ID = "8", Relationship = Relationship.FinishToStart });

            // Backend development, Frontend development <- Design complete
            Development[0].Predecessors.Add(new TaskRelationship() { ID = "9", Relationship = Relationship.FinishToStart });
            Development[1].Predecessors.Add(new TaskRelationship() { ID = "9", Relationship = Relationship.FinishToStart });

            // Code complete <- Backend development, Frontend development
            Development[2].Predecessors.Add(new TaskRelationship() { ID = "11", Relationship = Relationship.FinishToStart });
            Development[2].Predecessors.Add(new TaskRelationship() { ID = "12", Relationship = Relationship.FinishToStart });

            // Integration testing <- Code complete
            Testing[0].Predecessors.Add(new TaskRelationship() { ID = "13", Relationship = Relationship.FinishToStart });

            // User acceptance testing <- Integration testing
            Testing[1].Predecessors.Add(new TaskRelationship() { ID = "15", Relationship = Relationship.FinishToStart });

            // Deployment <- User acceptance testing
            Release[0].Predecessors.Add(new TaskRelationship() { ID = "16", Relationship = Relationship.FinishToStart });

            // Go live <- Deployment
            Release[1].Predecessors.Add(new TaskRelationship() { ID = "18", Relationship = Relationship.FinishToStart });

            // ---------------- Resources (IDs match GetResources) ----------------
            Planning[0].Resources.Add("1");      // Define scope             - Project Manager
            Planning[1].Resources.Add("1");      // Plan budget              - Project Manager
            Design[0].Resources.Add("2");        // UI design                - Developer
            Design[1].Resources.Add("2");        // Technical design         - Developer
            Development[0].Resources.Add("2");   // Backend development      - Developer
            Development[1].Resources.Add("2");   // Frontend development     - Developer
            Testing[0].Resources.Add("3");       // Integration testing      - Tester
            Testing[1].Resources.Add("3");       // User acceptance testing  - Tester
            Release[0].Resources.Add("1");       // Deployment               - Project Manager

            return Schedule;
        }
    }
}