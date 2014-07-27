using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Mono.Cecil;

using MEFEditor.TypeSystem.TypeParsing;

namespace RecommendedExtensions.Core.Languages.CIL
{
    /// <summary>
    /// Adapter for TypeReference
    /// </summary>
    class TypeReferenceAdapter : TypeAdapterBase<TypeReference>
    {
        /// <summary>
        /// Create adapter for given type reference
        /// </summary>
        /// <param name="type">Adapted type reference</param>
        internal TypeReferenceAdapter(TypeReference type)
            : base(type)
        {
        }

        ///<inheritdoc />
        public override string Name
        {
            get { return AdaptedType.Name; }
        }

        ///<inheritdoc />
        public override string Namespace
        {
            get { return AdaptedType.Namespace; }
        }

        ///<inheritdoc />
        public override bool IsArray
        {
            get { return AdaptedType.IsArray; }
        }

        ///<inheritdoc />
        public override bool IsGenericParameter
        {
            get { return AdaptedType.IsGenericParameter; }
        }

        ///<inheritdoc />
        protected override TypeAdapterBase adapt(TypeReference toAdapt)
        {
            return new TypeReferenceAdapter(toAdapt);
        }

        ///<inheritdoc />
        protected override IEnumerable<TypeReference> getGenericArgs()
        {
            var genericInstance = AdaptedType as GenericInstanceType;

            if (genericInstance == null)
                return AdaptedType.GenericParameters;

            return genericInstance.GenericArguments;
        }

        ///<inheritdoc />
        protected override TypeReference getDeclaringType()
        {
            return AdaptedType.DeclaringType;
        }

        ///<inheritdoc />
        protected override TypeReference getElementType()
        {
            return AdaptedType.GetElementType();
        }
    }
}
