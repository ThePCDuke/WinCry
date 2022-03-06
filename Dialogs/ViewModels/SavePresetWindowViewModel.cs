using System;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class SavePresetWindowViewModel: BaseViewModel, IDialogRequestClose
    {
        #region Public Properties

        private string _caption;
        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                OnPropertyChanged();
            }
        }

        private string _fileName;
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
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

        private RelayCommand _cancel;

        public RelayCommand Cancel
        {
            get
            {
                return _cancel ??
                   (_cancel = new RelayCommand(obj =>
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