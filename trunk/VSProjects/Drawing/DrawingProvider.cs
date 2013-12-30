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
        internal readonly DisplayEngine Engine;

        private readonly AbstractDiagramFactory _diagramFactory;

        internal DiagramCanvas Output { get { return Engine.Output; } }

        public DrawingProvider(DiagramCanvas output, AbstractDiagramFactory diagramFactory)
        {
            Engine = new DisplayEngine(output);
            _diagramFactory = diagramFactory;
        }

        public DiagramContext Display(DiagramDefinition diagramDefinition)
        {
            Engine.Clear();

            var context = new DiagramContext(this, diagramDefinition);

            foreach (var definition in context.RootItemDefinitions)
            {
                var item = new DiagramItem(definition, context);
                DrawItem(item);
            }

            foreach (var joinDefinition in diagramDefinition.JoinDefinitions)
            {
                foreach (var from in Engine.DefiningItems(joinDefinition.From))
                {
                    foreach (var to in Engine.DefiningItems(joinDefinition.To))
                    {
                        var join = _diagramFactory.CreateJoin(joinDefinition, context);
                        Engine.AddJoin(join,from,to);
                    }
                }
            }

            var menu = new ContextMenu();
            foreach (var edit in diagramDefinition.Edits)
            {
                var item = new MenuItem();
                item.Header = edit.Name;

                item.Click += (e, s) => edit.Commit(context.Diagram.InitialView);
                menu.Items.Add(item);
            }

            Engine.Output.ContextMenu = menu;

            Engine.Output.SetContext(context);
            Engine.Display();

            return context;
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
            Engine.RegisterItem(item);
            return item;
        }
    }
}
