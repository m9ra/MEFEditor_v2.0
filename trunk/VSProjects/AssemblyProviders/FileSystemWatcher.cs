using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Utilities;

using System.IO;

namespace AssemblyProviders
{
    public static class FileChangesWatcher
    {
        /// <summary>
        /// <see cref="FileSystemWatcher"/> objects indexed by directory or file that they are observing
        /// </summary>
        private readonly static Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        /// <summary>
        /// Actions of files registered by
        /// </summary>
        private readonly static MultiDictionary<string, Action> _registeredActions = new MultiDictionary<string, Action>();

        /// <summary>
        /// Actions that cannot be fired after resources are disposed
        /// </summary>
        private readonly static List<KeyValuePair<string, Action>> _temporaryActions = new List<KeyValuePair<string, Action>>();

        static FileChangesWatcher()
        {
            TypeSystem.UserInteraction.OnResourceDisposing += disposeResources;
        }

        public static void WatchFile(string file, Action handler, bool isTemporary)
        {
            if (file == null)
                return;

            setPathWatcher(file);
            registerAction(file, handler, isTemporary);
        }


        public static void WatchFolder(string folder, Action handler, bool isTemporary)
        {
            if (folder == null)
                return;

            if (!folder.EndsWith("" + Path.DirectorySeparatorChar))
                folder = folder + Path.DirectorySeparatorChar;

            setPathWatcher(folder);
            registerAction(folder, handler, isTemporary);
        }

        private static void setPathWatcher(string desiredFilePath)
        {
            var watchedPath = getObservingPath(desiredFilePath);

            FileSystemWatcher watcher;
            if (watchedPath == desiredFilePath)
            {
                //file exists we wait for its changes
                var directory = Path.GetDirectoryName(desiredFilePath) + Path.DirectorySeparatorChar;
                var fileName = Path.GetFileName(desiredFilePath);
                watcher = new FileSystemWatcher(directory, fileName);
            }
            else
            {
                //file doesn't exists we watch folder where it should be created
                watcher = new FileSystemWatcher(watchedPath);
            }

            registerWatcher(watcher, desiredFilePath);
        }

        private static void registerAction(string file, Action handler, bool isTemporary)
        {
            _registeredActions.Add(file, handler);

            if (isTemporary)
                _temporaryActions.Add(new KeyValuePair<string, Action>(file, handler));
        }

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

        private static void registerWatcher(FileSystemWatcher watcher, string desiredPath)
        {
            Action handler = () => fireEvents(watcher, desiredPath);
            watcher.Changed += (obj, e) => handler();
            watcher.Renamed += (obj, e) => handler();
            watcher.Created += (obj, e) => handler();
            watcher.Deleted += (obj, e) => handler();

            watcher.EnableRaisingEvents = true;
        }

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
