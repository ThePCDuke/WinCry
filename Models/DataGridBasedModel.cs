using System.Collections.ObjectModel;

namespace WinCry.Models
{
    class DataGridBasedModel
    {
        /// <summary>
        /// Changes IsChecked property of all ISelectable items based on parameters
        /// </summary>
        /// <param name="itemsCollection">Collection of items to change</param>
        /// <param name="parameters">Parameters as RelayCommandSender</param>
        public static void ChangeIsSelectedPropertyToAll<T>(ObservableCollection<T> itemsCollection, object parameters)
        {
            RelayCommandSender _sender = parameters as RelayCommandSender;

            foreach (ISelectable _item in itemsCollection)
            {
                _item.IsChecked = _sender.IsSelected;
            }
        }

        /// <summary>
        /// Changes IsChecked property of all ISelectable items of named category based on parameters
        /// </summary>
        /// <param name="itemsCollection">Collection of items to change</param>
        /// <param name="parameters">Parameters as RelayCommandSender</param>
        public static void ChangeIsSelectedPropertyToCategory<T>(ObservableCollection<T> itemsCollection, object parameters)
        {
            RelayCommandSender _sender = parameters as RelayCommandSender;

            foreach (ISelectable _item in itemsCollection)
            {
                if (_item.Category == _sender.Name)
                    _item.IsChecked = _sender.IsSelected;
            }
        }

        /// <summary>
        /// Inverts IsChecked property of all ISelectable items
        /// </summary>
        /// <param name="itemsCollection">Collection of items to invert</param>
        public static void InvertIsSelectedProperty<T>(ObservableCollection<T> itemsCollection)
        {
            foreach (ISelectable _item in itemsCollection)
            {
                _item.IsChecked = !_item.IsChecked;
            }
        }

        /// <summary>
        /// Inverts IsChecked property of ISelectable one or multiple items
        /// </summary>
        /// <param name="obj">One or multiple items as object</param>
        /// <param name="multiple">Is it collection or not</param>
        public static void InvertIsSelectedProperty(object obj, bool multiple)
        {
            if (multiple)
            {
                if (obj is ObservableCollection<object> _items && _items.Count != 0)
                {
                    ISelectable _firstItem = (ISelectable)_items[0];

                    bool _isChecked = !_firstItem.IsChecked;

                    foreach (ISelectable _item in _items)
                    {
                        _item.IsChecked = _isChecked;
                    }
                }
            }
            else
            {
                if (obj is ISelectable _currentItem)
                    _currentItem.IsChecked = !_currentItem.IsChecked;
            }
        }
    }
}