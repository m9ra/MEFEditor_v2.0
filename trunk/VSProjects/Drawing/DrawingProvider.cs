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

        private readonly AbstractDiagramFactory _diagramFactory;

        public DrawingProvider(DiagramCanvas output,AbstractDiagramFactory diagramFactory)
        {            
            _engine = new DisplayEngine(output);
            _diagramFactory = diagramFactory;
        }

        public void Display(DrawingContext context)
        {
            foreach (var definition in context.Definitions)
            {
                var drawing=new DiagramItem(definition);

                foreach (var connectorDefinition in context.GetConnectorDefinitions(definition))
                {
                    var connector = _diagramFactory.CreateConnector(connectorDefinition);
                    drawing.Attach(connector);
                }

                var content = _diagramFactory.CreateContent(definition);
                drawing.SetContent(content);
                _engine.AddItem(drawing);
            }

            foreach (var joinDefinition in context.JoinDefinitions)
            {
                var join=_diagramFactory.CreateJoin(joinDefinition);
                _engine.AddJoin(join);
            }

            _engine.Display();
        }
    }
}
