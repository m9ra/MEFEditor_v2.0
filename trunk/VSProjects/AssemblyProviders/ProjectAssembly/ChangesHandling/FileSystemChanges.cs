using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using Utilities;

namespace AssemblyProviders.ProjectAssembly.ChangesHandling
{

    static class FileSystemChanges
    {
        static Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();
        static object _depsOwner = new object();
        public static object GetFileDependency(string file)
        {
            file = Path.GetFullPath(file);
            var fileDir = Path.GetDirectoryName(file);

            var watchedFolder = setFolderWatcher(file);
            throw new NotImplementedException("Register dependencies");
        }

        public static object GetFolderDependency(string folder)
        {
            folder = Path.GetFullPath(folder);

            var watchedFolder = setFolderWatcher(folder);
            throw new NotImplementedException("Register dependencies");
        }

        private static string setFolderWatcher(string folderPath)
        {
            folderPath = folderPath.ToUpper();
            //Get first upper existing folder
            while (!Directory.Exists(folderPath))
                folderPath = Directory.GetParent(folderPath).FullName;

            if (folderPath == null)
                folderPath = ".";

            if (_watchers.ContainsKey(folderPath)) return folderPath; //watcher is already set            
            var watcher = new FileSystemWatcher(folderPath);

            _watchers[folderPath] = watcher;
            setEventHandlers(folderPath);
            return folderPath.ToUpper();
        }

        private static void setEventHandlers(string path)
        {
            var watcher = _watchers[path];

            Action handler = () => fileEvent(path);
            watcher.Changed += (obj, e) => handler();
            watcher.Renamed += (obj, e) => handler();
            watcher.Created += (obj, e) => handler();
            watcher.Deleted += (obj, e) => handler();

            watcher.EnableRaisingEvents = true;
        }

        private static void fileEvent(string path)
        {
            var watcher = _watchers[path];
            _watchers.Remove(path);
            watcher.Dispose();

            throw new NotImplementedException("Path has changed: " + path);
        }

        internal static void Clear()
        {
            foreach (var w in _watchers)
            {
                w.Value.Dispose();
            }
            _watchers.Clear();
        }
    }
}
