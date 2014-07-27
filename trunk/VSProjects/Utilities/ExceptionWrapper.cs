using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    /// <summary>
    /// Wrapper of <see cref="Exception"/> objects.
    /// </summary>
    public class ExceptionWrapper : Exception
    {
        /// <summary>
        /// The wrapped exception
        /// </summary>
        private readonly Exception _wrappedException;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionWrapper"/> class.
        /// </summary>
        /// <param name="wrappedException">The wrapped exception.</param>
        public ExceptionWrapper(Exception wrappedException)
        {
            _wrappedException = wrappedException;
        }

        /// <summary>
        /// Gets a string representation of the immediate frames on the call stack.
        /// </summary>
        /// <value>The stack trace.</value>      
        public override string StackTrace
        {
            get
            {
                return _wrappedException.StackTrace;
            }
        }

        /// <summary>
        /// Gets a collection of key/value pairs that provide additional user-defined information about the exception.
        /// </summary>
        /// <value>The data.</value>
        public override System.Collections.IDictionary Data
        {
            get
            {
                return _wrappedException.Data;
            }
        }

        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Exception" /> that is the root cause of one or more subsequent exceptions.
        /// </summary>
        /// <returns>The first exception thrown in a chain of exceptions. If the <see cref="P:System.Exception.InnerException" /> property of the current exception is a null reference (Nothing in Visual Basic), this property returns the current exception.</returns>
        public override Exception GetBaseException()
        {
            return _wrappedException.GetBaseException();
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            return _wrappedException.Equals(obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _wrappedException.GetHashCode();
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
        /// </PermissionSet>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            _wrappedException.GetObjectData(info, context);
        }

        /// <summary>
        /// Gets or sets a link to the help file associated with this exception.
        /// </summary>
        /// <value>The help link.</value>
        public override string HelpLink
        {
            get
            {
                return _wrappedException.HelpLink;
            }
            set
            {
                _wrappedException.HelpLink = value;
            }
        }

        /// <summary>
        /// Gets a message that describes the current exception.
        /// </summary>
        /// <value>The message.</value>
        public override string Message
        {
            get
            {
                return _wrappedException.Message;
            }
        }

        /// <summary>
        /// Gets or sets the name of the application or the object that causes the error.
        /// </summary>
        /// <value>The source.</value>
        public override string Source
        {
            get
            {
                return _wrappedException.Source;
            }
            set
            {
                _wrappedException.Source = value;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" PathDiscovery="*AllFiles*" />
        /// </PermissionSet>
        public override string ToString()
        {
            return _wrappedException.ToString();
        }
    }
}
