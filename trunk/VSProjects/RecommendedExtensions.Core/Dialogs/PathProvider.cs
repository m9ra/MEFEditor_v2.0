using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Windows.Forms;

namespace RecommendedExtensions.Core.Dialogs
{
    public static class PathProvider
    {
        public static readonly string AssemblyFilter = "Assembly files (*.dll;*.exe)|*.exe;*.dll|All files (*.*)|*.*";

        public static string GetAssemblyPath(string initialPath, string basePath = null)
        {
            return GetFilePath(initialPath, AssemblyFilter);
        }

        public static string GetFilePath(string initialPath, string fileFilter, string basePath = null)
        {
            if (initialPath == null)
                initialPath = basePath;

            var dialog = new OpenFileDialog();
            dialog.InitialDirectory = Path.GetDirectoryName(initialPath);
            dialog.Filter = fileFilter;

            var result = dialog.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    var fileName = dialog.FileName;
                    var resultPath = RelativePath(basePath, fileName);
                    if (resultPath.Length > fileName.Length)
                    {
                        //there is no sufficient relative path
                        resultPath = fileName;
                    }
                    return resultPath;
                default:
                    return null;
            }
        }

        public static string GetFolderPath(string initialPath, string basePath = null)
        {
            var dialog = new FolderBrowserDialog();

            if (initialPath == null)
                initialPath = basePath;

            dialog.SelectedPath = initialPath;
            var result = dialog.ShowDialog();
            switch (result)
            {
                case DialogResult.OK:
                    var resultPath = RelativePath(basePath, dialog.SelectedPath);
                    if (resultPath.Length > dialog.SelectedPath.Length)
                    {
                        //there is no sufficient relative path
                        resultPath = dialog.SelectedPath;
                    }
                    return resultPath;

                default:
                    return null;
            }
        }

        /// <summary>
        /// Return path2, relative to path1.
        /// </summary>
        /// <param name="path1">Base path.</param>
        /// <param name="path2">Path which will be returned relative to base path.</param>
        /// <returns>Path2 relative to path1.</returns>
        public static string RelativePath(string path1, string path2)
        {
            if (path1 == null) return path2;
            if (path2 == null) return path1;

            var uri1 = new Uri(path1);
            var uri2 = new Uri(path2);
            return Uri.UnescapeDataString(uri1.MakeRelativeUri(uri2).ToString());
        }
    }
}
