using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;

namespace Plugin.GUI
{
    public class SettingsSection : Border
    {
        private readonly StackPanel _items = new StackPanel();

        private readonly TextBlock _title = new TextBlock();

        private Dictionary<object, FrameworkElement> _itemsIndexes = new Dictionary<object, FrameworkElement>();

        public UIElementCollection Children { get { return _items.Children; } }

        public string Title
        {
            set
            {
                _title.Text = value;
            }
        }

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

        public void AddItem(object key, FrameworkElement item)
        {
            _items.Children.Add(item);
            _itemsIndexes.Add(key, item);
        }

        public void RemoveItem(object key)
        {
            var item = _itemsIndexes[key];
            _itemsIndexes.Remove(key);

            _items.Children.Remove(item);
        }
    }
}
