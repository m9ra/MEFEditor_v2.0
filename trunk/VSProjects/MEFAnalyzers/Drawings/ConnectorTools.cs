﻿using System;
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
using Drawing;

namespace MEFAnalyzers.Drawings
{
    static class ConnectorTools
    {
        internal static void SetMessages(StackPanel messagesOutput, ConnectorDefinition definition)
        {
            var error = definition.GetPropertyValue("Error");
            var warning = definition.GetPropertyValue("Warning");

            if (error != null)
            {
                var errorImg = new Image();
                DrawingTools.SetIcon(errorImg, Icons.Error);

                var errorDescription = DrawingTools.GetHeadingText("Error", error);
                DrawingTools.SetToolTip(errorImg, errorDescription);

                messagesOutput.Children.Add(errorImg);
            }

            if (warning != null)
            {
                var warningImg = new Image();
                DrawingTools.SetIcon(warningImg, Icons.Warning);

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

                propertiesText.AppendFormat("{0}: {1}\n", map.Key, property.Value);
            }

            var tooltip= DrawingTools.GetHeadingText(heading, propertiesText.ToString());

            DrawingTools.SetToolTip(connector, tooltip);

        }

    }
}