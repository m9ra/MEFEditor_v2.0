using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MEFEditor.Drawing
{
    /// <summary>
    /// Represents reference to some drawing definition. Can be stored
    /// in drawing slot to expressing contained - container relation ship
    /// </summary>
    public class DrawingReference
    {
        /// <summary>
        /// Id of referenced drawing definition
        /// </summary>
        public readonly string DefinitionID;


        public DrawingReference(string definitionID)
        {
            DefinitionID = definitionID;
        }
    }
}
