﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WinCry.Models
{
    public static class DragProperty
    {
        public static readonly DependencyProperty IsEnabled = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(DragProperty),
            new PropertyMetadata(default(bool), OnLoaded));

        private static void OnLoaded(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var uiElement = dependencyObject as UIElement;
            if (uiElement == null || (dependencyPropertyChangedEventArgs.NewValue is bool) == false)
            {
                return;
            }
            if ((bool)dependencyPropertyChangedEventArgs.NewValue)
            {
                uiElement.MouseMove += UiElementOnMouseMove;
            }
            else
            {
                uiElement.MouseMove -= UiElementOnMouseMove;
            }

        }

        private static void UiElementOnMouseMove(object sender, MouseEventArgs mouseEventArgs)
        {
            var uiElement = sender as UIElement;
            if (uiElement != null && mouseEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                DependencyObject parent = uiElement;
                int avoidInfiniteLoop = 0;

                while ((parent is Window) == false)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                    avoidInfiniteLoop++;
                    if (avoidInfiniteLoop == 1000)
                    {
                        return;
                    }
                }
                var window = parent as Window;
                window.DragMove();
            }
        }

        public static void SetIsEnabled(DependencyObject element, bool value)
        {
            element.SetValue(IsEnabled, value);
        }

        public static bool GetIsEnabled(DependencyObject element)
        {
            return (bool)element.GetValue(IsEnabled);
        }
    }
}
