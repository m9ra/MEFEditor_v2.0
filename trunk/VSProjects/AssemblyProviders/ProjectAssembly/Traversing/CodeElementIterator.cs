using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VSLangProj;
using VSLangProj2;
using VSLangProj80;

using EnvDTE;
using EnvDTE80;

using Analyzing;
using TypeSystem;
using Interoperability;

using AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace AssemblyProviders.ProjectAssembly.Traversing
{
    /// <summary>
    /// Search iterator implementation for iterating through CodeElements
    /// </summary>
    class CodeElementIterator : SearchIterator
    {
        /// <summary>
        /// Owning assembly
        /// </summary>
        private readonly VsProjectAssembly _assembly;

        /// <summary>
        /// Nodes, which are initial positions of current iterator
        /// <remarks>If no position is specified - contains null</remarks>
        /// </summary>
        private readonly IEnumerable<CodeElement> _currentNodes;

        /// <summary>
        /// Here is stored current path of iterator - it is used for generic building
        /// </summary>
        private readonly PathInfo _currentPath;

        private CodeElementIterator(IEnumerable<CodeElement> currentNodes, VsProjectAssembly assembly, PathInfo currentPath)
        {
            _currentNodes = currentNodes;
            _assembly = assembly;
            _currentPath = currentPath;
        }

        internal CodeElementIterator(VsProjectAssembly assembly)
        {
            _assembly = assembly;
            _currentNodes = null;
            _currentPath = null;
        }

        /// <inheritdoc />
        public override SearchIterator ExtendName(string suffix)
        {
            if (suffix == "")
                return this;

            var shortSuffix = suffix;
            var genericStart = shortSuffix.IndexOf('<');
            if (genericStart > 0)
                shortSuffix = shortSuffix.Substring(0, genericStart);

            var selectedNodes = new List<CodeElement>();
            foreach (var actualNode in getActualNodes())
            {
                var name = actualNode.Name();
                //TODO is name in correct form for generics?
                if (name == shortSuffix)
                {
                    selectedNodes.Add(actualNode);
                }
            }
            if (selectedNodes.Count == 0)
                return null;

            return new CodeElementIterator(selectedNodes, _assembly, PathInfo.Append(_currentPath, suffix));
        }

        /// <inheritdoc />
        public override IEnumerable<TypeMethodInfo> FindMethods(string searchedName)
        {
            var methodItems = getMethodItems(searchedName);

            foreach (var methodItem in methodItems)
            {
                yield return methodItem.Info;
            }
        }

        public override IEnumerable<string> GetExpansions()
        {
            if (_currentNodes == null)
                yield break;

            foreach (var node in _currentNodes)
            {
                //expand to base classes/interfaces

                switch (node.Kind)
                {
                    case vsCMElement.vsCMElementInterface:
                    case vsCMElement.vsCMElementClass:
                        var cls = node as CodeType;

                        foreach (CodeElement subType in cls.Bases)
                        {
                            yield return subType.FullName;
                        }

                        break;

                    default:
                        break;
                }
            }
        }

        private IEnumerable<CodeElement> getActualNodes()
        {
            //THIS IS PERFORMANCE KILLER - IS IT POSSIBLE TO WORKAROUND VISUAL STUDIO THREADING MODEL?
            if (_currentNodes == null)
            {
                //position has not been currently set - use root elements

                foreach (var element in _assembly.RootElements)
                    yield return element;

            }
            else
            {
                //we already have initial position
                foreach (var node in _currentNodes)
                {
                    foreach (CodeElement child in node.Children())
                    {
                        var lang = node.Language;
                        yield return child;
                    }
                }
            }
        }

        private IEnumerable<MethodItem> getMethodItems(string searchedName)
        {
            var methods = new List<MethodItem>();
            var path = PathInfo.Append(_currentPath, searchedName);
            
            //test if we have constraionts on name
            var returnAll = searchedName == null;
            if (returnAll)
                //HACK because of retrieving getters
                path = PathInfo.Append(path, Naming.GetterPrefix);

            var nodes = getActualNodes();
            foreach (CodeElement child in nodes)
            {
                var method = MethodBuilder.Build(child, path.LastPart, _assembly);
                if (method == null || (method.Info.MethodName != searchedName && !returnAll))
                    //not everything could be filtered by CodeFunction testing
                    continue;

                if (_currentPath.HasGenericArguments)
                    method = method.Make(path);

                methods.Add(method);
            }

            if (methods.Count == 0 && _currentNodes != null)
            {
                //_currentNodes contains parents of actual nodes - here will
                //be all classes where ctor has been searched for
                foreach (var node in _currentNodes)
                {
                    var classNode = node as CodeClass2;
                    if (classNode == null)
                        continue;

                    if (searchedName == Naming.CtorName || returnAll )
                    {
                        //get implicit ctor of class (we can create it, 
                        //because there is no other ctor)
                        methods.Add(MethodBuilder.BuildImplicitCtor(classNode, _assembly));
                    }
                    else if (searchedName == Naming.ClassCtorName || returnAll)
                    {
                        //get implicit cctor of class (we can create it,
                        //because there is no other cctor)
                        methods.Add(MethodBuilder.BuildImplicitClassCtor(classNode, _assembly));
                    }
                }
            }

            return methods;
        }
    }
}
