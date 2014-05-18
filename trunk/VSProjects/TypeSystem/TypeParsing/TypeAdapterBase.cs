using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem.TypeParsing
{
    public abstract class TypeAdapterBase
    {
        public abstract string Name { get; }

        public abstract string Namespace { get; }

        public abstract bool IsArray { get; }

        public abstract bool IsGenericParameter { get; }

        public abstract TypeAdapterBase[] GenericArgs { get; }

        public abstract TypeAdapterBase DeclaringType { get; }

        public abstract TypeAdapterBase ElementType { get; }
    }

    public abstract class TypeAdapterBase<Adapted> : TypeAdapterBase
    {
        public readonly Adapted AdaptedType;

        protected TypeAdapterBase(Adapted adaptedType)
        {
            if (adaptedType == null)
                throw new ArgumentNullException("adaptedType");

            AdaptedType = adaptedType;
        }

        protected abstract TypeAdapterBase adapt(Adapted toAdapt);

        protected abstract IEnumerable<Adapted> getGenericArgs();

        protected abstract Adapted getDeclaringType();

        protected abstract Adapted getElementType();
        
        public override TypeAdapterBase DeclaringType
        {
            get
            {
                var declaringType = getDeclaringType();
                return nullCheckAdapt(declaringType);
            }
        }

        public override TypeAdapterBase ElementType
        {
            get
            {
                var elementType = getElementType();
                return nullCheckAdapt(elementType);
            }
        }

        public override TypeAdapterBase[] GenericArgs
        {
            get {
                var args = new List<TypeAdapterBase>();
                foreach (var arg in getGenericArgs())
                {
                    args.Add(adapt(arg));
                }

                return args.ToArray();
            }
        }

        private TypeAdapterBase nullCheckAdapt(Adapted toAdapt)
        {
            if (toAdapt == null)
                return null;

            return adapt(toAdapt);
        }
    }
}
