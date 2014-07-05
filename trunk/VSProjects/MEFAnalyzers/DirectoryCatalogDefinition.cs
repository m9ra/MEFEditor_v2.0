using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition.Hosting;


using Utilities;
using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

namespace MEFAnalyzers
{
    public class DirectoryCatalogDefinition : DataTypeDefinition
    {
        public readonly Field<List<Instance>> Components;
        public readonly Field<string> GivenPath;
        public readonly Field<string> FullPath;
        public readonly Field<string> Pattern;
        public readonly Field<string> Error;

        public DirectoryCatalogDefinition()
        {
            Simulate<DirectoryCatalog>();
            AddCreationEdit("Add DirectoryCatolog", Dialogs.VariableName.GetName, (v) =>
            {
                return new object[]{
                    _pathInput(v)
                };
            });
        }

        #region Type members implementation


        public void _method_ctor(string path)
        {
            _method_ctor(path, "*.dll");
        }

        public void _method_ctor(string path, string pattern)
        {
            string fullPath = null;

            try
            {
                fullPath = ResolveFullPath(path, Services);
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

        public Instance[] _get_Parts()
        {
            return Components.Get().ToArray();
        }

        public string _get_Path()
        {
            return GivenPath.Get();
        }

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
            if (path1 == null || path1=="") return path2;
            if (path2 == null || path2=="") return path1;

            // Folders must end in a slash
            if (!path1.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                path1 += Path.DirectorySeparatorChar;
            }

            var path1Uri = new Uri(path1);
            var path2Uri = new Uri(path2);

            return Uri.UnescapeDataString(path1Uri.MakeRelativeUri(path2Uri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }

        public static string ResolveFullPath(string relativePath, TypeServices services)
        {
            var codeBase = services.CodeBaseFullPath;

            var combined = Path.Combine(codeBase, relativePath);
            var fullPath = Path.GetFullPath(combined);

            return fullPath;
        }

        /// <summary>
        /// Fill given list with components collectd from assemblies specified by fullpath and pattern
        /// </summary>
        /// <param name="result">Here are stored found components</param>
        /// <param name="fullpath">Fullpath of directory where assemblies are searched</param>
        /// <param name="pattern">Pattern for an assembly</param>
        /// <returns>Error if any, null otherwise</returns>
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

        private void setCtorEdits()
        {
            RewriteArg(1, "Change path", _pathInput);
            AppendArg(2, "Add search pattern", _patternInput);
            RewriteArg(2, "Change search pattern", _patternInput);
        }

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

        protected override void draw(InstanceDrawer drawer)
        {
            drawer.SetProperty("Path", GivenPath.Get());
            drawer.SetProperty("FullPath", FullPath.Get());
            drawer.SetProperty("Pattern", Pattern.Get());
            drawer.SetProperty("Error", Error.Get());

            var slot = drawer.AddSlot();

            foreach (var component in Components.Get())
            {
                var componentDrawing = drawer.GetInstanceDrawing(component);
                slot.Add(componentDrawing.Reference);
            }

            drawer.ForceShow();
        }
    }
}
