using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.TypeSystem.TypeParsing
{
    class TypeAdapter : TypeAdapterBase<Type>
    {
        public TypeAdapter(Type toAdapt)
            : base(toAdapt)
        {
        }

        protected override TypeAdapterBase adapt(Type toAdapt)
        {
            return new TypeAdapter(toAdapt);
        }

        protected override IEnumerable<Type> getGenericArgs()
        {
            return AdaptedType.GetGenericArguments();
        }

        protected override Type getDeclaringType()
        {
            return AdaptedType.DeclaringType;
        }

        protected override Type getElementType()
        {
            return AdaptedType.GetElementType();
        }

        public override string Name
        {
            get { return AdaptedType.Name; }
        }

        public override string Namespace
        {
            get { return AdaptedType.Namespace; }
        }

        public override bool IsArray
        {
            get { return AdaptedType.IsArray; }
        }

        public override bool IsGenericParameter
        {
            get { return AdaptedType.IsGenericParameter; }
        }
    }
}
