using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MEFEditor.Drawing;

namespace MEFAnalyzers.Drawings
{
    static class ConnectorTools
    {
        static readonly CachedImage ErrorImage = new CachedImage(Icons.Error);

        static readonly CachedImage WarningImage = new CachedImage(Icons.Warning);

        internal static void SetMessages(StackPanel messagesOutput, ConnectorDefinition definition)
        {
            var error = definition.GetPropertyValue("Error");
            var warning = definition.GetPropertyValue("Warning");

            if (error != null)
            {
                var errorImg = new Image();
                DrawingTools.SetImage(errorImg, ErrorImage);

                var errorDescription = DrawingTools.GetHeadingText("Error", error);
                DrawingTools.SetToolTip(errorImg, errorDescription);

                messagesOutput.Children.Add(errorImg);
            }

            if (warning != null)
            {
                var warningImg = new Image();
                DrawingTools.SetImage(warningImg, WarningImage);

                var warningDescription = DrawingTools.GetHeadingText("Warning", warning);
                DrawingTools.SetToolTip(warningImg, warningDescription);

                messagesOutput.Children.Add(warningImg);
            }
        }

        internal static void SetProperties(ConnectorDrawing connector, string heading, IEnumerable<KeyValuePair<string, string>> mapping)
        {
            var definition = connector.Definition;

            var propertiesText = new StringBuilder();
            foreach (var map in mapping)
            {
                var property = definition.GetProperty(map.Key);
                if (property == null || property.Value == null)
                    continue;

                propertiesText.AppendFormat("{0}: {1}\n", map.Value, property.Value);
            }

            var metaText = new StringBuilder();
            foreach (var property in definition.Properties)
            {
                var prefix = "$Meta";
                var propertyName = property.Name;
                if (!propertyName.StartsWith(prefix))
                    continue;

                var name = propertyName.Substring(propertyName.IndexOf('-') + 1);
                metaText.AppendFormat("{0}: {1}\n", name, property.Value);
            }

            var tooltip = DrawingTools.GetHeadingText(heading, propertiesText.ToString());

            if (metaText.Length > 0)
            {
                DrawingTools.AppendHeadingText("Metadata", metaText.ToString(), tooltip);
            }

            DrawingTools.SetToolTip(connector, tooltip);

        }

    }
}
