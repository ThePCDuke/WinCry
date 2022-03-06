using System;
using System.Collections.Generic;
using System.Windows;

namespace WinCry.Dialogs
{
    public interface IDialog
    {
        object DataContext { get; set; }
        bool? DialogResult { get; set; }
        Window Owner { get; set; }
        void Close();
        bool? ShowDialog();
        void Show();
    }

    public interface IDialogService
    {
        void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose
                                           where TView : IDialog;

        bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;

        void Show<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose;
    }

    public interface IDialogRequestClose
    {
        event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;
    }

    public class DialogCloseRequestedEventArgs : EventArgs
    {
        public DialogCloseRequestedEventArgs(bool? dialogResult)
        {
            DialogResult = dialogResult;
        }

        public bool? DialogResult { get; }
    }

    public class DialogService : IDialogService
    {
        private readonly Window _owner;

        public DialogService(Window owner)
        {
            _owner = owner;
            Mappings = new Dictionary<Type, Type>();
        }

        public IDictionary<Type, Type> Mappings { get; }

        public void Register<TViewModel, TView>() where TViewModel : IDialogRequestClose
                                                  where TView : IDialog
        {
            if (Mappings.ContainsKey(typeof(TViewModel)))
            {
                throw new ArgumentException($"Type {typeof(TViewModel)} is already mapped to type {typeof(TView)}");
            }

            Mappings.Add(typeof(TViewModel), typeof(TView));
        }

        public bool? ShowDialog<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
        {
            Type _viewType = Mappings[typeof(TViewModel)];

            IDialog _dialog = (IDialog)Activator.CreateInstance(_viewType);

            void _handler(object sender, DialogCloseRequestedEventArgs e)
            {
                viewModel.CloseRequested -= _handler;

                if (e.DialogResult.HasValue)
                {
                    _dialog.DialogResult = e.DialogResult;
                }
                else
                {
                    _dialog.Close();
                }
            }

            viewModel.CloseRequested += _handler;

            _dialog.DataContext = viewModel;
            _dialog.Owner = _owner;

            Models.WindowBlur.ApplyBlur(_owner);

            bool? _result = _dialog.ShowDialog();

            Models.WindowBlur.RemoveBlur(_owner);
            

            return _result;
        }

        public void Show<TViewModel>(TViewModel viewModel) where TViewModel : IDialogRequestClose
        {
            Type _viewType = Mappings[typeof(TViewModel)];

            IDialog _dialog = (IDialog)Activator.CreateInstance(_viewType);

            void _handler(object sender, DialogCloseRequestedEventArgs e)
            {
                viewModel.CloseRequested -= _handler;

                _dialog.Close();
            }

            viewModel.CloseRequested += _handler;

            _dialog.DataContext = viewModel;
            _dialog.Owner = _owner;

            _dialog.Show();
        }
    }
}