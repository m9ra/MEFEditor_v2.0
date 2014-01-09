﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeSystem.TypeParsing
{
    /// <summary>
    /// Context holding info on single level of type description
    /// </summary>
    class TypeBuildContext
    {
        /// <summary>
        /// Type arguments of tyhpe
        /// </summary>
        private readonly List<TypeBuildContext> _arguments = new List<TypeBuildContext>();

        /// <summary>
        /// Type name of builded context
        /// </summary>
        private readonly StringBuilder _typeName = new StringBuilder();

        /// <summary>
        /// Child of builded descriptor - belongs to namespace type
        /// </summary>
        private TypeBuildContext _connectedChild;

        /// <summary>
        /// Type arguments of builded type
        /// </summary>
        internal IEnumerable<TypeBuildContext> Arguments { get { return _arguments; } }

        /// <summary>
        /// Name of parameter - if not null context belongs to unresolved type parameter
        /// </summary>
        internal string ParameterName;

        /// <summary>
        /// Determine number that is used for first index of type parameter if any
        /// </summary>
        internal int ParameterOffset
        {
            get
            {
                if (_connectedChild == null)
                    return 0;

                return _connectedChild._arguments.Count;
            }
        }

        /// <summary>
        /// Fullname of builded descriptor
        /// </summary>
        internal string FullName
        {
            get
            {
                if (ParameterName != null)
                    return null;

                var fullName = new StringBuilder();
                appendName(fullName);
                appendArguments(fullName);

                return fullName.ToString();
            }
        }

        #region API for building

        /// <summary>
        /// Add argument to current context
        /// </summary>
        /// <param name="typeArgument">Type argument</param>
        internal void AddArgument(TypeBuildContext typeArgument)
        {
            _arguments.Add(typeArgument);
        }

        /// <summary>
        /// Add parameter to current context
        /// </summary>
        /// <param name="parameterName">Type parameter</param>
        internal void AddParameter(string parameterName)
        {
            //TODO keep parameter name
            _arguments.Add(null);
        }

        /// <summary>
        /// Connect child context to current context
        /// </summary>
        /// <param name="childContext">Child context</param>
        internal void Connect(TypeBuildContext childContext)
        {
            if (childContext == this)
                throw new NotSupportedException();

            _connectedChild = childContext;
        }

        /// <summary>
        /// Append part of type name
        /// </summary>
        /// <param name="typePart">Part of type name</param>
        internal void Append(string typePart)
        {
            if (_typeName.Length > 0)
            {
                _typeName.Append('.');
            }

            _typeName.Append(typePart);
        }

        /// <summary>
        /// Build type descriptor according to current state
        /// </summary>
        /// <returns>Builded descriptor</returns>
        internal TypeDescriptor BuildDescriptor()
        {
            var arguments = new Dictionary<string, TypeDescriptor>();

            for (int i = 0; i < _arguments.Count; ++i)
            {
                var arg = _arguments[i];
                var key = i.ToString();

                var isParam = arg.FullName == null;
                if (isParam)
                    key = "@" + key;

                var descriptor = isParam ? null : arg.BuildDescriptor();
                arguments.Add(key, descriptor);
            }

            return new TypeDescriptor(FullName, arguments);
        }
        #endregion

        #region Private routines

        /// <summary>
        /// Append list of arguments into given builder
        /// </summary>
        /// <param name="builder">Builder where agruments will be added</param>
        private void appendArguments(StringBuilder builder)
        {
            var hasArguments = _arguments.Count > 0;
            var addBrackets = hasArguments;

            if (addBrackets)
                builder.Append('<');

            for (int i = 0; i < _arguments.Count; ++i)
            {
                if (i > 0)
                    builder.Append(',');

                var arg = _arguments[i].FullName;

                var isParam = arg == null;
                var name = isParam ? "@" + (ParameterOffset + i) : arg;

                builder.Append(name);
            }

            if (addBrackets)
                builder.Append('>');
        }

        /// <summary>
        /// Append name of current context into given builder
        /// </summary>
        /// <param name="builder">Builder where name will be added</param>
        private void appendName(StringBuilder builder)
        {
            if (_connectedChild != null)
            {
                builder.Append(_connectedChild.FullName);
                builder.Append('.');
            }

            builder.Append(_typeName.ToString());
        }

        #endregion 
    }
}
