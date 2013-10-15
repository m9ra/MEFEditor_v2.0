using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;

namespace Drawing
{
    public class DrawingProvider
    {
        private readonly DisplayEngine _engine;
        
        public DrawingProvider(DiagramCanvas output)
        {
            //TODO require DefinitionDrawers
            _engine = new DisplayEngine(output);
        }

        public void Display(DrawingContext context)
        {
            foreach (var definition in context.Definitions)
            {
                var drawing=new DiagramItem(definition);

                foreach (var joinPoint in context.GetJoinPointDefinitions(definition))
                {
                    var connector = new Connector(joinPoint);
                    drawing.Attach(connector);
                }

                _engine.AddItem(drawing);
            }

            foreach (var join in context.JoinDefinitions)
            {
                _engine.AddJoin(join);
            }

            _engine.Display();
        }
    }
}
