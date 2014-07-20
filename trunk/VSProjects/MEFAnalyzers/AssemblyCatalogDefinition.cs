using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.ComponentModel.Composition.Hosting;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

namespace MEFAnalyzers
{
    public class AssemblyCatalogDefinition : DataTypeDefinition
    {
        public readonly Field<List<Instance>> Components;
        public readonly Field<string> Path;
        public readonly Field<string> AssemblyName;
        public readonly Field<string> FullPath;
        public readonly Field<string> Error;

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

        public void _method_ctor(string path)
        {
            Path.Set(path);
            FullPath.Set(resolveFullPath(path));

            var components = new List<Instance>();
            Components.Set(components);

            var fullPath = DirectoryCatalogDefinition.ResolveFullPath(path, Services);
            var assembly = Services.LoadAssembly(fullPath);

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

            setCtorEdits();
        }

        public Instance[] _get_Parts()
        {
            return Components.Get().ToArray();
        }

        public string _get_Path()
        {
            return Path.Get();
        }

        public string _get_FullPath()
        {
            return FullPath.Get();
        }

        #endregion

        #region Private utilities

        private string resolveFullPath(string relativePath)
        {
            //TODO resolve according to codebase
            return "FullPath://" + relativePath;
        }

        #endregion

        #region Edits handling

        private void setCtorEdits()
        {
            RewriteArg(1, "Change path", _pathInput);
            AddActionEdit("Open folder", () =>
                DirectoryCatalogDefinition.OpenPathInExplorer(System.IO.Path.GetDirectoryName(FullPath.Value)
                ));
        }

        private object _pathInput(ExecutionView view)
        {
            //TODO resolve base path
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

        protected override void draw(InstanceDrawer drawer)
        {
            drawer.SetProperty("Path", Path.Value);
            drawer.SetProperty("FullPath", FullPath.Value);
            drawer.SetProperty("Error", Error.Value);
            drawer.SetProperty("AssemblyName", AssemblyName.Value);

            var slot = drawer.AddSlot();

            foreach (var component in Components.Value)
            {
                var componentDrawing = drawer.GetInstanceDrawing(component);
                slot.Add(componentDrawing.Reference);
            }

            drawer.ForceShow();
        }
    }
}
