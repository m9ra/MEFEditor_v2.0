using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;

using Analyzing;
using Analyzing.Editing;
using TypeSystem;
using TypeSystem.Runtime;

namespace MEFAnalyzers
{
    public class DirectoryCatalogDefinition : DataTypeDefinition
    {
        public readonly Field<List<Instance>> Components;
        public readonly Field<string> Path;
        public readonly Field<string> FullPath;
        public readonly Field<string> Pattern;

        public DirectoryCatalogDefinition()
        {
            Simulate<DirectoryCatalog>();

            Components = new Field<List<Instance>>(this);
            Path = new Field<string>(this);
            FullPath = new Field<string>(this);
            Pattern = new Field<string>(this);
        }

        #region Type members implementation

        public void _method_ctor(string path, string pattern = "*.dll")
        {
            Path.Set(path);
            FullPath.Set(resolveFullPath(path));
            Pattern.Set(pattern);

            var components = new List<Instance>();
            Components.Set(components);

            var assembly = Services.LoadAssembly(path);

            foreach (var componentInfo in assembly.GetComponents())
            {
                var component = Context.Machine.CreateInstance(componentInfo.ComponentType);
                components.Add(component);
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
            AppendArg(2, "Add search pattern", _patternInput);
            RewriteArg(2, "Change search pattern", _patternInput);
        }

        private object _patternInput(ExecutionView services)
        {
            var oldPattern = Pattern.Get();

            //TODO
            return "*.newPattern" + oldPattern;
        }

        private object _pathInput(ExecutionView services)
        {
            var oldPath = Path.Get();

            return "Test:" + oldPath;
        }

        #endregion

        protected override void draw(InstanceDrawer drawer)
        {
            drawer.SetProperty("Path", Path.Get());
            drawer.SetProperty("FullPath", FullPath.Get());
            drawer.SetProperty("Pattern", Pattern.Get());

            var slot = drawer.AddSlot();

            foreach (var component in Components.Get())
            {
                var componentDrawing = drawer.GetInstanceDrawing(component);
                slot.Add(componentDrawing.Reference);
            }

            drawer.CommitDrawing();
        }
    }
}
