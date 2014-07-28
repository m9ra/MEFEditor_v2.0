using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition.Hosting;

using Utilities;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

using RecommendedExtensions.Core.Services;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of <see cref="DirectoryCatalog" />.
    /// </summary>
    public class DirectoryCatalogDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The contained components.
        /// </summary>
        public readonly Field<List<Instance>> Components;

        /// <summary>
        /// The given path.
        /// </summary>
        public readonly Field<string> GivenPath;

        /// <summary>
        /// The full path.
        /// </summary>
        public readonly Field<string> FullPath;

        /// <summary>
        /// The pattern.
        /// </summary>
        public readonly Field<string> Pattern;

        /// <summary>
        /// The error.
        /// </summary>
        public readonly Field<string> Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectoryCatalogDefinition" /> class.
        /// </summary>
        public DirectoryCatalogDefinition()
        {
            Simulate<DirectoryCatalog>();
            AddCreationEdit("Add DirectoryCatolog", (v) =>
            {
                return new object[]{
                    _pathInput(v)
                };
            });
        }

        #region Type members implementation

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="path">The path.</param>
        public void _method_ctor(string path)
        {
            _method_ctor(path, "*.dll");
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="pattern">The pattern.</param>
        public void _method_ctor(string path, string pattern)
        {
            string fullPath = null;

            try
            {
                fullPath = ResolveFullPath(path, Services);
                FileChangesWatcher.WatchFolder(fullPath, Services.CompositionSchemeInvalidation, true);
            }
            catch (Exception)
            {
                Error.Set("Path is invalid");
            }

            GivenPath.Set(path);
            FullPath.Set(fullPath);
            Pattern.Set(pattern);

            var components = new List<Instance>();
            Components.Set(components);

            if (Error.Get() == null)
            {
                //if there are no errors try to load components
                var error = fillWithComponents(components, fullPath, pattern);
                Error.Set(error);
            }

            setCtorEdits();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>Instance[].</returns>
        public Instance[] _get_Parts()
        {
            return Components.Get().ToArray();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>System.String.</returns>
        public string _get_Path()
        {
            return GivenPath.Get();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <returns>System.String.</returns>
        public string _get_FullPath()
        {
            return FullPath.Get();
        }

        #endregion

        #region Private utilities

        /// <summary>
        /// Return path2, relative to path1.
        /// </summary>
        /// <param name="path1">Base path.</param>
        /// <param name="path2">Path which will be returned relative to base path.</param>
        /// <returns>Path2 relative to path1.</returns>
        public static string RelativePath(string path1, string path2)
        {
            if (path1 == null || path1 == "") return path2;
            if (path2 == null || path2 == "") return path1;

            // Folders must end in a slash
            if (!path1.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path1 += Path.DirectorySeparatorChar;
            }

            var path1Uri = new Uri(path1);
            var path2Uri = new Uri(path2);

            return Uri.UnescapeDataString(path1Uri.MakeRelativeUri(path2Uri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        /// <summary>
        /// Resolves the full path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <param name="services">The services.</param>
        /// <returns>System.String.</returns>
        public static string ResolveFullPath(string relativePath, TypeServices services)
        {
            var codeBase = services.CodeBaseFullPath;

            var combined = Path.Combine(codeBase, relativePath);
            var fullPath = Path.GetFullPath(combined);

            return fullPath;
        }


        /// <summary>
        /// Open folder in windows explorer.
        /// </summary>
        /// <param name="path">Path to be opened.</param>
        public static void OpenPathInExplorer(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    System.Windows.MessageBox.Show("Desired path doesn't exists","Path not found",System.Windows.MessageBoxButton.OK,System.Windows.MessageBoxImage.Warning);
                    return;
                }

                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            catch
            {
                //opening wasn't successful
            }
        }

        /// <summary>
        /// Fill given list with components collected from assemblies specified by fullpath and pattern.
        /// </summary>
        /// <param name="result">Here are stored found components.</param>
        /// <param name="fullpath">Fullpath of directory where assemblies are searched.</param>
        /// <param name="pattern">Pattern for an assembly.</param>
        /// <returns>Error if any, null otherwise.</returns>
        private string fillWithComponents(List<Instance> result, string fullpath, string pattern)
        {
            var files = Services.GetFiles(fullpath);
            var filteredFiles = FindFiles.Filter(files, pattern).ToArray();

            if (filteredFiles.Length == 0)
                return "No files has been found";

            foreach (var file in filteredFiles)
            {
                try
                {
                    var assembly = Services.LoadAssembly(file);

                    if (assembly == null)
                        return "Assembly " + file + " hasn't been loaded";

                    fillWithComponents(result, assembly);
                }
                catch (Exception)
                {
                    return "Assembly " + file + " loading failed";
                }
            }

            return null;
        }


        /// <summary>
        /// Fills the list with components from given assembly.
        /// </summary>
        /// <param name="result">The result list.</param>
        /// <param name="assembly">The assembly with components</param>
        private void fillWithComponents(List<Instance> result, TypeAssembly assembly)
        {
            foreach (var componentInfo in assembly.GetComponents())
            {
                var component = Context.Machine.CreateInstance(componentInfo.ComponentType);
                result.Add(component);
            }
        }

        #endregion

        #region Edits handling

        /// <summary>
        /// Sets the ctor edits.
        /// </summary>
        private void setCtorEdits()
        {
            AddActionEdit("Open folder", () => OpenPathInExplorer(FullPath.Value));
            RewriteArg(1, "Change path", _pathInput);
            AppendArg(2, "Add search pattern", _patternInput);
            RewriteArg(2, "Change search pattern", _patternInput);
        }

        /// <summary>
        /// Dialog for getting pattern input.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>System.Object.</returns>
        private object _patternInput(ExecutionView view)
        {
            var oldPattern = Pattern.Get();

            var inputPattern = Dialogs.ValueProvider.GetSearchPattern(oldPattern);
            if (inputPattern == null || inputPattern == "")
            {
                view.Abort("No pattern has been selected");
                return null;
            }

            return inputPattern;
        }

        /// <summary>
        /// Dialog for getting path input.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>System.Object.</returns>
        private object _pathInput(ExecutionView view)
        {
            var oldPath = FullPath.Get();
            if (oldPath == null) oldPath = ResolveFullPath(".", Services);

            var fullpath = Dialogs.PathProvider.GetFolderPath(oldPath);
            if (fullpath == null)
            {
                view.Abort("Path hasn't been selected");
                return null;
            }

            var relative = RelativePath(Services.CodeBaseFullPath, fullpath);

            if (relative.Length > fullpath.Length)
                return fullpath;

            return relative;
        }

        #endregion

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of <see cref="MEFEditor.Drawing.DiagramCanvas" /></remarks>.
        /// </summary>
        /// <param name="drawer">The drawer.</param>
        protected override void draw(InstanceDrawer drawer)
        {
            drawer.SetProperty("Path", GivenPath.Get());
            drawer.SetProperty("FullPath", FullPath.Get());
            drawer.SetProperty("Pattern", Pattern.Get());
            drawer.SetProperty("Error", Error.Get());

            var slot = drawer.AddSlot();

            if (Components.Value != null)
                foreach (var component in Components.Value)
                {
                    var componentDrawing = drawer.GetInstanceDrawing(component);
                    slot.Add(componentDrawing.Reference);
                }

            drawer.ForceShow();
        }
    }
}
