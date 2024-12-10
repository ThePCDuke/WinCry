using System;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class ExpertModeDisclaimerWindowViewModel : BaseViewModel, IDialogRequestClose
    {
        #region Public Properties

        private bool _gotIt;
        public bool GotIt
        {
            get { return _gotIt; }
            set
            {
                _gotIt = value;
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

        #endregion

        #region Events

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #endregion
    }
}