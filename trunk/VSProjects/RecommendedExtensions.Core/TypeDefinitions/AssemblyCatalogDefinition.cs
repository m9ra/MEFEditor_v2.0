using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition.Hosting;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;

namespace RecommendedExtensions.Core.TypeDefinitions
{
    /// <summary>
    /// Analyzing definition of <see cref="AssemblyCatalog" />.
    /// </summary>
    public class AssemblyCatalogDefinition : DataTypeDefinition
    {
        /// <summary>
        /// The contained components.
        /// </summary>
        public readonly Field<List<Instance>> Components;

        /// <summary>
        /// Path of assembly.
        /// </summary>
        public readonly Field<string> Path;

        /// <summary>
        /// The assembly name.
        /// </summary>
        public readonly Field<string> AssemblyName;

        /// <summary>
        /// The full path.
        /// </summary>
        public readonly Field<string> FullPath;

        /// <summary>
        /// The error.
        /// </summary>
        public readonly Field<string> Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyCatalogDefinition" /> class.
        /// </summary>
        public AssemblyCatalogDefinition()
        {
            Simulate<AssemblyCatalog>();
            AddCreationEdit("Add AssemblyCatolog", (v) =>
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
            Path.Value = path;

            loadComponentsFromPath(path);

            setCtorEdits();
        }

        /// <summary>
        /// Runtime member definition.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        [ParameterTypes(typeof(System.Reflection.Assembly))]
        public void _method_ctor(Instance assembly)
        {
            AsyncCall<string>(assembly, "get_FullPath", (fullPath) =>
            {
                Path.Value = fullPath;
                loadComponentsFromPath(fullPath);
            });
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
            return Path.Get();
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
        /// Resolves the full path.
        /// </summary>
        /// <param name="relativePath">The relative path.</param>
        /// <returns>System.String.</returns>
        private string resolveFullPath(string relativePath)
        {
            return DirectoryCatalogDefinition.ResolveFullPath(relativePath, Services);
        }

        /// <summary>
        /// Loads the components from path.
        /// </summary>
        /// <param name="path">The path.</param>
        private void loadComponentsFromPath(string path)
        {
            var components = new List<Instance>();
            Components.Set(components);

            try
            {
                FullPath.Value = resolveFullPath(path);

                var assembly = Services.LoadAssembly(path);
                //we wont test existence of path in file system, because of mapping
                if (assembly == null)
                {
                    Error.Set("Assembly file hasn't been found");
                }
                else
                {
                    AssemblyName.Value = assembly.Name;
                    foreach (var componentInfo in assembly.GetComponents())
                    {
                        var component = Context.Machine.CreateInstance(componentInfo.ComponentType);
                        components.Add(component);
                    }
                }
            }
            catch (Exception)
            {
                FullPath.Value = path;
                Error.Value = "Invalid path specified";
            }
        }

        #endregion

        #region Edits handling

        /// <summary>
        /// Sets the ctor edits.
        /// </summary>
        private void setCtorEdits()
        {
            RewriteArg(1, "Change path", _pathInput);
            AddActionEdit("Open folder", () =>
                DirectoryCatalogDefinition.OpenPathInExplorer(System.IO.Path.GetDirectoryName(FullPath.Value)
                ));
        }

        /// <summary>
        /// Dialog for path input.
        /// </summary>
        /// <param name="view">View where path edit will be processed.</param>
        /// <returns>System.Object.</returns>
        private object _pathInput(ExecutionView view)
        {
            var oldPath = FullPath.Get();
            var path = Dialogs.PathProvider.GetAssemblyPath(oldPath);

            if (path == null)
            {
                view.Abort("Path hasn't been selected");
                return null;
            }

            return path;
        }

        #endregion

        /// <summary>
        /// Export data from represented <see cref="Instance" /> by using given drawer.
        /// <remarks>Note that only instances which are forced to display are displayed in root of editors workspace.</remarks>.
        /// </summary>
        /// <param name="drawer">The drawer.</param>
        protected override void draw(InstanceDrawer drawer)
        {
            drawer.SetProperty("Path", Path.Value);
            drawer.SetProperty("FullPath", FullPath.Value);
            drawer.SetProperty("Error", Error.Value);
            drawer.SetProperty("AssemblyName", AssemblyName.Value);

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
