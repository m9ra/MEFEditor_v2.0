using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace AssemblyProviders.CILAssembly
{
    public static class FileChangesWatcher
    {
        private readonly static Dictionary<string, FileSystemWatcher> _watchers = new Dictionary<string, FileSystemWatcher>();

        public static void WatchFile(string file, Action handler)
        {
            file = Path.GetFullPath(file);
            var fileDir = Path.GetDirectoryName(file);

            var watchedFolder = setFolderWatcher(file);
        }

        public static void WatchFolder(string folder)
        {
            folder = Path.GetFullPath(folder);

            var watchedFolder = setFolderWatcher(folder);
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
        }
    }
}
