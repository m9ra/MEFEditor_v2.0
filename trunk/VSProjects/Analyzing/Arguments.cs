using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzing
{
    /// <summary>
    /// Store arguments (value variables and type arguments) for call
    /// </summary>
    public class Arguments
    {
        /// <summary>
        /// Determine that arguments object has been initialized, so it can be used for call invoking
        /// </summary>
        internal bool IsInitialized { get; private set; }

        /// <summary>
        /// Variables where are stored values for variable arguments
        /// <remarks>Zero index is used for this object</remarks>
        /// </summary>
        internal readonly VariableName[] ValueVariables;

        private readonly InstanceInfo[] _typeArguments;

        private Arguments(string[] valueArguments, InstanceInfo[] typeArguments)
        {
            _typeArguments = typeArguments; 

            //reserve first index for thisObject
            ValueVariables = new VariableName[valueArguments.Length + 1];
            for (int i = 0; i < valueArguments.Length; ++i)
            {
                ValueVariables[i + 1] = new VariableName(valueArguments[i]);
            }
        }

        public static Arguments Values(params string[] valueArguments)
        {
            return new Arguments(valueArguments, new InstanceInfo[0]);
        }

        internal void Initialize(VariableName calledObject)
        {
            System.Diagnostics.Debug.Assert(!IsInitialized, "Cannot initialize arguments twice");

            ValueVariables[0] = calledObject;
            IsInitialized = true;
        }
    }
}
