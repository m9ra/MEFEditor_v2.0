using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Definition for displaying command actions.
    /// </summary>
    public class CommandDefinition
    {
        /// <summary>
        /// The name of command.
        /// </summary>
        internal readonly string Name;

        /// <summary>
        /// The command action.
        /// </summary>
        internal readonly Action Command;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandDefinition" /> class.
        /// </summary>
        /// <param name="name">The name of command.</param>
        /// <param name="command">The command action.</param>
        public CommandDefinition(string name, Action command)
        {
            Name = name;
            Command = command;
        }
    }
}
