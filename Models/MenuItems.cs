using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace WinCry.Models
{
    class MenuItems
    {
        public static ObservableCollection<MenuItem> BuildFromCollection(ObservableCollection<string> itemsCollection, RelayCommand command, bool select)
        {
            ObservableCollection<MenuItem> _menus = new ObservableCollection<MenuItem>();

            foreach (string _item in itemsCollection)
            {
                _menus.Add(new MenuItem { Header = _item, Command = command, CommandParameter = new RelayCommandSender() { Name = _item, IsSelected = select }, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left, VerticalContentAlignment = System.Windows.VerticalAlignment.Center });
            }

            return _menus;
        }

        public static MenuItem Build(string header, RelayCommand command, bool select)
        {
            MenuItem _menuItem = new MenuItem() { Header = header, Command = command, CommandParameter = new RelayCommandSender() { Name = header, IsSelected = select }, HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left, VerticalContentAlignment = System.Windows.VerticalAlignment.Center };

            return _menuItem;
        }
    }
}
