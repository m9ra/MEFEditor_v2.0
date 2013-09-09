using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel.Composition.Hosting;

using Analyzing;
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
            Pattern.Set(pattern);

            //TODO resolve full path

            setCtorEdits();
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

        private object _patternInput()
        {
            var oldPattern = Pattern.Get();

            return "*.newPattern" + oldPattern;
        }

        private object _pathInput()
        {
            var oldPath = Path.Get();

            return "Test:" + oldPath;
        }

        #endregion
    }
}
