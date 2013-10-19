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

        internal DiagramCanvas Output { get { return _engine.Output; } }

        public DrawingProvider(DiagramCanvas output, AbstractDiagramFactory diagramFactory)
        {
            _engine = new DisplayEngine(output);
            _diagramFactory = diagramFactory;
        }

        public void Display(DiagramDefinition diagramDefinition)
        {
            var context = new DiagramContext(this, diagramDefinition);

            foreach (var definition in context.RootItemDefinitions)
            {
                var item = new DiagramItem(definition, context);
                DrawItem(item);
            }

            foreach (var joinDefinition in diagramDefinition.JoinDefinitions)
            {
                foreach (var from in _engine.DefiningItems(joinDefinition.From))
                {
                    foreach (var to in _engine.DefiningItems(joinDefinition.To))
                    {
                        var join = _diagramFactory.CreateJoin(joinDefinition, context);
                        _engine.AddJoin(join,from,to);
                    }
                }
            }

            _engine.Display();
        }

        internal DiagramItem DrawItem(DiagramItem item)
        {

            foreach (var connectorDefinition in item.ConnectorDefinitions)
            {
                var connector = _diagramFactory.CreateConnector(connectorDefinition, item);
                item.Attach(connector);
            }

            var content = _diagramFactory.CreateContent(item);
            item.SetContent(content);
            _engine.RegisterItem(item);
            return item;
        }
    }
}
