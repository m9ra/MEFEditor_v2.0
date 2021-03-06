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

using MEFEditor.Drawing;

namespace RecommendedExtensions.Core.Drawings
{
    /// <summary>
    /// Drawing definition for component drawing.
    /// </summary>
    public partial class ComponentDrawing : ContentDrawing
    {
        /// <summary>
        /// Cached image for icon.
        /// </summary>
        protected static readonly CachedImage Image = new CachedImage(Icons.Component);

        /// <summary>
        /// Cached image for remove icon.
        /// </summary>
        protected static readonly CachedImage RemoveImage = new CachedImage(Icons.Remove);

        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentDrawing"/> class.
        /// </summary>
        /// <param name="item">The item.</param>
        public ComponentDrawing(DiagramItem item)
            : base(item)
        {
            InitializeComponent();

            DrawingTools.SetImage(ComponentIcon, Image);

            TypeName.Text = Definition.DrawedType;
            InstanceID.Text = Definition.ID;

            var properties = Definition.Properties.OrderBy((prop) => prop.Name);

            string definingAssembly = null;
            string removedBy = null;
            var isEntryInstance = false;

            foreach (var property in properties)
            {
                var value = property.Value;
                var name = property.Name;

                switch (name)
                {
                    case "EntryInstance":
                        isEntryInstance = true;
                        break;
                    case "Removed":
                        removedBy = value;
                        break;
                    case "DefiningAssembly":
                        definingAssembly = value;
                        break;
                    default:
                        var prefix = value == null || value == "" ? name : name + ": ";
                        var propertyBlock = new TextBlock();
                        propertyBlock.Text = prefix + value;
                        Properties.Children.Add(propertyBlock);
                        break;
                }
            }

            if (isEntryInstance)
            {
                BorderBrush = Brushes.DarkGreen;
                BorderThickness = new Thickness(6);
            }

            var isRemoved = removedBy != null;
            if (isRemoved)
            {
                DrawingTools.SetImage(RemoveIcon, RemoveImage);
                DrawingTools.SetToolTip(RemoveIcon, DrawingTools.GetText("Component has been removed " + removedBy));
            }

            if (definingAssembly != null)
            {
                var assembly = DrawingTools.GetHeadingText("Defining assembly", definingAssembly);
                DrawingTools.SetToolTip(TypeName, assembly);
            }

            RemoveIcon.Visibility = isRemoved ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
