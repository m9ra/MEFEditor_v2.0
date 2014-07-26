using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Definition for displaying command actions
    /// </summary>
    public class CommandDefinition
    {
        internal readonly string Name;

        internal readonly Action Command;

        public CommandDefinition(string name, Action command)
        {
            Name = name;
            Command = command;
        }
    }
}
