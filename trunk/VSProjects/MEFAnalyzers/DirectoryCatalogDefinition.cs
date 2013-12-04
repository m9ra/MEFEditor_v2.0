using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;

using Analyzing;
using Analyzing.Editing;
using TypeSystem.Runtime;

namespace MEFAnalyzers
{
    public class DirectoryCatalogDefinition : DataTypeDefinition
    {
        public readonly Field<List<InstanceInfo>> Components;
        public readonly Field<string> Path;
        public readonly Field<string> FullPath;
        public readonly Field<string> Pattern;

        public DirectoryCatalogDefinition()
        {
            Simulate<DirectoryCatalog>();

            Components = new Field<List<InstanceInfo>>(this);
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

            var components = new List<InstanceInfo>();
            Components.Set(components);

            var assembly=Services.LoadAssembly(path);

            foreach (var component in assembly.GetComponents())
            {
                components.Add(component.ComponentType);
            }

            setCtorEdits();
        }

        public Instance[] _get_Parts()
        {
            var result = new List<Instance>();
            var componentTypes = Components.Get();

            foreach (var type in componentTypes)
            {
                var part = Context.Machine.CreateInstance(type);
                result.Add(part);
            }

            return result.ToArray();
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
            RewriteArg(0, "Change path", _pathInput);
            AddArg(1, "Add search pattern", _patternInput);
            RewriteArg(1, "Change search pattern", _patternInput);
        }

        private object _patternInput(ExecutionView services)
        {
            var oldPattern = Pattern.Get();

            return "*.newPattern" + oldPattern;
        }

        private object _pathInput(ExecutionView services)
        {
            var oldPath = Path.Get();

            return "Test:" + oldPath;
        }

        #endregion
    }
}
