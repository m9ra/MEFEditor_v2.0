﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem.TypeParsing
{
    /// <summary>
    /// Builder used for TypeDescriptors building
    /// </summary>
    class TypeDescriptorBuilder
    {
        /// <summary>
        /// Build context stack used for handling type arguments level
        /// </summary>
        private readonly Stack<TypeBuildContext> _buildStack = new Stack<TypeBuildContext>();

        /// <summary>
        /// Define ordering number of parameter according to its name
        /// </summary>
        private readonly Dictionary<string, int> _parametersNumbering = new Dictionary<string, int>();

        /// <summary>
        /// Current peek of context stack
        /// </summary>
        internal TypeBuildContext CurrentContext { get { return _buildStack.Peek(); } }

        internal TypeDescriptorBuilder()
        {
            Push();
        }

        /// <summary>
        /// Push new context on type build context
        /// </summary>
        internal void Push()
        {
            _buildStack.Push(new TypeBuildContext());
        }

        /// <summary>
        /// Pop context from stack and add it as argument
        /// to parent context
        /// </summary>
        internal void Pop()
        {
            var popped = _buildStack.Pop();
            CurrentContext.AddArgument(popped);
        }

        /// <summary>
        /// Pop 2 contexts from stack and connect them
        /// </summary>
        internal void ConnectPop()
        {
            var popped = _buildStack.Pop();
            var popped2 = _buildStack.Pop();
            popped.Connect(popped2);
            _buildStack.Push(popped);
        }

        /// <summary>
        /// Insert argument into current context
        /// </summary>
        /// <param name="typeName">Inserted argument</param>
        internal void InsertArgument(string typeName)
        {
            Push();
            Append(typeName);
            Pop();
        }

        /// <summary>
        /// Set current context as parameter of given name
        /// </summary>
        internal void SetDescriptor(TypeDescriptor description)
        {
            CurrentContext.SetDescriptor(description);
        }

        /// <summary>
        /// Append part of type to name in current context
        /// </summary>
        /// <param name="typePart"></param>
        internal void Append(string typePart)
        {
            CurrentContext.Append(typePart);
        }

        /// <summary>
        /// Build type descriptor from current context
        /// </summary>
        /// <returns>Builded descriptor</returns>
        internal TypeDescriptor BuildDescriptor()
        {
            return CurrentContext.BuildDescriptor();
        }
    }
}