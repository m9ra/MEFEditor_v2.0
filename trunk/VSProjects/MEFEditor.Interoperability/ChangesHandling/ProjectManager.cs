using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EnvDTE;

namespace MEFEditor.Interoperability
{
    /// <summary>
    /// Handle changes on <see cref="Project" /><remarks>All changes made are buffered until <see cref="FlushChanges" /> is called</remarks>.
    /// </summary>
    class ProjectManager
    {
        /// <summary>
        /// Assembly which <see cref="Project" /> will be watched after Hooking.
        /// </summary>
        private readonly Project _project;

        /// <summary>
        /// Available services.
        /// </summary>
        private readonly VisualStudioServices _vs;

        /// <summary>
        /// Determine that all items has been already registered by manager.
        /// </summary>
        private bool _isAllRegistered;

        /// <summary>
        /// Items that has been registered for watching changes.
        /// </summary>
        private readonly Dictionary<ProjectItem, FileItemManager> _watchedItems = new Dictionary<ProjectItem, FileItemManager>();

        /// <summary>
        /// File item managers that have received change.
        /// </summary>
        private readonly HashSet<FileItemManager> _changedFileManagers = new HashSet<FileItemManager>();

        /// <summary>
        /// Event fired whenever element is added (top most).
        /// </summary>
        internal event ElementNodeHandler ElementAdded;

        /// <summary>
        /// Event fired whenever element is removed (every).
        /// </summary>
        internal event ElementNodeHandler ElementRemoved;

        /// <summary>
        /// Event fired whenever element is added (every).
        /// </summary>
        internal event ElementNodeHandler ElementChanged;

        /// <summary>
        /// Gets the root nodes in FileCodeModel of manager.
        /// </summary>
        /// <value>The root nodes.</value>
        internal IEnumerable<ElementNode> RootNodes
        {
            get
            {
                var result = new List<ElementNode>();

                foreach (var file in _watchedItems.Values)
                {
                    if (file.Root == null)
                        continue;

                    foreach (var node in file.Root.Children)
                    {
                        //yield return node;
                        result.Add(node);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Initialize manager.
        /// </summary>
        /// <param name="project"><see cref="Project" /> that will be watched after Hooking.</param>
        /// <param name="vs">Visual studio services.</param>
        public ProjectManager(Project project, VisualStudioServices vs)
        {
            _project = project;
            _vs = vs;
        }

        /// <summary>
        /// Registers the add handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        internal void RegisterAddHandler(ElementNodeHandler handler)
        {
            ElementAdded += handler;
        }

        /// <summary>
        /// Registers the remove handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        internal void RegisterRemoveHandler(ElementNodeHandler handler)
        {
            ElementRemoved += handler;
        }

        /// <summary>
        /// Registers the change handler.
        /// </summary>
        /// <param name="handler">The handler.</param>
        internal void RegisterChangeHandler(ElementNodeHandler handler)
        {
            ElementChanged += handler;
        }

        /// <summary>
        /// Get <see cref="FileItemManager" /> watching given item.
        /// </summary>
        /// <param name="item">Item which manager is needed.</param>
        /// <returns>Founded <see cref="FileItemManager" /> if available, or new one that is created.</returns>
        internal FileItemManager GetFileManager(ProjectItem item)
        {
            FileItemManager fileManager;
            if (!_watchedItems.TryGetValue(item, out fileManager))
            {
                fileManager = registerItem(item);
            }

            return fileManager;
        }

        /// <summary>
        /// Register all items that are contained within project.
        /// </summary>
        internal void RequireRegisterAllItems()
        {
            if (_isAllRegistered)
                return;

            var items = _project.ProjectItems;
            if (items != null)
            {
                foreach (ProjectItem item in items)
                    registerItem(item); //folder and its project items
            }

            _isAllRegistered = true;
        }

        /// <summary>
        /// Remove all registered items.
        /// </summary>
        internal void RemoveAll()
        {
            foreach (var item in _watchedItems.Values)
            {
                item.Disconnect();
                _changedFileManagers.Add(item);
            }
        }

        /// <summary>
        /// Register removing of given <see cref="ProjectItem" />.
        /// </summary>
        /// <param name="item">Project item that is removed.</param>
        internal void RegisterRemove(ProjectItem item)
        {
            FileItemManager manager;
            if (!_watchedItems.TryGetValue(item, out manager))
                //manager is not present
                return;

            _watchedItems.Remove(item);

            manager.Disconnect();
            _changedFileManagers.Add(manager);
        }

        /// <summary>
        /// Register adding of given <see cref="ProjectItem" />.
        /// </summary>
        /// <param name="item">Project item that is added.</param>
        internal void RegisterAdd(ProjectItem item)
        {
            registerItem(item);
        }

        /// <summary>
        /// Register <see cref="LineChange" /> that has been made within watched project.
        /// </summary>
        /// <param name="change">Registered change.</param>
        internal void RegisterChange(LineChange change)
        {
            FileItemManager fileManager;
            if (!_watchedItems.TryGetValue(change.Item, out fileManager))
            {
                //requested manager is not registered yet
                fileManager = registerItem(change.Item);
            }

            if (fileManager == null)
                //there is no availabe manager for requested change
                return;

            if (change.DocumentLength >= 0)
                //force checking
                fileManager.LineChanged(change);

            //changes are fired lazily
            _changedFileManagers.Add(fileManager);
        }

        /// <summary>
        /// Flush all waiting changes.
        /// </summary>
        internal void FlushChanges()
        {
            foreach (var file in _changedFileManagers)
            {
                file.FlushChanges();
            }

            _changedFileManagers.Clear();
        }

        #region Private utilities

        /// <summary>
        /// Register given <see cref="ProjectItem" />.
        /// </summary>
        /// <param name="item">Project item that is registered.</param>
        /// <returns><see cref="FileItemManager" /> created for given item if any.</returns>
        private FileItemManager registerItem(ProjectItem item)
        {
            if (_watchedItems.ContainsKey(item))
                return _watchedItems[item];

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
                return null;
            }
            else
            {
                //item is source code file so it needs to be registered                
                var manager = new FileItemManager(_vs, fileCodeModel);
                manager.ElementAdded += (e) =>
                {
                    if (ElementAdded != null) ElementAdded(e);
                };
                manager.ElementChanged += (e) =>
                {
                    if (ElementChanged != null) ElementChanged(e);
                };
                manager.ElementRemoved += (e) =>
                {
                    if (ElementRemoved != null) ElementRemoved(e);
                };
                manager.FlushChanges();

                //register item
                _watchedItems.Add(item, manager);

                return manager;
            }
        }

        #endregion
    }
}
