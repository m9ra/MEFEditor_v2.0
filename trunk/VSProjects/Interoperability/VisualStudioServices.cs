using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        private readonly DTE _dte;
        private readonly Events _events;
        private readonly TextEditorEvents _textEditorEvents;
        private readonly SolutionEvents _solutionEvents;
        private readonly ProjectItemsEvents _projectItemEvents;

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

        private void onLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            throw new NotImplementedException();
        }

        private void solutionClosed()
        {
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
