using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Represents reference to some drawing definition. Can be stored
    /// in drawing slot to expressing contained - container relation ship.
    /// </summary>
    public class DrawingReference
    {
        /// <summary>
        /// Id of referenced drawing definition.
        /// </summary>
        public readonly string DefinitionID;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingReference"/> class.
        /// </summary>
        /// <param name="definitionID">Id of referenced drawing definition.</param>
        public DrawingReference(string definitionID)
        {
            DefinitionID = definitionID;
        }
    }
}
