using System.ComponentModel;
using System.Runtime.CompilerServices;
using WinCry.Services;
using WinCry.Tweaks;

namespace WinCry.Settings
{
    public class Setting : INotifyPropertyChanged
    {
        private bool _removeShortcutIcon;
        public bool RemoveShortcutIcon
        {
            get { return _removeShortcutIcon; }
            set
            {
                _removeShortcutIcon = value;
                OnPropertyChanged();
            }
        }

        private bool _doTimerTweak;
        public bool DoTimerTweak
        {
            get { return _doTimerTweak; }
            set
            {
                _doTimerTweak = value;
                OnPropertyChanged();
            }
        }

        private bool _doSchemeTweak;
        public bool DoSchemeTweak
        {
            get { return _doSchemeTweak; }
            set
            {
                _doSchemeTweak = value;
                OnPropertyChanged();
            }
        }

        private bool _doPrimaryGPUTweak;
        public bool DoPrimaryGPUTweak
        {
            get { return _doPrimaryGPUTweak; }
            set
            {
                _doPrimaryGPUTweak = value;
                OnPropertyChanged();
            }
        }

        private bool _doSecondaryGPUTweak;
        public bool DoSecondaryGPUTweak
        {
            get { return _doSecondaryGPUTweak; }
            set
            {
                _doSecondaryGPUTweak = value;
                OnPropertyChanged();
            }
        }

        private bool _increaseBootSpeed;
        public bool IncreaseBootSpeed
        {
            get { return _increaseBootSpeed; }
            set
            {
                _increaseBootSpeed = value;
                OnPropertyChanged();
            }
        }

        private bool _doTempTweak;
        public bool DoTempTweak
        {
            get { return _doTempTweak; }
            set
            {
                _doTempTweak = value;
                OnPropertyChanged();
            }
        }

        private bool _setTempVariable;
        public bool SetTempVariable
        {
            get { return _setTempVariable; }
            set
            {
                _setTempVariable = value;
                OnPropertyChanged();
            }
        }

        private bool _removeNotificationIcon;
        public bool RemoveNotificationIcon
        {
            get { return _removeNotificationIcon; }
            set
            {
                _removeNotificationIcon = value;
                OnPropertyChanged();
            }
        }

        private bool _enableDirectPlay;
        public bool EnableDirectPlay
        {
            get { return _enableDirectPlay; }
            set
            {
                _enableDirectPlay = value;
                OnPropertyChanged();
            }
        }

        private bool _removeFoldersFromThisPC;
        public bool RemoveFoldersFromThisPC
        {
            get { return _removeFoldersFromThisPC; }
            set
            {
                _removeFoldersFromThisPC = value;
                OnPropertyChanged();
            }
        }

        private bool _doPagefileTweak;
        public bool DoPagefileTweak
        {
            get { return _doPagefileTweak; }
            set
            {
                _doPagefileTweak = value;
                OnPropertyChanged();
            }
        }

        private byte _pagefileOption;
        public byte PagefileOption
        {
            get { return _pagefileOption; }
            set
            {
                _pagefileOption = value;
                OnPropertyChanged();
            }
        }

        #region OnProperty Changed
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}