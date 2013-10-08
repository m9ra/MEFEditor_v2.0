using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;

namespace Drawing
{
    public class DrawingProvider
    {
        private readonly Canvas _output;

        public DrawingProvider(Canvas output)
        {
            //TODO require DefinitionDrawers
            _output = output;
        }

        public void Draw(IEnumerable<DrawingDefinition> definitions)
        {
            var i=0;
            foreach (var definition in definitions)
            {
                var drawing=new TestControl(definition);

                Canvas.SetTop(drawing, i * 200);
                _output.Children.Add(drawing);

                ++i;
            }
        }
    }
}
