﻿using System;
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
            Path.Set(path);
            FullPath.Set(resolveFullPath(path));
            Pattern.Set(pattern);

            var components = new List<Instance>();
            Components.Set(components);

            var assembly = Services.LoadAssembly(path);
            if (assembly == null)
            {
                Error.Set("Assembly file hasn't been found");
            }
            else
            {
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
            return relativePath;
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
            //TODO resolve base path
            var oldPath = FullPath.Get();
            var path = Dialogs.PathProvider.GetFolderPath(oldPath);

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
            drawer.SetProperty("Path", Path.Get());
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
