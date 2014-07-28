using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.TypeSystem.TypeParsing
{
    /// <summary>
    /// Adapter base that defines common type interface for
    /// different type systems. It is used for conversions to
    /// <see cref="TypeDescriptor"/> type representation.
    /// </summary>
    public abstract class TypeAdapterBase
    {
        /// <summary>
        /// Gets the name of type.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the namespace of type.
        /// </summary>
        /// <value>The namespace.</value>
        public abstract string Namespace { get; }

        /// <summary>
        /// Gets a value indicating whether this type describes array.
        /// </summary>
        /// <value><c>true</c> if this type is array; otherwise, <c>false</c>.</value>
        public abstract bool IsArray { get; }

        /// <summary>
        /// Gets a value indicating whether this type is generic parameter.
        /// </summary>
        /// <value><c>true</c> if this type is generic parameter; otherwise, <c>false</c>.</value>
        public abstract bool IsGenericParameter { get; }

        /// <summary>
        /// Gets the generic arguments of type.
        /// </summary>
        /// <value>The generic arguments.</value>
        public abstract TypeAdapterBase[] GenericArgs { get; }

        /// <summary>
        /// Gets the declaring type of current type.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public abstract TypeAdapterBase DeclaringType { get; }

        /// <summary>
        /// Gets element type of array type.
        /// </summary>
        /// <value>The element type.</value>
        public abstract TypeAdapterBase ElementType { get; }
    }


    /// <summary>
    /// Partial specialization of <see cref="TypeAdapterBase"/> that 
    /// simplifies common routines.
    /// </summary>
    /// <typeparam name="Adapted">The type representation of adapted type system.</typeparam>
    public abstract class TypeAdapterBase<Adapted> : TypeAdapterBase
    {
        /// <summary>
        /// The adapted type.
        /// </summary>
        public readonly Adapted AdaptedType;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeAdapterBase{Adapted}"/> class.
        /// </summary>
        /// <param name="adaptedType">Type that will be adapted.</param>
        /// <exception cref="System.ArgumentNullException">adaptedType</exception>
        protected TypeAdapterBase(Adapted adaptedType)
        {
            if (adaptedType == null)
                throw new ArgumentNullException("adaptedType");

            AdaptedType = adaptedType;
        }

        /// <summary>
        /// Adapts the specified type.
        /// </summary>
        /// <param name="toAdapt">Type to adapt.</param>
        /// <returns>TypeAdapterBase.</returns>
        protected abstract TypeAdapterBase adapt(Adapted toAdapt);

        /// <summary>
        /// Gets the generic arguments of adapted type.
        /// </summary>
        /// <returns>IEnumerable&lt;Adapted&gt;.</returns>
        protected abstract IEnumerable<Adapted> getGenericArgs();

        /// <summary>
        /// Gets the declaring type of current type.
        /// </summary>
        /// <value>The type of the declaring.</value>
        protected abstract Adapted getDeclaringType();
        
        /// <summary>
        /// Gets element type of array type.
        /// </summary>
        /// <value>The element type.</value>
        protected abstract Adapted getElementType();

        /// <summary>
        /// Gets the declaring type of current type.
        /// </summary>
        /// <value>The type of the declaring.</value>
        public override TypeAdapterBase DeclaringType
        {
            get
            {
                var declaringType = getDeclaringType();
                return nullCheckAdapt(declaringType);
            }
        }

        /// <summary>
        /// Gets element type of array type.
        /// </summary>
        /// <value>The element type.</value>
        public override TypeAdapterBase ElementType
        {
            get
            {
                var elementType = getElementType();
                return nullCheckAdapt(elementType);
            }
        }

        /// <summary>
        /// Gets the generic arguments of type.
        /// </summary>
        /// <value>The generic arguments.</value>
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

        /// <summary>
        /// Nulls the check adapt.
        /// </summary>
        /// <param name="toAdapt">To adapt.</param>
        /// <returns>TypeAdapterBase.</returns>
        private TypeAdapterBase nullCheckAdapt(Adapted toAdapt)
        {
            if (toAdapt == null)
                return null;

            return adapt(toAdapt);
        }
    }
}
