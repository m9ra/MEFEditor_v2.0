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
    /// Representation of join between import and export.
    /// </summary>
    public class Join
    {
        /// <summary>
        /// Import join point.
        /// </summary>
        public readonly JoinPoint Import;
        /// <summary>
        /// Export join point.
        /// </summary>
        public readonly JoinPoint Export;

        /// <summary>
        /// Determine if join is error join.
        /// </summary>
        /// <remarks>Error joins can be used for errors highlighting.</remarks>
        public bool IsErrorJoin;

        /// <summary>
        /// Create Join object.
        /// </summary>
        /// <param name="import">Import join point.</param>
        /// <param name="export">Export join point.</param>
        public Join(JoinPoint import, JoinPoint export)
        {
            if (!(import.Point is Import) || !(export.Point is Export)) throw new NotSupportedException("not supported join points");
            Import = import;
            Export = export;
        }
    }

    /// <summary>
    /// Representation of Import/Export defined on instance.
    /// </summary>
    public class JoinPoint
    {
        /// <summary>
        /// Instance on which is Import/Export defined.
        /// </summary>
        internal readonly ComponentRef Instance;
        /// <summary>
        /// Point which belongs to this join point.
        /// </summary>
        public readonly object Point;

        /// <summary>
        /// Determine if import is prerequisity.
        /// </summary>
        public readonly bool IsPrerequesity;

        /// <summary>
        /// Determine contract of Import/Export.
        /// </summary>
        public readonly string Contract;
        /// <summary>
        /// Type of Imported/Exported value.
        /// </summary>
        public readonly TypeDescriptor ContractType;
        /// <summary>
        /// Type of imported item value. (Is different from <see cref="ContractType"/> for ImportMany/Lazy imports)
        /// </summary>
        public readonly TypeDescriptor ImportManyItemType;



        /// <summary>
        /// Determine if import allow default value.
        /// </summary>
        public readonly bool AllowDefault;
        /// <summary>
        /// Determine if import allow importing many values.
        /// </summary>
        public readonly bool AllowMany;
        /// <summary>
        /// Error which is set to current join point.
        /// </summary>
        public string Error;
        /// <summary>
        /// Warning which is set to current join point.
        /// </summary>
        public string Warning;

        /// <summary>
        /// Create join point from export.
        /// </summary>
        /// <param name="instance">Instance where is export defined.</param>
        /// <param name="export">Export which join point is created.</param>
        internal JoinPoint(ComponentRef instance, Export export)
        {
            Point = export;
            Contract = export.Contract;
            ContractType = export.ExportType;
            Instance = instance;
        }

        /// <summary>
        /// Create join point from import.
        /// </summary>
        /// <param name="instance">Instance where is import defined.</param>
        /// <param name="import">Import which join point is created.</param>
        internal JoinPoint(ComponentRef instance, Import import)
        {
            IsPrerequesity = import.IsPrerequisity;
            Point = import;
            Contract = import.Contract;

            AllowDefault = import.AllowDefault;
            AllowMany = import.AllowMany;

            var info = import.ImportTypeInfo;
            ContractType = info.ImportType;
            if (AllowMany) ImportManyItemType = info.ItemType;

            Instance = instance;
        }
    }
}
