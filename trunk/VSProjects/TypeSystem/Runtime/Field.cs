using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;
using Analyzing.Execution;

namespace TypeSystem.Runtime
{
    /// <summary>
    /// Handler for field storage in type definition
    /// </summary>
    public abstract class Field
    {
        protected readonly string Storage;

        protected readonly DataTypeDefinition DefiningType;

        internal abstract object RawObject { get; }

        internal Field(DataTypeDefinition definingType)
        {
            DefiningType = definingType;
            DefiningType.RegisterField(this, out Storage);
        }

        internal Field(DataTypeDefinition definingType, string fieldId)
        {
            DefiningType = definingType;
            DefiningType.RegisterField(this, out Storage);
            Storage = fieldId;
        }
    }


    /// <summary>
    /// Handler for field storage in type definition
    /// <typeparam name="FieldType">Type of field</typeparam>
    /// </summary>
    public class Field<FieldType> : Field
    {
        internal override object RawObject { get { return Get(); } }

        internal Field(DataTypeDefinition definingType)
            : base(definingType)
        {

        }

        internal Field(DataTypeDefinition definingType, string fieldId)
            : base(definingType, fieldId)
        {
        }

        /// <summary>
        /// Get value of field
        /// </summary>
        /// <returns>Value of property</returns>
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
        /// Set value of property
        /// </summary>
        /// <param name="value">Value that will be set to property</param>
        public void Set(FieldType value)
        {
            thisObj.SetField(Storage, value);
        }

        /// <summary>
        /// Representation of this object
        /// </summary>
        private DataInstance thisObj
        {
            get { return DefiningType.This as DataInstance; }
        }
    }
}
