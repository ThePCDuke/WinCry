using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace WinCry.Models
{
    class RestrictedTextBox : TextBox
    {
        private static readonly Regex regex = new Regex("[^0-9]+");

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            e.Handled = regex.IsMatch(e.Text);
            base.OnPreviewTextInput(e);
        }
    }
}
