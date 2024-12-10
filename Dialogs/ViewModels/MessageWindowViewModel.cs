using System;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class MessageWindowViewModel : BaseViewModel, IDialogRequestClose
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

        #endregion

        #region Commands

        private RelayCommand _OK;
        public RelayCommand OK
        {
            get
            {
                return _OK ??
                   (_OK = new RelayCommand(obj =>
                   {
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
                   }));
            }
        }

        #endregion

        #region Events

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #endregion
    }
}
