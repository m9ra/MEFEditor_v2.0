using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drawing
{
    public class DrawingContext
    {
        private readonly Dictionary<string,DrawingDefinition> _definitions = new Dictionary<string,DrawingDefinition>();

        private readonly Dictionary<string, JoinPointDefinitions> _joinPointDefintions = new Dictionary<string, JoinPointDefinitions>();

        private readonly HashSet<JoinDefinition> _joinDefinitions = new HashSet<JoinDefinition>();

        public IEnumerable<DrawingDefinition> Definitions { get { return _definitions.Values; } }

        public int Count { get { return _definitions.Count; } }

        public void Add(DrawingDefinition drawing)
        {
            if (ContainsDrawing(drawing.ID))
                throw new NotSupportedException("Drawing definition with same ID has already been added");

            _definitions.Add(drawing.ID,drawing);
        }

        public bool ContainsDrawing(string id)
        {
            return _definitions.ContainsKey(id);
        }

        public JoinPointDefinition DrawJoinPoint(DrawingReference owningDrawing,object pointKey)
        {
            JoinPointDefinitions joins;

            if(!_joinPointDefintions.TryGetValue(owningDrawing.DefinitionID,out joins)){
                joins=new JoinPointDefinitions();
                _joinPointDefintions[owningDrawing.DefinitionID]=joins;
            }

            JoinPointDefinition result;
            if(!joins.TryGetValue(pointKey,out result)){
                result=new JoinPointDefinition(owningDrawing);
                joins[pointKey]=result;
            }

            return result;
        }

        public void DrawJoin(JoinDefinition join)
        {
            _joinDefinitions.Add(join);
        }
    }
}
