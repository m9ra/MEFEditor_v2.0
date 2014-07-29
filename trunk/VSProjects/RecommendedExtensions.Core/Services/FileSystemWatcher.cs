using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utilities;

using System.IO;

namespace RecommendedExtensions.Core.Services
{
    /// <summary>
    /// Service for watching of file and folder changes. It hooks the system fired
    /// when file system changes are registered.
    /// <remarks>Because of controlled resources usage, it disposed temporal watcher after redrawing composition point scheme.</remarks>.
    /// </summary>
    public static class FileChangesWatcher
    {
        /// <summary>
        /// <see cref="FileSystemWatcher" /> objects indexed by directory or file that they are observing.
        /// </summary>
        private readonly static Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// Actions of files registered by.
        /// </summary>
        private readonly static MultiDictionary<string, Action> _registeredActions = new MultiDictionary<string, Action>();

        /// <summary>
        /// Actions that cannot be fired after resources are disposed.
        /// </summary>
        private readonly static List<KeyValuePair<string, Action>> _temporaryActions = new List<KeyValuePair<string, Action>>();

        /// <summary>
        /// Initializes static members of the <see cref="FileChangesWatcher" /> class.
        /// </summary>
        static FileChangesWatcher()
        {
            MEFEditor.TypeSystem.UserInteraction.OnResourceDisposing += disposeResources;
        }

        /// <summary>
        /// Set handler that will be fired when change on watched file is 
        /// registered.
        /// </summary>
        /// <param name="file">The watched file.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="isTemporary">if set to <c>true</c> watcher will be cleaned after composition scheme redrawing.</param>
        public static void WatchFile(string file, Action handler, bool isTemporary)
        {
            if (file == null)
                return;

            setPathWatcher(file);
            registerAction(file, handler, isTemporary);
        }


        /// <summary>
        /// Set handler that will be fired when change on watched folder
        /// is registered.
        /// </summary>
        /// <param name="folder">The watched folder.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="isTemporary">if set to <c>true</c> watcher will be cleaned after composition scheme redrawing.</param>
        public static void WatchFolder(string folder, Action handler, bool isTemporary)
        {
            if (folder == null)
                return;

            if (!folder.EndsWith("" + Path.DirectorySeparatorChar))
                folder = folder + Path.DirectorySeparatorChar;

            setPathWatcher(folder);
            registerAction(folder, handler, isTemporary);
        }

        /// <summary>
        /// Sets the path watcher.
        /// </summary>
        /// <param name="desiredFilePath">The desired file path.</param>
        private static void setPathWatcher(string desiredFilePath)
        {
            var watchedPath = getObservingPath(desiredFilePath);
            var fileName = Path.GetFileName(watchedPath);

            FileSystemWatcher watcher;
            if (watchedPath == desiredFilePath && fileName != "")
            {
                //file exists we wait for its changes
                var directory = Path.GetDirectoryName(desiredFilePath) + Path.DirectorySeparatorChar;
                watcher = new FileSystemWatcher(directory, fileName);
            }
            else
            {
                //file doesn't exists we watch folder where it should be created
                watcher = new FileSystemWatcher(watchedPath);
            }

            registerWatcher(watcher, desiredFilePath);
        }

        /// <summary>
        /// Registers the action with given path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="handler">The handler.</param>
        /// <param name="isTemporary">if set to <c>true</c> watcher will be cleaned after composition scheme redrawing.</param>
        private static void registerAction(string path, Action handler, bool isTemporary)
        {
            _registeredActions.Add(path, handler);

            if (isTemporary)
                _temporaryActions.Add(new KeyValuePair<string, Action>(path, handler));
        }

        /// <summary>
        /// Gets the observing path that is longes existing root of desired path.
        /// </summary>
        /// <param name="desiredPath">The desired path.</param>
        /// <returns>System.String.</returns>
        private static string getObservingPath(string desiredPath)
        {
            if (File.Exists(desiredPath))
                return desiredPath;

            var currentPath = desiredPath;
            while (!Directory.Exists(currentPath))
                currentPath = Directory.GetParent(currentPath).FullName;

            if (currentPath.EndsWith("" + Path.DirectorySeparatorChar))
                return currentPath;

            return currentPath + Path.DirectorySeparatorChar;
        }

        /// <summary>
        /// Registers the watcher.
        /// </summary>
        /// <param name="watcher">The watcher.</param>
        /// <param name="desiredPath">The desired path.</param>
        private static void registerWatcher(FileSystemWatcher watcher, string desiredPath)
        {
            Action handler = () => fireEvents(watcher, desiredPath);
            watcher.Changed += (obj, e) => handler();
            watcher.Renamed += (obj, e) => handler();
            watcher.Created += (obj, e) => handler();
            watcher.Deleted += (obj, e) => handler();

            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Fires the events registered for given watcher.
        /// </summary>
        /// <param name="watcher">The watcher.</param>
        /// <param name="path">The path.</param>
        private static void fireEvents(FileSystemWatcher watcher, string path)
        {
            _watchers.Remove(path);
            watcher.Dispose();

            var eventsCopy = _registeredActions.Get(path).ToArray();
            foreach (var evt in eventsCopy)
            {
                _registeredActions.Remove(path, evt);

                evt();
            }
        }

        /// <summary>
        /// Disposes the resources.
        /// </summary>
        private static void disposeResources()
        {
            foreach (var temporary in _temporaryActions)
            {
                var desiredPath = temporary.Key;
                _registeredActions.Remove(desiredPath, temporary.Value);

                if (!_registeredActions.Get(desiredPath).Any())
                {
                    FileSystemWatcher watcher;
                    if (_watchers.TryGetValue(desiredPath, out watcher))
                    {
                        //watcher is still active - dispose it
                        _watchers.Remove(desiredPath);
                        watcher.EnableRaisingEvents = false;
                        watcher.Dispose();
                    }
                }
            }

            _temporaryActions.Clear();
        }
    }
}
