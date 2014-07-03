using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExtensionPoints
{
    public interface ILogger
    {
        void Log(string message);
    }

    public interface IContent
    {
        string InnerHTML { get; }
    }
}
