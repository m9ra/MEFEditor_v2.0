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
            Pattern.Set(pattern);
            
            //TODO resolve full path

            //TODO resolve componetn types
            Components.Set(new List<InstanceInfo>());


            setCtorEdits();
        }

        public List<Instance> _get_Parts()
        {
            var result=new List<Instance>();
            var componentTypes=Components.Get();

            foreach (var type in componentTypes)
            {
                var part = Context.Machine.CreateInstance(type);
                result.Add(part);
            }

            return result;
        }

        public string _get_Path()
        {
            return Path.Get();
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
