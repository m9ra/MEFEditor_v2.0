using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace MEFEditor.Plugin.GUI
{
    /// <summary>
    /// Class representing settings section of editor.
    /// </summary>
    public class SettingsSection : Border
    {
        /// <summary>
        /// The settings items
        /// </summary>
        private readonly StackPanel _items = new StackPanel();

        /// <summary>
        /// The settings title
        /// </summary>
        private readonly TextBlock _title = new TextBlock();

        /// <summary>
        /// The settings items indexes
        /// </summary>
        private Dictionary<object, FrameworkElement> _itemsIndexes = new Dictionary<object, FrameworkElement>();

        /// <summary>
        /// Children of settings section.
        /// </summary>
        /// <value>The children.</value>
        public UIElementCollection Children { get { return _items.Children; } }

        /// <summary>
        /// Sets the title of settings section.
        /// </summary>
        /// <value>The title.</value>
        public string Title
        {
            set
            {
                _title.Text = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsSection"/> class.
        /// </summary>
        public SettingsSection()
        {
            _title.FontSize = 15;
            _title.FontStyle = FontStyles.Italic;
            _title.Margin = new Thickness(5);

            var children = new StackPanel();

            children.Children.Add(_title);
            children.Children.Add(_items);

            Child = children;

            _items.Margin = new Thickness(1);

            Margin = new Thickness(0, 0, 0, 20);
            Background = GUIColors.SectionBackground;
        }

        /// <summary>
        /// Adds the settings item according to given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="item">The item.</param>
        public void AddItem(object key, FrameworkElement item)
        {
            _items.Children.Insert(0,item);
            _itemsIndexes.Add(key, item);
        }

        /// <summary>
        /// Removes the item with given key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void RemoveItem(object key)
        {
            var item = _itemsIndexes[key];
            _itemsIndexes.Remove(key);

            _items.Children.Remove(item);
        }
    }
}
