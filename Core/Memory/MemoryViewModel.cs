using PСSLC.Core;
using System.Threading.Tasks;
using WinCry.Dialogs;
using WinCry.Dialogs.ViewModels;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Memory
{
    class MemoryViewModel : BaseViewModel
    {
        #region Private Members

        readonly IDialogService _dialogService;

        #endregion

        #region Public Properties

        public ulong CachedRAMGreaterThan
        {
            get { return Data.CachedRAMGreaterThan; }
            set
            {
                Data.CachedRAMGreaterThan = value;
                OnPropertyChanged();
                if (IsServiceInstalled)
                    MemoryModel.WriteUserPreferences(new RegulationsData(CachedRAMGreaterThan, FreeRAMLessThan, ServiceThreadSleepSeconds * 1000));
            }
        }

        public ulong FreeRAMLessThan
        {
            get { return Data.FreeRAMLessThan; }
            set
            {
                Data.FreeRAMLessThan = value;
                OnPropertyChanged();
                if (IsServiceInstalled)
                    MemoryModel.WriteUserPreferences(new RegulationsData(CachedRAMGreaterThan, FreeRAMLessThan, ServiceThreadSleepSeconds * 1000));
            }
        }

        public int ServiceThreadSleepSeconds
        {
            get { return Data.ServiceThreadSleepSeconds; }
            set
            {
                Data.ServiceThreadSleepSeconds = value;
                OnPropertyChanged();
                if (IsServiceInstalled)
                    MemoryModel.WriteUserPreferences(new RegulationsData(CachedRAMGreaterThan, FreeRAMLessThan, ServiceThreadSleepSeconds * 1000));
            }
        }

        private MemoryData _data;
        public MemoryData Data
        {
            get { return _data; }
            set
            {
                _data = value;
                OnPropertyChanged("CachedRAMGreaterThan");
                OnPropertyChanged("FreeRAMLessThan");
                OnPropertyChanged("ServiceThreadSleepSeconds");
            }
        }

        private bool _isServiceInstalled;
        public bool IsServiceInstalled
        {
            get { return _isServiceInstalled; }
            set
            {
                _isServiceInstalled = value;
                OnPropertyChanged();
            }
        }

        private bool _isServiceRunning;
        public bool IsServiceRunning
        {
            get { return _isServiceRunning; }
            set
            {
                _isServiceRunning = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public MemoryViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            Data = new MemoryData();

            IsServiceInstalled = MemoryModel.IsServiceInstalled;
            IsServiceRunning = MemoryModel.IsServiceRunning;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Updates status 10 times with 0.5 second delay
        /// </summary>
        public async void UpdateStatus()
        {
            int _counts = 0;
            while (_counts < 10)
            {
                IsServiceInstalled = MemoryModel.IsServiceInstalled;
                IsServiceRunning = MemoryModel.IsServiceRunning;

                await Task.Delay(500);
                _counts += 1;
            }
        }

        #endregion

        #region Commands

        private RelayCommand _installUninstallService;
        public RelayCommand InstallUninstallService
        {
            get
            {
                return _installUninstallService ??
                   (_installUninstallService = new RelayCommand(obj =>
                   {
                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();
                       var _taskVM = new TaskViewModel();

                       if (!MemoryModel.IsServiceInstalled)
                       {
                           if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogInstallMemoryTweakCaption, DialogConsts.BaseDialogInstallMemoryTweakMessage))
                               return;

                           try
                           {
                               MemoryModel.ValidateRegulationsData(Data);
                           }
                           catch (System.Exception ex)
                           {
                               DialogHelper.ShowMessageDialog(_dialogService, DialogConsts.Error, $"{ex.Message}");
                               return;
                           }

                           _vm.AddTask(MemoryModel.InstallService(_taskVM, CachedRAMGreaterThan, FreeRAMLessThan, ServiceThreadSleepSeconds * 1000), _taskVM, UpdateStatus);
                       }
                       else
                       {
                           if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogUninstallMemoryTweakCaption, DialogConsts.BaseDialogUninstallMemoryTweakMessage))
                               return;

                           _vm.AddTask(MemoryModel.UninstallService(_taskVM), _taskVM, UpdateStatus);
                       }

                       // Starting all tasks
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);

                       if (DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogRebootCaption, DialogConsts.BaseDialogRebootMessage))
                       {
                           Helpers.RunByCMD("shutdown /r /t 0");
                       }
                   }));
            }
        }

        private RelayCommand _startStopService;
        public RelayCommand StartStopService
        {
            get
            {
                return _startStopService ??
                   (_startStopService = new RelayCommand(obj =>
                   {
                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();
                       var _taskVM = new TaskViewModel();

                       if (!IsServiceRunning)
                       {
                           try
                           {
                               MemoryModel.ValidateRegulationsData(Data);
                           }
                           catch (System.Exception ex)
                           {
                               DialogHelper.ShowMessageDialog(_dialogService, DialogConsts.Error, $"{ex.Message}");
                               return;
                           }

                           _vm.AddTask(MemoryModel.StartService(_taskVM, CachedRAMGreaterThan, FreeRAMLessThan, ServiceThreadSleepSeconds * 1000), _taskVM, UpdateStatus);
                       }
                       else
                       {
                           _vm.AddTask(MemoryModel.StopService(_taskVM), _taskVM, UpdateStatus);
                       }

                       // Starting all tasks
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);
                   }));
            }
        }

        private RelayCommand _stopService;
        public RelayCommand StopService
        {
            get
            {
                return _stopService ??
                   (_stopService = new RelayCommand(obj =>
                   {
                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();

                       var _taskVM = new TaskViewModel();


                       // Starting all tasks
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);
                   }));
            }
        }

        #endregion
    }
}