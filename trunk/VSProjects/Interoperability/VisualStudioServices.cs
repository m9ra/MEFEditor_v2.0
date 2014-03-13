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

        private readonly DTE _dte;
        private readonly Events _events;
        private readonly TextEditorEvents _textEditorEvents;
        private readonly SolutionEvents _solutionEvents;
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

        #region Forwarded events

        public event _dispSolutionEvents_ProjectAddedEventHandler ProjectAdded;

        public event _dispSolutionEvents_ProjectRemovedEventHandler ProjectRemoved;

        public event _dispSolutionEvents_OpenedEventHandler SolutionOpened;

        public event _dispSolutionEvents_AfterClosingEventHandler SolutionClosed;

        public event _dispProjectItemsEvents_ItemAddedEventHandler ProjectItemAdded;

        public event _dispProjectItemsEvents_ItemRemovedEventHandler ProjectItemRemoved;

        #endregion

        public VisualStudioServices(DTE dte)
        {
            Log = new Log();

            _dte = dte;

            _events = _dte.Events;

            _changeWait.Interval = new TimeSpan(0, 0, 1);
            _changeWait.Tick += flushChanges;

            _solutionEvents = _events.SolutionEvents;
            _solutionEvents.Opened += solutionOpened;
            _solutionEvents.AfterClosing += solutionClosed;

            _textEditorEvents = _events.TextEditorEvents;
            _projectItemEvents = _events.SolutionItemsEvents;

            _textEditorEvents.LineChanged += onLineChanged;

            _solutionEvents.ProjectAdded += (p) =>
            {
                if (isMiscellanaeous(p))
                    return;
                onProjectAdded(p);
            };

            _solutionEvents.ProjectRemoved += (p) =>
            {
                if (isMiscellanaeous(p))
                    return;
                onProjectRemoved(p);
            };

            _projectItemEvents.ItemAdded += ProjectItemAdded;
            _projectItemEvents.ItemRemoved += ProjectItemRemoved;
        }

        #region Visual Studio Event handlers

        private void onProjectAdded(Project addedProject)
        {
            var manager = new ProjectManager(addedProject, this);
            _watchedProjects.Add(addedProject, manager);

            if (ProjectAdded != null)
                ProjectAdded(addedProject);
        }

        private void onProjectRemoved(Project removedProject)
        {
            ProjectManager removedManager;
            if (!_watchedProjects.TryGetValue(removedProject, out removedManager))
            {
                Log.Message("Removing not registered project: {0}", removedProject.Name);
                return;
            }

            removedManager.UnHook();

            if (ProjectRemoved != null)
                ProjectRemoved(removedProject);
        }

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

        private void solutionOpened()
        {
            //open all projects
            foreach (Project project in _dte.Solution.Projects)
                onProjectAdded(project);

            if (SolutionOpened != null)
                SolutionOpened();
        }

        #endregion

        /// <summary>
        /// changes are applied with some delay, based on user activities
        /// </summary>
        /// <param name="sender">Sender of event</param>
        /// <param name="e">Arguments of event</param>        
        private void flushChanges(object sender, EventArgs e)
        {
            _changeWait.Stop();

            var changes = _changes.ToArray();
            _changes.Clear();

            //apply collected line changes
            foreach (var change in changes)
            {
                var project = change.Item.ContainingProject;

                ProjectManager manager;
                if (!_watchedProjects.TryGetValue(project, out manager))
                {
                    Log.Warning("Change in not registered project {0}", project.Name);
                    continue;
                }

                throw new NotImplementedException();
            }

            throw new NotImplementedException();
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
