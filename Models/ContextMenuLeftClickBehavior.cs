using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

namespace WinCry.Models
{
    public static class ContextMenuLeftClickBehavior
    {
        public static bool GetIsLeftClickEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsLeftClickEnabledProperty);
        }

        public static void SetIsLeftClickEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsLeftClickEnabledProperty, value);
        }

        public static readonly DependencyProperty IsLeftClickEnabledProperty = DependencyProperty.RegisterAttached(
            "IsLeftClickEnabled",
            typeof(bool),
            typeof(ContextMenuLeftClickBehavior),
            new UIPropertyMetadata(false, OnIsLeftClickEnabledChanged));

        private static void OnIsLeftClickEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is UIElement uiElement)
            {
                bool IsEnabled = e.NewValue is bool && (bool)e.NewValue;

                if (IsEnabled)
                {
                    if (uiElement is ButtonBase)
                        ((ButtonBase)uiElement).Click += OnMouseLeftButtonUp;
                    else
                        uiElement.MouseLeftButtonUp += OnMouseLeftButtonUp;
                }
                else
                {
                    if (uiElement is ButtonBase)
                        ((ButtonBase)uiElement).Click -= OnMouseLeftButtonUp;
                    else
                        uiElement.MouseLeftButtonUp -= OnMouseLeftButtonUp;
                }
            }
        }

        private static void OnMouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            FrameworkElement fe = sender as FrameworkElement;
            if (fe != null)
            {
                if (fe.ContextMenu.DataContext == null)
                {
                    fe.ContextMenu.SetBinding(FrameworkElement.DataContextProperty, new Binding { Source = fe.DataContext });
                }

                fe.ContextMenu.IsOpen = true;
            }
        }
    }
}
