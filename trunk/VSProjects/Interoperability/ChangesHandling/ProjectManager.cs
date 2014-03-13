using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace Interoperability
{
    /// <summary>
    /// Handle changes on <see cref="Project"/>
    /// </summary>
    class ProjectManager
    {
        /// <summary>
        /// Assembly which <see cref="Project"/> will be watched after Hooking
        /// </summary>
        private readonly Project _project;

        private readonly VisualStudioServices _vs;

        /// <summary>
        /// Items that has been registered for watching changes
        /// </summary>
        private readonly Dictionary<ProjectItem, FileItemManager> _registeredItems = new Dictionary<ProjectItem, FileItemManager>();

        /// <summary>
        /// Initialize manager
        /// </summary>
        /// <param name="project"><see cref="Project"/> that will be watched after Hooking</param>
        public ProjectManager(Project project, VisualStudioServices vs)
        {
            _project = project;
            _vs = vs;
        }

        /// <summary>
        /// Hook handlers that will be used for watching changes
        /// </summary>
        internal void Hook()
        {
            hookEvents();
            foreach (ProjectItem item in _project.ProjectItems)
                registerItem(item); //folder and its project items
        }

        /// <summary>
        /// Close all hooks - project has been removed
        /// </summary>
        internal void UnHook()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Hook events for listening <see cref="ProjectItem"/> add-remove events
        /// </summary>
        private void hookEvents()
        {
            _vs.ProjectItemAdded += (i) =>
            {
                if (i.ContainingProject == _project)
                    //listen only item adds for current project
                    registerItem(i);
            };

            _vs.ProjectItemRemoved += (i) =>
            {
                FileItemManager removed;
                if (!_registeredItems.TryGetValue(i, out removed))
                    return;

                throw new NotImplementedException("Remove events has to be fired");
            };
        }

        private void registerItem(ProjectItem item)
        {
            var fileCodeModel = item.FileCodeModel;
            if (fileCodeModel == null)
            {
                if (item.ProjectItems != null)
                {
                    //possible folder, which contains another project items
                    foreach (ProjectItem subItm in item.ProjectItems)
                        //folder and its project items
                        registerItem(subItm);
                }

                //Note that we don't need to watch SubProjects in folders for now           
            }
            else
            {
                //item is source code file so it needs to be registered
                var manager = new FileItemManager(_vs, fileCodeModel);

                //register item
                _registeredItems.Add(item, manager);
            }
        }
    }
}
