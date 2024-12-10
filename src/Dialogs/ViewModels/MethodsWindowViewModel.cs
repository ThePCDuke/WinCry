using System;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class MethodsWindowViewModel : BaseViewModel, IDialogRequestClose
    {
        #region Public Properties

        private string _dialogCaption;
        public string DialogCaption
        {
            get { return _dialogCaption; }
            set
            {
                _dialogCaption = value;
                OnPropertyChanged();
            }
        }

        private string _dialogText;
        public string DialogText
        {
            get { return _dialogText; }
            set
            {
                _dialogText = value;
                OnPropertyChanged();
            }
        }

        public byte Method { get; set; }

        #endregion

        #region Commands

        private RelayCommand _main;
        public RelayCommand Main
        {
            get
            {
                return _main ??
                   (_main = new RelayCommand(obj =>
                   {
                       Method = 0;
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
                   }));
            }
        }

        private RelayCommand _alternative;
        public RelayCommand Alternative
        {
            get
            {
                return _alternative ??
                   (_alternative = new RelayCommand(obj =>
                   {
                       Method = 1;
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
                   }));
            }
        }

        private RelayCommand _close;
        public RelayCommand Close
        {
            get
            {
                return _close ??
                   (_close = new RelayCommand(obj =>
                   {
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false));
                   }));
            }
        }

        #endregion

        #region Events

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #endregion
    }
}
