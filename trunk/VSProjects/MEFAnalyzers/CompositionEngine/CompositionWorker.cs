using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using TypeSystem;
using Utilities;

namespace MEFAnalyzers.CompositionEngine
{
    /// <summary>
    /// Worker which is used for MEF composition simulation.
    /// </summary>
    class CompositionWorker
    {
        /// <summary>
        /// Instances which has been constructed
        /// </summary>
        HashSet<Instance> _constructedInstances = new HashSet<Instance>();
        /// <summary>
        /// Instances with all imports satisfied
        /// </summary>
        HashSet<Instance> _satisfiedInstances = new HashSet<Instance>();
        /// <summary>
        /// Instances which has satisfied preimports
        /// </summary>
        HashSet<Instance> _preImportsSatisfied = new HashSet<Instance>();
        /// <summary>
        /// Instances which satisfiing failed
        /// </summary>
        HashSet<Instance> _failedInstances = new HashSet<Instance>();

        /// <summary>
        /// Instances, which prerequisities are currently satisfied - used for circular dependency checking
        /// </summary>
        HashSet<Instance> _prereqInstances = new HashSet<Instance>();

        ComponentStorage _componentsStorage;
        List<Action> _composeActions = new List<Action>();
        List<Join> _joins = new List<Join>();
        bool _failed;

        CompositionContext _context;

        /// <summary>
        /// imports to its collected exports, if ! _failed, are there all needed exports for defined imports, after constructor call
        /// </summary>
        MultiDictionary<JoinPoint, JoinPoint> _storage = new MultiDictionary<JoinPoint, JoinPoint>();


        /// <summary>
        /// Create composition worker which compose given instances.
        /// </summary>
        /// <param name="context">Services available for interpreting.</param>
        /// <param name="parts">Parts to compose.</param>
        public CompositionWorker(CompositionContext context, IEnumerable<Instance> parts)
        {
            _context = context;
            _componentsStorage = new ComponentStorage(context, parts);

            if (_componentsStorage.Failed)
            {
                _failed = true;
                return;
            }

            foreach (var inst in _componentsStorage.GetComponents())
                if (hasImports(inst))
                    if (!satisfyComponent(inst))
                    {
                        _failed = true;
                        return;
                    }
        }

        /// <summary>
        /// Determine if instance has imports.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>True if instance has imports.</returns>
        private bool hasImports(Instance instance)
        {
            return _context.HasImports(instance);
        }

        private bool satisfyComponent(Instance inst)
        {
            if (_satisfiedInstances.Contains(inst)) return true; //instance has already been satisfied
            if (_failedInstances.Contains(inst)) return false; //instance has been proceeded, but failed

            bool satisfied = satisfyPreImports(inst);
            if (!satisfied)
            {
                _failedInstances.Add(inst);
                return false;
            }

            satisfied = satisfyNormImports(inst);

            if (!satisfied) _failedInstances.Add(inst);
            else _satisfiedInstances.Add(inst);

            return satisfied;
        }

        /// <summary>
        /// Can be satisfied only after Prerequisities imports has been satisfied
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
        private bool satisfyNormImports(Instance inst)
        {
            var hasSatisfiedImports = _constructedInstances.Contains(inst) || _context.IsInstanceConstructed(inst);
            if (!hasSatisfiedImports) throw new NotSupportedException("InternalError:Cant satisfy imports before prerequisities are satisfied");
            foreach (var imp in getImportsRaw(inst))
            {
                if (imp.IsPrerequisity)
                    //satisfy only normal imports, because prerequisities has to be satisfied now
                    continue;

                if (!satisfyImport(inst, imp))
                    //composition failed
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Prerequisity imports has to be satisfied before component construction
        /// </summary>
        /// <param name="inst"></param>
        /// <returns></returns>
        private bool satisfyPreImports(Instance inst)
        {
            if (_failedInstances.Contains(inst))
                //has been already proceeded         
                return false;

            if (_prereqInstances.Contains(inst))
                //circular dependency
                return false;

            if (_preImportsSatisfied.Contains(inst))
                //has been already satisfied
                return true;

            if (_context.IsInstanceConstructed(inst))
                //prerequisities was solved before constructing
                return true;

            var preImports = new List<Instance>();
            bool satisfied = true;

            _prereqInstances.Add(inst); //avoid dependency cycling

            foreach (var pre in getImportsRaw(inst))
            {
                if (!pre.IsPrerequisity)
                    //satisfy only prerequisities
                    continue;

                if (satisfyImport(inst, pre))
                    //import was satisfied
                    continue;

                satisfied = false;
                break;
            }

            _prereqInstances.Remove(inst);

            if (!satisfied)
            {
                _failedInstances.Add(inst);
                return false;
            }

            if (!enqConstructor(inst))
                return false;

            _preImportsSatisfied.Add(inst);
            return true;
        }



        /// <summary>
        /// Satisfy given import. Found exports are added int storage
        /// </summary>
        /// <param name="component"></param>
        /// <param name="import"></param>
        /// <returns></returns>
        bool satisfyImport(Instance component, Import import)
        {
            if (import.Setter == null && _context.IsInstanceConstructed(component))
                //instance is already constructed - dont need satisfy via importing constructor
                return true;

            var components = getComponents(import);
            bool hasCandidates = components.Length > 0;
            components.Except(_prereqInstances);
            JoinPoint imp = getImport(import, component);

            if (components.Length == 0 && !import.AllowDefault)
            {
                string error = "Can't satisfy import";
                if (hasCandidates)
                    error += ", there are probably circular dependencies in prerequisity imports";
                else
                    error = noMatchingExportError(imp, error);

                //set error to import
                setError(imp, error);

                if (import.IsPrerequisity)
                    setWarning(getExports(component), "Because of unsatisfied prerequisity import, exports cannot be provided");
                return false;
            }

            if (components.Length > 1 && !import.AllowMany)
            {
                setError(imp, "There are more than one matching component for export satisfiyng");
                foreach (var expProvider in components)
                    makeErrorJoins(imp, expProvider, "Matching export in ambiguous component");
                return false;
            }

            foreach (var candidate in components)
            {
                if (!satisfyPreImports(candidate))
                {
                    if (_failed)
                        //error has been already set
                        return false;

                    setError(imp, "Cannot satisfy import, because depending component cannot be instantiated");
                    makeErrorJoins(imp, candidate, "This export cannot be provided before prerequisity imports are satisfied");

                    //satisfy all needed requirments
                    return false;
                }

                if (!satisfyImport(component, import, candidate))
                    //errors were set
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Resolve error for unsatisfiable import. Make error joins and set warnings for contract matching export.
        /// </summary>        
        /// <param name="impPoint">Unsatisfiable import.</param>
        /// <param name="errorPrefix">Error prefix.</param>
        /// <returns>Return error description according to given prefix.</returns>
        private string noMatchingExportError(JoinPoint impPoint, string errorPrefix)
        {
            var import = impPoint.Point as Import;
            var exps = getExports(import);
            if (exps.Length == 0)
            {
                //there are no matching components/or there is problem in meta data filtering.
                exps = getExports(import, false);
                if (exps.Length == 0)
                    errorPrefix += " because there are no exports matching contract";
                else
                {
                    errorPrefix += " because of incompatible meta data";
                    makeErrorJoins(impPoint, exps, "Incompatible exported metadata");
                }
            }
            else
            {
                errorPrefix += " because all matching components have ambiguous exports";
                makeErrorJoins(impPoint, exps, "Ambiguous export");
            }
            return errorPrefix;
        }

        /// <summary>
        /// make error joins between matching exports (used for showing errors)
        /// </summary>
        /// <param name="imp"></param>
        /// <param name="exportProvider"></param>
        /// <param name="expWarning"></param>
        private void makeErrorJoins(JoinPoint imp, Instance exportProvider, string expWarning)
        {
            var exps = new List<JoinPoint>();
            foreach (var exp in getExports(exportProvider))
            {
                if (!match(imp.Point as Import, exp)) continue;
                exps.Add(exp);
            }

            makeErrorJoins(imp, exps, expWarning);
        }

        private void makeErrorJoins(JoinPoint imp, IEnumerable<JoinPoint> exps, string expWarning)
        {
            foreach (var exp in exps)
            {
                var join = new Join(imp, exp);
                join.IsErrorJoin = true;
                _joins.Add(join);
                setWarning(exp, expWarning);
            }
        }

        /// <summary>
        /// find exports which satisfies import from exportsProvider (exportsProvider has been instatiated and its exports has to match into import)
        /// found exports are added into storage
        /// 
        /// provide type checking of join, on fail set errors and return false
        /// </summary>
        /// <param name="component"></param>
        /// <param name="import"></param>
        /// <param name="exportsProvider"></param>
        private bool satisfyImport(Instance component, Import import, Instance exportsProvider)
        {
            if (!_constructedInstances.Contains(exportsProvider) && !_context.IsInstanceConstructed(exportsProvider))
                throw new NotSupportedException("candidate has to be constructed before providing exports");

            var imp = getImport(import, component);
            foreach (var exp in getExports(exportsProvider))
            {
                if (match(import, exp))
                {
                    var join = new Join(imp, exp);
                    _joins.Add(join);

                    if (!typeCheck(join)) return false;
                    _storage.Add(imp, exp);
                }
            }

            if (import.Setter != null)  // else it will be set via importing constructor
                enqSetter(imp);

            return true;
        }

        private bool typeCheck(Join join)
        {
            var imp = join.Import;
            var exp = join.Export;
            var impType = imp.ContractType;
            var expType = exp.ContractType;

            var importItem = imp.AllowMany ? imp.ImportManyItemType.TypeName : imp.Contract;
            var importId = imp.AllowMany ? "Import item" : "Import";

            if (!_context.IsSubType(expType, importItem))
            {
                setError(imp, string.Format("{0} is of type {1}, so it cannot accept export of type {2}", importId, importItem, expType.TypeName));
                setWarning(exp, "Export contract doesn't provide type safe identification");
                join.IsErrorJoin = true;
                return false;
            }
            return true;
        }


        /// <summary>
        /// Return components which are candidates for import satisfying
        /// </summary>
        /// <param name="import"></param>
        /// <returns></returns>
        Instance[] getComponents(Import import)
        {
            var candidates = _componentsStorage.GetComponents(import.Contract);
            var result = new List<Instance>();
            foreach (var candidate in candidates)
            {
                var matchingExps = new List<JoinPoint>();
                foreach (var exp in getExports(candidate))
                {
                    if (match(import, exp))
                        matchingExps.Add(exp);
                }

                switch (matchingExps.Count) //continue, when candidate doesnt match to import
                {
                    case 0://if no matching export - no candidate                        
                        continue;
                    case 1:
                        break; //all imports can accept one export
                    default: //more than one
                        if (import.AllowMany)
                            break;
                        continue;
                }
                //here are all candidates matching export                
                result.Add(candidate);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Determine if import is matching to given export.
        /// </summary>
        /// <param name="import">Import to test.</param>
        /// <param name="export">Export to test.</param>
        /// <param name="metaDataTest">Determine if meta data should be used for filtering.</param>
        /// <returns>True if import can be satisfied from export.</returns>
        private bool match(Import import, JoinPoint export, bool metaDataTest = true)
        {
            var importMeta = import.ImportTypeInfo.MetaDataType;

            bool metaDataMatch = true;
            if (importMeta != null && metaDataTest)
            {
                var exportMeta = (export.Point as Export).Meta;

                metaDataMatch = testMetaData(importMeta, exportMeta);
            }

            return import.Contract == export.Contract && metaDataMatch;
        }


        /// <summary>
        /// Test if metadataType can be satisfied from metadata
        /// </summary>
        /// <param name="metadataType"></param>
        /// <param name="meta"></param>
        /// <returns></returns>
        private bool testMetaData(InstanceInfo metadataType, MetaExport meta)
        {
            foreach (var setter in _context.GetMethods(metadataType))
            {
                if (setter.Parameters.Length != 1)
                    //is not valid setter method
                    continue;
                var setterType = setter.Parameters[0].Type;

                var name = setter.MethodName;
                if (name.Length < 5 || name.Substring(0, 4) != "set_")
                    //is not valid setter name
                    continue;

                name = name.Substring(4);

                if (meta == null)
                    //cannot satisfy metaDataType
                    return false;

                IEnumerable<Instance> data = null;
                meta.Data.TryGetValue(name, out data);

                var metaToSet = getMetaInst(setterType, data, meta.IsMultiple(name));
                if (metaToSet == null)
                    //not matching types
                    return false;
            }
            return true;
        }


        private Instance getMetaInst(InstanceInfo setterType, IEnumerable<Instance> data, bool isMultiple)
        {
            if (data == null)
                return null;

            int metaDataCount = 0;

            foreach (var inst in data)
            {
                ++metaDataCount;
                if (!testTypeArrayMatch(inst.Info, setterType, isMultiple))
                    //not matching type
                    return null;
            }

            if (metaDataCount != 1 && !isMultiple)
                //cannot load multiple instances into setter
                return null;

            if (!isMultiple)
                return data.First();

            throw new NotImplementedException();
            /*
            var directLoad = ArrayDirectLoad.FromEnumerable(data);
            return _context.InstanceCreator.CreateInstance(setterType, directLoad);*/
        }

        private bool testTypeArrayMatch(InstanceInfo testedType, InstanceInfo setterType, bool arrayTest)
        {
            if (arrayTest)
                return setterType.TypeName.Contains(testedType.TypeName);
            else
                return _context.IsSubType(testedType, setterType);
        }


        /// <summary>
        /// Return exports which are declared by instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        JoinPoint[] getExports(Instance instance)
        {
            var result = new List<JoinPoint>();
            foreach (var exp in _context.GetExports(instance))
                result.Add(getExport(exp, instance));
            foreach (var exp in _context.GetSelfExports(instance))
                result.Add(getExport(exp, instance));

            return result.ToArray();
        }

        /// <summary>
        /// return all matching imports in all components
        /// </summary>
        /// <param name="import">Import to get exports for.</param>
        /// <param name="metaDataTest">Determine if metadata are used for export filtering.</param>
        /// <returns>Available exports for import.</returns>
        JoinPoint[] getExports(Import import, bool metaDataTest = true)
        {
            var result = new List<JoinPoint>();
            var candidates = _componentsStorage.GetComponents(import.Contract);
            foreach (var candidate in candidates)
                foreach (var exp in getExports(candidate))
                    if (match(import, exp, metaDataTest)) result.Add(exp);

            return result.ToArray();
        }


        /// <summary>
        /// Return joinpoint for given export
        /// </summary>
        /// <param name="export"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        private JoinPoint getExport(Export export, Instance component)
        {
            return _componentsStorage.Translate(export, component);
        }


        /// <summary>
        /// Return all declared imports for this instance
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        private JoinPoint[] getImports(Instance instance)
        {
            var result = new List<JoinPoint>();
            foreach (var imp in getImportsRaw(instance))
                result.Add(getImport(imp, instance));
            return result.ToArray();
        }


        private IEnumerable<Import> getImportsRaw(Instance instance)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Return JoinPoint for given import
        /// </summary>
        /// <param name="import"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        private JoinPoint getImport(Import import, Instance component)
        {
            return _componentsStorage.Translate(import, component);
        }

        /// <summary>
        /// enqueue setter call which satisfy import from exports
        /// </summary>
        /// <param name="import"></param>    
        private void enqSetter(JoinPoint import)
        {
            var exps = _storage.GetValues(import);
            if (exps == null) return; //allow default doesnt require setter

            foreach (var exp in exps)
                _joins.Add(new Join(import, exp));

            _composeActions.Add(() => callSetter(import));
        }

        private void callSetter(JoinPoint import) //call setter
        {
            var imp = import.Point as Import;

            _context.AddCall(import.Instance, imp.Setter, createExport(import));
        }
        /// <summary>
        /// enqueue call importing/default constructor on instance
        /// </summary>
        /// <param name="inst"></param>        
        private bool enqConstructor(Instance inst)
        {
            if (_constructedInstances.Contains(inst)) throw new NotSupportedException("InternalError: Cant construct instance twice");
            if (!_context.IsInstanceConstructed(inst))
            { //test if instance was added into composition in constructed state
                if (_context.GetComponentInfo(inst).ImportingConstructor == null)
                {
                    setError(getExports(inst), "Cannot provide exports because of missing importing or parameter less constructor");
                    setError(getImports(inst), "Cannot set imports, because of missing importing or parameter less constructor");
                    _failed = true;
                    return false;
                }
                _composeActions.Add(() => callImportingConstructor(inst));
            }
            _constructedInstances.Add(inst);
            return true;
        }

        private void callImportingConstructor(Instance inst)
        {
            var info = _context.GetComponentInfo(inst);
            var constr = info.ImportingConstructor;

            var args = new List<Instance>();

            foreach (var import in info.Imports)
                if (import.Setter == null) //has to be satisfied via importing constructor
                {
                    var imp = getImport(import, inst);
                    args.Add(createExport(imp));
                }

            _context.AddCall(inst, constr, args.ToArray());
        }

        /// <summary>
        /// Create export for given import, from exports available in storage
        /// Is lazy determine, if export will be wrapped into lazys
        /// </summary>
        /// <param name="imp"></param>
        /// <returns></returns>
        private Instance createExport(JoinPoint imp)
        {
            var exps = _storage.GetValues(imp);
            switch (exps.Count())
            {
                case 0:
                    return null;
                case 1:
                    if (imp.AllowMany) return callManyExportGetter(imp, exps);
                    else return callExportGetter(imp, exps.First());

                default:
                    return callManyExportGetter(imp, exps);
            }
        }

        private Instance callExportGetter(JoinPoint import, JoinPoint export)
        {
            var exp = export.Point as Export;
            var imp = import.Point as Import;
            var info = imp.ImportTypeInfo;

            /*     InstanceLoader loader;

                 if (exp.Getter == null)
                     //self export
                     loader = () => export.Instance;
                 else
                     loader = () => export.Instance.CallMethod(exp.Getter, new CallInfo(_context.Context));

                 if (info.IsItemLazy)
                 {
                     var valMeta = new ValueWithMetadata(loader);

                     //generic for created lazy object
                     var lazyParam = info.ItemType.FullName;

                     if (info.MetaDataType != null)
                     {
                         lazyParam += "," + info.MetaDataType.FullName;
                         var proxyType = string.Format("System.Proxy<{0}>", info.MetaDataType.FullName);

                         ProxyMethodCall proxyMethod = (m, i) => metaDataProxyMethod(m, i, exp.Meta);
                         valMeta.Metadata = _context.InstanceCreator.CreateInstance(proxyType, proxyMethod);
                     }

                     var lazyTypeName = string.Format("System.Lazy<{0}>", lazyParam);

                     return _context.InstanceCreator.CreateInstance(lazyTypeName, valMeta);
                 }
                 else return loader();*/

            throw new NotImplementedException();
        }


        /// <summary>
        /// Method used for proxiing methods on objects created from meta data info.
        /// </summary>
        /// <param name="proxiedMethod">Method which is proxied</param>
        /// <param name="context">Available interpreting services.</param>
        /// <param name="meta">Proxied meta data object.</param>
        /// <returns></returns>
        private Instance metaDataProxyMethod(TypeMethodInfo proxiedMethod, MetaExport meta)
        {
            var name = proxiedMethod.MethodName;
            if (name.Length < 4 || name.Substring(0, 4) != "get_")
                return null;

            name = proxiedMethod.MethodName.Substring(4);

            IEnumerable<Instance> data;
            meta.Data.TryGetValue(name, out data);
            var result = getMetaInst(proxiedMethod.ReturnType, data, meta.IsMultiple(name));

            if (result == null)
                result = null;

            return result;
        }

        private Instance callManyExportGetter(JoinPoint import, IEnumerable<JoinPoint> exps)
        {
            var instances = new List<Instance>();

            foreach (var exp in exps)
                instances.Add(callExportGetter(import, exp));


            Instance iCollectionToSet;
            TypeMethodInfo addMethod;

            if (isICollectionImport(import, out iCollectionToSet, out addMethod))
            {
                //import will be filled by adding instances            
                if (iCollectionToSet == null)
                {
                    //there is no collection which could be set.
                    setWarning(import, "Cannot get ICollection object, for importing exports.");
                    return null;
                }

                foreach (var inst in instances)
                    _context.AddCall(iCollectionToSet, addMethod.MethodID, inst);

                //because it will be set via setter
                return iCollectionToSet;
            }
            else
            {
                //import will be filled with an array
                var exportType = string.Format("System.Array<{0},1>", import.ImportManyItemType);
                return _context.CreateArray(import.ImportManyItemType, instances);
            }
        }


        private bool isICollectionImport(JoinPoint import, out Instance iCollectionToSet, out TypeMethodInfo addMethod)
        {
            var imp = import.Point as Import;

            /*  TypeMethodInfo addingMethod = null;
              imp.ImportTypeInfo.ImportType.ForEachBaseType((fullname) =>
              {
                  addingMethod = Tools.GetCollectionAdd(fullname);
                  return addingMethod != null;
              });
              addMethod = addingMethod;
              iCollectionToSet = null;

              if (addMethod == null)
                  //cannot resolve type is ICollection
                  return false;

              iCollectionToSet = getImportInstance(import);

              return true;*/
            throw new NotImplementedException();
        }

        private Instance getImportInstance(JoinPoint import)
        {
            var imp = import.Point as Import;

            var setter = imp.Setter;
            if (setter == null || !setter.MethodName.StartsWith("set_"))
                //cannot find setter -> cannot get getter
                return null;

            var getterName = "get_" + setter.MethodName.Substring(4);
            var instType = import.Instance.Info;

            var overloads = _context.GetMethods(instType, getterName);
            if (overloads == null || overloads.Count() != 1)
                //cannot resolve getter overload
                return null;


            //return import.Instance.CallMethod(overloads[0], new CallInfo(_context.Context));
            throw new NotImplementedException();
        }

        /// <summary>
        /// Set error for given joinPoints
        /// </summary>
        /// <param name="points"></param>
        /// <param name="error"></param>
        private void setError(JoinPoint[] points, string error)
        {
            foreach (var joinPoint in points) setError(joinPoint, error);
        }

        private void setError(JoinPoint point, string error)
        {
            if (point.Error != null)
            {
                if (point.Error.Contains(error))
                    //same error has already been set.
                    return;
                point.Error += "\n" + error;
            }
            else point.Error = error;
        }


        private void setWarning(JoinPoint[] points, string warning)
        {
            foreach (var point in points) setWarning(point, warning);
        }

        private void setWarning(JoinPoint point, string warning)
        {
            if (point.Warning != null)
            {
                if (point.Warning.Contains(warning))
                    //same warning has already been set.
                    return;
                point.Warning += "\n" + warning;
            }
            else point.Warning = warning;
        }

        /// <summary>
        /// Simulate composition. If composition doesn't failed, call appropriate constructors/setters and satisfy all imports of all available components.
        /// </summary>
        /// <returns>CompositionResult collected during composition simulation.</returns>
        internal CompositionResult GetResult()
        {
            if (_componentsStorage.Failed) return new CompositionResult(_joins.ToArray(), _componentsStorage.GetPoints(), _componentsStorage.Error);

            string error = null;
            if (!_failed)
            {
                foreach (var act in _composeActions) act();
            }
            if (_failed) error = "Composition failed because there were some errors";

            var result = new CompositionResult(_joins.ToArray(), _componentsStorage.GetPoints(), error);
            return result;
        }
    }
}
