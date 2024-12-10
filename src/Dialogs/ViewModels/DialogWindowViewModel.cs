using System;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class DialogWindowViewModel : BaseViewModel, IDialogRequestClose
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

        private RelayCommand _yes;
        public RelayCommand Yes
        {
            get
            {
                return _yes ??
                   (_yes = new RelayCommand(obj =>
                   {
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
                   }));
            }
        }

        private RelayCommand _no;
        public RelayCommand No
        {
            get
            {
                return _no ??
                   (_no = new RelayCommand(obj =>
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
