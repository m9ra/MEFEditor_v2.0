using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Execution;

namespace MEFEditor.TypeSystem.Runtime
{
    /// <summary>
    /// Utility class that provides simple access to data
    /// saved in <see cref="DataInstance"/> fields from <see cref="DataTypeDefinition"/>.
    /// </summary>
    public abstract class Field
    {
        /// <summary>
        /// The storage where value is stored.
        /// </summary>
        protected readonly string Storage;

        /// <summary>
        /// The type definition where field is used.
        /// </summary>
        protected readonly DataTypeDefinition DefiningType;

        /// <summary>
        /// Gets the object stored by current field.
        /// </summary>
        /// <value>The stored object.</value>
        internal abstract object RawObject { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="definingType">The type definition where field is used.</param>
        internal Field(DataTypeDefinition definingType)
        {
            DefiningType = definingType;
            DefiningType.RegisterField(this, out Storage);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="definingType">The type definition where field is used.</param>
        /// <param name="fieldId">The explicit field identifier that should be used.</param>
        internal Field(DataTypeDefinition definingType, string fieldId)
        {
            DefiningType = definingType;
            DefiningType.RegisterField(this, out Storage);
            Storage = fieldId;
        }
    }


    /// <summary>
    /// Utility class that provides simple strongly typed access to data
    /// saved in <see cref="DataInstance" /> fields from <see cref="DataTypeDefinition" />.
    /// <remarks>Usage of fields consists of member declaration in appropriate <see cref="DataTypeDefinition"/> marked
    /// with non-private access modifier. Initialization of declared field is done automatically in <see cref="DataTypeDefinition"/> constructor.</remarks>
    /// </summary>
    /// <typeparam name="FieldType">The type of the field.</typeparam>
    public class Field<FieldType> : Field
    {
        /// <summary>
        /// Gets the object stored by current field.
        /// </summary>
        /// <value>The stored object.</value>
        internal override object RawObject { get { return Get(); } }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="definingType">The type definition where field is used.</param>
        internal Field(DataTypeDefinition definingType)
            : base(definingType)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Field" /> class.
        /// </summary>
        /// <param name="definingType">The type definition where field is used.</param>
        /// <param name="fieldId">The explicit field identifier that should be used.</param>
        internal Field(DataTypeDefinition definingType, string fieldId)
            : base(definingType, fieldId)
        {
        }

        /// <summary>
        /// Gets or sets value stored in current field.
        /// </summary>
        /// <value>The stored value.</value>
        public FieldType Value
        {
            get
            {
                return Get();
            }
            set
            {
                Set(value);
            }
        }

        /// <summary>
        /// Get value stored in current field.
        /// </summary>
        /// <returns>The stored value.</returns>
        public FieldType Get()
        {
            if (thisObj == null)
                return default(FieldType);

            var field = thisObj.GetField(Storage);
            if (field == null)
                return default(FieldType);

            return (FieldType)field;
        }

        /// <summary>
        /// Get value stored in current field.
        /// </summary>
        /// <param name="value">Value that will be stored in current field.</param>
        public void Set(FieldType value)
        {
            if (thisObj != null)
                thisObj.SetField(Storage, value);
        }

        /// <summary>
        /// Representation of this object.
        /// </summary>
        /// <value>The this object.</value>
        private DataInstance thisObj
        {
            get { return DefiningType.This as DataInstance; }
        }
    }
}
