using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Threading;

using EnvDTE;
using System.Runtime.InteropServices;

namespace Interoperability
{
    /// <summary>
    /// Services that are used by plugin to interoperate with AssemblyProviders
    /// 
    /// TODO here will be top level handling of Changes
    /// </summary>
    public class VisualStudioServices
    {
        /// <summary>
        /// Projects that are registered for watching
        /// </summary>
        private readonly Dictionary<Project, ProjectManager> _watchedProjects = new Dictionary<Project, ProjectManager>();

        /// <summary>
        /// List of changes that is managed because of lazy changes handling
        /// </summary>
        private readonly List<LineChange> _changes = new List<LineChange>();

        /// <summary>
        /// Timer for lazy waiting on changes
        /// </summary>
        private readonly DispatcherTimer _changeWait = new DispatcherTimer();

        /// <summary>
        /// Timer for waiting until solution starts to been processed. This waiting
        /// is needed for getting attribute and other fullnames resolved
        /// </summary>
        private readonly DispatcherTimer _solutionWait = new DispatcherTimer();


        /// <summary>
        /// Visual studio object used for interoperability
        /// </summary>
        private readonly DTE _dte;

        /// <summary>
        /// Visual studio events object which reference is needed because of unwanted garbage collection
        /// </summary>
        private readonly Events _events;

        /// <summary>
        /// Events provided by text editor
        /// </summary>
        private readonly TextEditorEvents _textEditorEvents;

        /// <summary>
        /// Events provided by active solution
        /// </summary>
        private readonly SolutionEvents _solutionEvents;

        /// <summary>
        /// Events provided for <see cref="ProjectItems"/>
        /// </summary>
        private readonly ProjectItemsEvents _projectItemEvents;

        /// <summary>
        /// Storage for <see cref="HasWaitingChanges"/>
        /// </summary>        
        private bool _hasWaitingChanges = false;

        /// <summary>
        /// Determine that change has been registered and not flushed yet
        /// </summary>
        public bool HasWaitingChanges
        {
            get { return _hasWaitingChanges; }
            private set
            {
                if (value == _hasWaitingChanges)
                    return;
                _hasWaitingChanges = value;

                //TODO fire events
                _changeWait.Stop();
                if (_hasWaitingChanges)
                {
                    _changeWait.Start();
                }
            }
        }

        /// <summary>
        /// Logging service that can be used for displaying messages to user
        /// </summary>
        public readonly Log Log;

        /// <summary>
        /// Projects that are discovered in current solution
        /// </summary>
        public IEnumerable<Project> SolutionProjects { get { return _watchedProjects.Keys; } }

        /// <summary>
        /// Determine that solution is opened
        /// </summary>
        public bool IsSolutionOpen { get { return _dte.Solution != null; } }


        #region Forwarded events

        /// <summary>
        /// Event fired whenever new <see cref="Project"/> is added into active solution
        /// </summary>
        public event _dispSolutionEvents_ProjectAddedEventHandler ProjectAdded;

        /// <summary>
        /// Event fired whenever <see cref="Project"/> is removed from active solution
        /// </summary>
        public event _dispSolutionEvents_ProjectRemovedEventHandler ProjectRemoved;

        /// <summary>
        /// Event fired whenever active solution is opened - it is ensured that SoluctionClosed appears before opening new solution
        /// </summary>
        public event _dispSolutionEvents_OpenedEventHandler SolutionOpened;

        /// <summary>
        /// Event fired whenever active solution is closed
        /// </summary>
        public event _dispSolutionEvents_AfterClosingEventHandler SolutionClosed;

        #endregion

        /// <summary>
        /// Initialize services available from Visual Studio
        /// </summary>
        /// <param name="dte">Entry object for Visual Studio services</param>
        public VisualStudioServices(DTE dte)
        {
            Log = new Log();

            _changeWait.Interval = new TimeSpan(0, 0, 1);
            _changeWait.Tick += flushChanges;

            _solutionWait.Interval = new TimeSpan(0, 0, 1);
            _solutionWait.Tick += solutionOpenedAfterWait;

            _dte = dte;
            _events = _dte.Events;
            _textEditorEvents = _events.TextEditorEvents;
            _solutionEvents = _events.SolutionEvents;
            _projectItemEvents = _events.SolutionItemsEvents;

            _textEditorEvents.LineChanged += onLineChanged;

            _solutionEvents.Opened += solutionOpened;
            _solutionEvents.AfterClosing += solutionClosed;
            _solutionEvents.ProjectAdded += onProjectAdded;
            _solutionEvents.ProjectRemoved += onProjectRemoved;

            _projectItemEvents.ItemAdded += onProjectItemAdded;
            _projectItemEvents.ItemRemoved += onProjectItemRemoved;
        }

        #region Exposed services

        /// <summary>
        /// Get namespaces that are valid for given <see cref="ProjectItem"/>        
        /// </summary>
        /// <param name="projectItem">Project item</param>
        /// <returns>Valid namespaces for given projectItem</returns>
        public IEnumerable<string> GetNamespaces(ProjectItem projectItem)
        {
            var manager = findFileManager(projectItem);
            if (manager == null)
                return new string[0];

            return manager.Namespaces;
        }
        
        #endregion

        #region Visual Studio Event handlers

        /// <summary>
        /// Handler called whenever <see cref="Project"/> is added into active solution
        /// </summary>
        /// <param name="addedProject">Project that has been added</param>
        private void onProjectAdded(Project addedProject)
        {
            if (isMiscellanaeous(addedProject))
                //we don't need to handle miscellanaeous projects
                return;

            if (_watchedProjects.ContainsKey(addedProject))
                //project is already contained
                return;

            var manager = new ProjectManager(addedProject, this);
            _watchedProjects.Add(addedProject, manager);

            if (ProjectAdded != null)
                ProjectAdded(addedProject);

            manager.FlushChanges();
        }

        /// <summary>
        /// Handler called whenever <see cref="Project"/> is removed from active solution
        /// </summary>
        /// <param name="removedProject">Project that has been removed</param>
        private void onProjectRemoved(Project removedProject)
        {
            if (isMiscellanaeous(removedProject))
                //we don't need to handle miscellanaeous projects
                return;

            ProjectManager removedManager;
            if (!_watchedProjects.TryGetValue(removedProject, out removedManager))
            {
                Log.Message("Removing not registered project: {0}", removedProject.Name);
                return;
            }

            if (ProjectRemoved != null)
                ProjectRemoved(removedProject);

            removedManager.RemoveAll();
            removedManager.FlushChanges();
        }

        /// <summary>
        /// Handler called whenever <see cref="ProjectItem"/> is removed
        /// </summary>
        /// <param name="item">Removed project item</param>
        private void onProjectItemRemoved(ProjectItem item)
        {
            var manager = findProjectManager(item);
            if (manager == null)
                //nothing to do
                return;

            manager.RegisterRemove(item);
            manager.FlushChanges();
        }

        /// <summary>
        /// Handler called whenever new <see cref="ProjectItem"/> is added
        /// </summary>
        /// <param name="item">Added project item</param>
        private void onProjectItemAdded(ProjectItem item)
        {
            var manager = findProjectManager(item);
            if (manager == null)
                //nothing to do
                return;

            manager.RegisterAdd(item);
            manager.FlushChanges();
        }

        /// <summary>
        /// Handler called whenever line of some file is changed.
        /// </summary>
        /// <param name="startPoint">Start point of change</param>
        /// <param name="endPoint">End point of change</param>
        /// <param name="hint">Hint determining type of change</param>
        private void onLineChanged(TextPoint startPoint, TextPoint endPoint, int hint)
        {
            //represent opened editor window
            var textDocument = startPoint.Parent;
            if (textDocument == null)
                //there is no available editor window
                return;

            //document assigned to edited source code 
            var changedDocument = textDocument.Parent;
            if (changedDocument == null)
                //there is no available document
                return;

            var changeStart = startPoint.AbsoluteCharOffset;
            var changeEnd = endPoint.AbsoluteCharOffset;

            //in debug mode is relativly slow
            var documentLength = textDocument.EndPoint.AbsoluteCharOffset;

            LogEntry entry = null;
            try
            {
                if (_dte.Solution.SolutionBuild.BuildState == vsBuildState.vsBuildStateInProgress)
                    //no changes can be done during building
                    return;

                if (changedDocument.FullName != "")
                {
                    var change = new LineChange(changedDocument, documentLength, changeStart, changeEnd);
                    _changes.Add(change);
                    HasWaitingChanges = true;
                }

                return;
            }
            catch (COMException ex)
            {
                entry = new LogEntry(LogLevels.Error, "Line change handler throws COMException", ex.ToString(), null);
            }
            catch (ArgumentException ex)
            {
                entry = new LogEntry(LogLevels.Notification, "Line change handler throws ArgumentException", ex.ToString(), null);
            }

            Log.Entry(entry);
        }

        /// <summary>
        /// Handler called whenever active solution is closed
        /// </summary>
        private void solutionClosed()
        {
            //changes can be omitted
            _changes.Clear();

            //all projects are also closed
            var projectsCopy = SolutionProjects.ToArray();

            foreach (var project in projectsCopy)
            {
                onProjectRemoved(project);
            }

            if (SolutionClosed != null)
                SolutionClosed();
        }

        /// <summary>
        /// Handler called whenever active solution is opened
        /// </summary>
        private void solutionOpened()
        {
            _solutionWait.Stop();
            _solutionWait.Start();
        }

        /// <summary>
        /// Handler called after some time from opening solution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void solutionOpenedAfterWait(object sender, EventArgs e)
        {
            _solutionWait.Stop();

            //open all projects
            foreach (Project project in _dte.Solution.Projects)
                onProjectAdded(project);

            if (SolutionOpened != null)
                SolutionOpened();
        }

        #endregion

        /// <summary>
        /// Find <see cref="ProjectManager"/> containing given <see cref="ProjectItem"/>.
        /// If manager cannot be found, log entries are emitted
        /// </summary>
        /// <param name="item">Item which manager has to be found</param>
        /// <returns>Found <see cref="ProjectManager"/> if available, <c>null</c> otherwise</returns>
        private ProjectManager findProjectManager(ProjectItem item)
        {
            var containingProject = item.ContainingProject;
            if (containingProject == null)
            {
                Log.Warning("Project for item {0} is not known", item.Name);
                return null;
            }

            ProjectManager manager;
            if (!_watchedProjects.TryGetValue(containingProject, out manager))
            {
                Log.Message("Project for project item {0} is not known");
            }
            return manager;
        }

        /// <summary>
        /// Find <see cref="FileItemManager"/> according to given <see cref="ProjectItem"/>.
        /// If manager cannot be found, log entries are emitted
        /// </summary>
        /// <param name="item">Item which manager has to be found</param>
        /// <returns>Found <see cref="FileItemManager"/> if available, <c>null</c> otherwise</returns>
        private FileItemManager findFileManager(ProjectItem item)
        {
            var projectManager = findProjectManager(item);
            if (projectManager == null)
                return null;

            return projectManager.GetFileManager(item);
        }

        /// <summary>
        /// changes are applied with some delay, based on user activities
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Arguments of event</param>        
        private void flushChanges(object sender, EventArgs e)
        {
            HasWaitingChanges = false;

            //TODO optimize
            var changes = _changes.ToArray();
            _changes.Clear();

            var changedManagers = new HashSet<ProjectManager>();

            //apply collected line changes to affected managers
            foreach (var change in changes)
            {
                var project = change.Item.ContainingProject;

                ProjectManager manager;
                if (!_watchedProjects.TryGetValue(project, out manager))
                {
                    Log.Warning("Change in not registered project {0}", project.Name);
                    continue;
                }

                manager.RegisterChange(change);
                changedManagers.Add(manager);
            }

            //flush all changes in managers
            foreach (var manager in changedManagers)
            {
                manager.FlushChanges();
            }
        }

        /// <summary>
        /// Determine if given project is used only for miscelanaeous files
        /// </summary>
        /// <param name="project">Tested project</param>
        /// <returns><c>True</c> if project has only miscelneous files, <c>false</c> otherwise</returns>
        private bool isMiscellanaeous(Project project)
        {
            try
            {
                if (project.Name == "Miscellaneous Files")
                    return true;
            }
            catch (Exception ex)
            {
                var entry = new LogEntry(LogLevels.Warning, "Determining miscellaneous files project", ex.ToString(), null);
                Log.Entry(entry);
            }

            return false;
        }

        #region Exception tools

        /// <summary>
        /// Handle all exceptions, which can occur during executing invoke info.       
        /// </summary>
        /// <param name="action">Method which will be checked for exceptions.</param>
        /// <param name="actionDescription">Description which will be included in error description.</param>
        public void ExecutingExceptions(Action action, string actionDescription)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogErrorEntry("Unhandled exception when " + actionDescription, ex.ToString());
            }
        }
        /// <summary>
        /// Handle all exceptions, which can occur during processing CodeModel objects.       
        /// </summary>
        /// <param name="action">Method which will be checked for exceptions.</param>
        /// <param name="actionDescription">Description which will be included in error description.</param>
        public void CodeModelExceptions(Action action, string actionDescription)
        {
            try
            {
                action();
            }
            catch (COMException ex)
            {
                LogErrorEntry("Processing CodeModel failed when " + actionDescription, ex.ToString());
            }
            catch (Exception ex)
            {
                LogErrorEntry("Unhandled exception when " + actionDescription, ex.ToString());
            }
        }

        /// <summary>
        /// Handle all exceptions, which can occur during drawing.       
        /// </summary>
        /// <param name="action">Method which will be checked for exceptions.</param>
        /// <param name="actionDescription">Description which will be included in error description.</param>
        public void DrawingExceptions(Action action, string actionDescription)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogErrorEntry("Unhandled exception when " + actionDescription, ex.ToString());
            }
        }

        /// <summary>
        /// Handle all exceptions, which can occur during editor loading..       
        /// </summary>
        /// <param name="action">Method which will be checked for exceptions.</param>
        /// <param name="actionDescription">Description which will be included in error description.</param>
        public void EditorLoadingExceptions(Action action, string actionDescription)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                LogErrorEntry("Unhandled exception when " + actionDescription, ex.ToString());
            }
        }

        /// <summary>
        /// Create log entry with specified description, description and position.
        /// </summary>
        /// <param name="msg">Message for log entry.</param>
        /// <param name="descr">Description for log entry.</param>
        /// <param name="navigate">Navigate to error position</param>
        /// <returns>Log entry according to specified parameters</returns>
        public void LogErrorEntry(string msg, string descr, Action navigate = null)
        {
            Log.Entry(new LogEntry(LogLevels.Error, msg, descr, navigate));
        }

        #endregion

    }
}
