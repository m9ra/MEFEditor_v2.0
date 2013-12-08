using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class ExceptionWrapper : Exception
    {
        private readonly Exception _wrappedException;

        public ExceptionWrapper(Exception wrappedException)
        {
            _wrappedException = wrappedException;
        }

        public override string StackTrace
        {
            get
            {
                return _wrappedException.StackTrace;
            }
        }

        public override System.Collections.IDictionary Data
        {
            get
            {
                return _wrappedException.Data;
            }
        }

        public override Exception GetBaseException()
        {
            return _wrappedException.GetBaseException();
        }

        public override bool Equals(object obj)
        {
            return _wrappedException.Equals(obj);
        }

        public override int GetHashCode()
        {
            return _wrappedException.GetHashCode();
        }

        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            _wrappedException.GetObjectData(info, context);
        }

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

        public override string Message
        {
            get
            {
                return _wrappedException.Message;
            }
        }

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

        public override string ToString()
        {
            return _wrappedException.ToString();
        }
    }
}
