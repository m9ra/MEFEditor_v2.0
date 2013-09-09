using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Analyzing;

namespace TypeSystem.Runtime
{
    /// <summary>
    /// Handler for field storage in type definition
    /// </summary>
    public class Field
    {
        protected readonly string Storage;

        protected readonly DataTypeDefinition DefiningType;

        public Field(DataTypeDefinition definingType)
        {
            DefiningType = definingType;
            DefiningType.RegisterField(this, out Storage);
        }
    }


    /// <summary>
    /// Handler for field storage in type definition
    /// <typeparam name="FieldType">Type of field</typeparam>
    /// </summary>
    public class Field<FieldType>:Field
    {
        public Field(DataTypeDefinition definingType)
            :base(definingType)
        {
            
        }

        /// <summary>
        /// Get value of field
        /// </summary>
        /// <returns>Value of property</returns>
        public FieldType Get()
        {
            return (FieldType)DefiningType.Context.GetField(thisObj, Storage);
        }

        /// <summary>
        /// Set value of property
        /// </summary>
        /// <param name="value">Value that will be set to property</param>
        public void Set(FieldType value)
        {
            
            DefiningType.Context.SetField(thisObj, Storage, value);
        }

        /// <summary>
        /// Representation of this object
        /// </summary>
        private Instance thisObj
        {
            get { return DefiningType.CurrentArguments[0]; }
        }
    }
}
