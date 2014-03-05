using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

using Utilities;

namespace AssemblyProviders.ProjectAssembly.ChangesHandling
{
    /// <summary>
    /// Handle changes on <see cref="Project"/> represented by <see cref="VsProjectAssembly"/>
    /// </summary>
    class ProjectManager
    {
        /// <summary>
        /// Assembly which <see cref="Project"/> will be watched after Hooking
        /// </summary>
        private readonly VsProjectAssembly _assembly;

        /// <summary>
        /// Items that has been registered for watching changes
        /// </summary>
        private readonly Dictionary<ProjectItem, FileItemManager> _registeredItems = new Dictionary<ProjectItem, FileItemManager>();

        /// <summary>
        /// Initialize manager
        /// </summary>
        /// <param name="assembly">Assembly which <see cref="Project"/> will be watched after Hooking</param>
        public ProjectManager(VsProjectAssembly assembly)
        {
            _assembly = assembly;
        }

        /// <summary>
        /// Hook handlers that will be used for watching changes
        /// </summary>
        internal void Hook()
        {
            hookEvents();
            foreach (ProjectItem item in _assembly.Project.ProjectItems)
                registerItem(item); //folder and its project items
        }

        /// <summary>
        /// Hook events for listening <see cref="ProjectItem"/> add-remove events
        /// </summary>
        private void hookEvents()
        {
            _assembly.VS.ProjectItemAdded += (i) =>
            {
                if (i.ContainingProject == _assembly.Project)
                    //listen only item adds for current project
                    registerItem(i);
            };

            _assembly.VS.ProjectItemRemoved += (i) =>
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
                var manager = new FileItemManager(_assembly, fileCodeModel);

                //register item
                _registeredItems.Add(item, manager);
            }
        }
    }
}
