using System;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class DisclaimerWindowViewModel : BaseViewModel, IDialogRequestClose
    {
        #region Public Properties

        private bool _doNotShow;
        public bool DoNotShow
        {
            get { return _doNotShow; }
            set
            {
                _doNotShow = value;
                OnPropertyChanged();
            }
        }

        private bool _doBackup;
        public bool DoBackup
        {
            get { return _doBackup; }
            set
            {
                _doBackup = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

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

        private RelayCommand _closeAndBackup;
        public RelayCommand CloseAndBackup
        {
            get
            {
                return _closeAndBackup ??
                   (_closeAndBackup = new RelayCommand(obj =>
                   {
                       DoBackup = true;
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