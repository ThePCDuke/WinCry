using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using WinCry.Dialogs;
using WinCry.Dialogs.ViewModels;
using WinCry.Memory;
using WinCry.Models;
using WinCry.Services;
using WinCry.Settings;
using WinCry.Tweaks;

namespace WinCry.ViewModels
{
    class MainWindowViewModel : BaseViewModel
    {
        #region Private Members

        readonly IDialogService _dialogService;

        #endregion

        #region Constructor

        public MainWindowViewModel(IDialogService dialogService)
        {
            try
            {
                ServicesModel.EnableWMIService();
            }
            catch { }

            System.Windows.Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            _dialogService = dialogService;

            SettingsVM = new SettingsViewModel(_dialogService);

            if (Helpers.GetWinBuild() >= 22000)
            {
                IsNotificationIconTweakEnabled = false;
            }
            else
            {
                IsNotificationIconTweakEnabled = true;
            }

            ObservableCollection<string> _detectedGPUs = Helpers.GetInstalledGPUManufacturers();

            if (_detectedGPUs.Count > 0)
            {
                PrimaryGPUManufacturer = _detectedGPUs[0];
            }
            if (_detectedGPUs.Count >= 2)
            {
                SecondaryGPUManufacturer = _detectedGPUs[1];
                IsSecondaryGPUDetected = true;
            }
            else
            {
                SecondaryGPUManufacturer = null;
            }

            Version _version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ApplicationVersion = $"{_version.Major}.{_version.Minor}.{_version.Build}";
        }

        #endregion

        #region Public Properties

        public SettingsViewModel SettingsVM { get; set; }

        public string ApplicationVersion { get; set; }

        public string Username { get; set; } = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\')[1];

        public bool IsNotificationIconTweakEnabled { get; set; }
        public bool IsPrimaryGPUDetected { get; set; }
        public bool IsSecondaryGPUDetected { get; set; }

        private string _primaryGPUManufacturer;
        public string PrimaryGPUManufacturer
        {
            get { return _primaryGPUManufacturer; }
            set
            {
                _primaryGPUManufacturer = value;

                if (value != null)
                    IsPrimaryGPUDetected = true;
                else
                {
                    SettingsVM.Settings.DoPrimaryGPUTweak = false;
                    IsPrimaryGPUDetected = false;
                }
            }
        }

        private string _secondaryGPUManufacturer;
        public string SecondaryGPUManufacturer
        {
            get { return _secondaryGPUManufacturer; }
            set
            {
                _secondaryGPUManufacturer = value;

                if (value != null)
                    IsSecondaryGPUDetected = true;
                else
                {
                    SettingsVM.Settings.DoSecondaryGPUTweak = false;
                    IsSecondaryGPUDetected = false;
                }
            }
        }

        private byte _selectedTab;
        public byte SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                _selectedTab = value;
                OnPropertyChanged();

                if (value == 7)
                    Environment.Exit(0);
            }
        }

        #endregion

        #region ApplyingCommands

        private RelayCommand _quickApply;
        public RelayCommand QuickApply
        {
            get
            {
                return _quickApply ??
                   (_quickApply = new RelayCommand(obj =>
                   {
                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogQuickApplyCaption, DialogConsts.BaseDialogQuickApplyMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();

                       // Timer Tweak
                       if (SettingsVM.Settings.DoTimerTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallImprovedTimer(_taskVM, true), _taskVM); }
                       // Scheme Tweak
                       if (SettingsVM.Settings.DoSchemeTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallPowerScheme(_taskVM, true), _taskVM); }
                       // GPU Tweak
                       if (SettingsVM.Settings.DoPrimaryGPUTweak || SettingsVM.Settings.DoSecondaryGPUTweak)
                       { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetGPUSettings(_taskVM, true, SettingsVM.Settings.DoPrimaryGPUTweak, SettingsVM.Settings.DoSecondaryGPUTweak), _taskVM); }
                       // Pagefile Tweak
                       if (SettingsVM.Settings.DoPagefileTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetPagefile(_taskVM, SettingsVM.Settings.PagefileOption), _taskVM); }
                       // Auto Temp Cleaner Tweak
                       if (SettingsVM.Settings.DoTempTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallAutoTempCleaner(_taskVM, true), _taskVM); }

                       // Tweaks
                       if (SettingsVM.TweaksVM.Option != TweaksOption.Nothing && SettingsVM.TweaksVM.Tweaks.Where(t => t.IsChecked).Count() != 0) { var _taskVM = new TaskViewModel(); _vm.AddTask(TweaksModel.Apply(SettingsVM.TweaksVM.Tweaks, SettingsVM.TweaksVM.Option, _taskVM), _taskVM); }
                       // Services
                       if (SettingsVM.ServicesVM.Option == ServicesOption.Delete) { ServicesModel.CheckServicesForRemovability(SettingsVM.ServicesVM.Services, _dialogService); }
                       if (SettingsVM.ServicesVM.Option == ServicesOption.Disable) { ServicesModel.CheckServicesForDisability(SettingsVM.ServicesVM.Services, _dialogService); }
                       if (SettingsVM.ServicesVM.Option != ServicesOption.Nothing && SettingsVM.ServicesVM.Services.Where(s => s.IsChecked).Count() != 0) { var _taskVM = new TaskViewModel(); _vm.AddTask(ServicesModel.ApplyTask(SettingsVM.ServicesVM.Services, SettingsVM.ServicesVM.Option, _taskVM), _taskVM); }
                       // Memory
                       if (!MemoryModel.IsServiceInstalled) { var _taskVM = new TaskViewModel(); _vm.AddTask(MemoryModel.InstallService(_taskVM, SettingsVM.MemoryVM.CachedRAMGreaterThan, SettingsVM.MemoryVM.FreeRAMLessThan, SettingsVM.MemoryVM.ServiceThreadSleepSeconds * 1000), _taskVM, SettingsVM.MemoryVM.UpdateStatus); }

                       // Shortcut icon Tweak
                       if (SettingsVM.Settings.RemoveShortcutIcon) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveShortcutIcon(_taskVM, true), _taskVM); }
                       // Increase boot speed Tweak
                       if (SettingsVM.Settings.IncreaseBootSpeed) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.IncreaseBootSpeed(_taskVM, true), _taskVM); }
                       // Set Temp Variable Tweak
                       if (SettingsVM.Settings.SetTempVariable) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetTempVariables(_taskVM, true), _taskVM); }
                       // Remove Notification Icon Tweak
                       if (SettingsVM.Settings.RemoveNotificationIcon) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveNotificationIcon(_taskVM, true), _taskVM); }
                       // Enable DirectPlay Tweak
                       if (SettingsVM.Settings.EnableDirectPlay) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetDirectPlayComponent(_taskVM, true), _taskVM); }
                       // Remove Folders from this pc Tweak
                       if (SettingsVM.Settings.RemoveFoldersFromThisPC) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveFoldersFromThisPC(_taskVM, true), _taskVM); }

                       // Starting all tasks
                       _vm.StartTasks();

                       if (_vm.TaskViewModels.Count != 0)
                           _dialogService.ShowDialog(_vm);
                       else
                           return;

                       if (DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogRebootCaption, DialogConsts.BaseDialogRebootMessage))
                       {
                           Helpers.RunByCMD("shutdown /r /t 0");
                       }
                   }));
            }
        }

        private RelayCommand _applyTweaks;
        public RelayCommand ApplyTweaks
        {
            get
            {
                return _applyTweaks ??
                   (_applyTweaks = new RelayCommand(obj =>
                   {
                       SettingsVM.TweaksVM.Apply.Execute(null);
                   }));
            }
        }

        private RelayCommand _applyServices;
        public RelayCommand ApplyServices
        {
            get
            {
                return _applyServices ??
                   (_applyServices = new RelayCommand(obj =>
                   {
                       SettingsVM.ServicesVM.Apply.Execute(null);
                   }));
            }
        }

        private RelayCommand _applyMemory;
        public RelayCommand ApplyMemory
        {
            get
            {
                return _applyMemory ??
                   (_applyMemory = new RelayCommand(obj =>
                   {
                       SettingsVM.MemoryVM.InstallUninstallService.Execute(null);
                   }));
            }
        }

        private RelayCommand _applyMain;
        public RelayCommand ApplyMain
        {
            get
            {
                return _applyMain ??
                   (_applyMain = new RelayCommand(obj =>
                   {
                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogApplyMainSettingsCaption, DialogConsts.BaseDialogApplyMainSettingsMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();

                       // Timer Tweak
                       if (SettingsVM.Settings.DoTimerTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallImprovedTimer(_taskVM, true), _taskVM); }
                       // Scheme Tweak
                       if (SettingsVM.Settings.DoSchemeTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallPowerScheme(_taskVM, true), _taskVM); }
                       // GPU Tweak
                       if (SettingsVM.Settings.DoPrimaryGPUTweak || SettingsVM.Settings.DoSecondaryGPUTweak)
                       { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetGPUSettings(_taskVM, true, SettingsVM.Settings.DoPrimaryGPUTweak, SettingsVM.Settings.DoSecondaryGPUTweak), _taskVM); }
                       // Pagefile Tweak
                       if (SettingsVM.Settings.DoPagefileTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetPagefile(_taskVM, SettingsVM.Settings.PagefileOption), _taskVM); }
                       // Auto Temp Cleaner Tweak
                       if (SettingsVM.Settings.DoTempTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallAutoTempCleaner(_taskVM, true), _taskVM); }

                       // Starting all tasks
                       _vm.StartTasks();

                       if (_vm.TaskViewModels.Count != 0)
                           _dialogService.ShowDialog(_vm);
                       else
                           return;

                       if (DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogRebootCaption, DialogConsts.BaseDialogRebootMessage))
                       {
                           Helpers.RunByCMD("shutdown /r /t 0");
                       }
                   }));
            }
        }

        private RelayCommand _applyOther;
        public RelayCommand ApplyOther
        {
            get
            {
                return _applyOther ??
                   (_applyOther = new RelayCommand(obj =>
                   {
                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogApplyOtherSettingsCaption, DialogConsts.BaseDialogApplyOtherSettingsMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();

                       // Shortcut icon Tweak
                       if (SettingsVM.Settings.RemoveShortcutIcon) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveShortcutIcon(_taskVM, true), _taskVM); }
                       // Increase boot speed Tweak
                       if (SettingsVM.Settings.IncreaseBootSpeed) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.IncreaseBootSpeed(_taskVM, true), _taskVM); }
                       // Set Temp Variable Tweak
                       if (SettingsVM.Settings.SetTempVariable) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetTempVariables(_taskVM, true), _taskVM); }
                       // Remove Notification Icon Tweak
                       if (SettingsVM.Settings.RemoveNotificationIcon) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveNotificationIcon(_taskVM, true), _taskVM); }
                       // Enable DirectPlay Tweak
                       if (SettingsVM.Settings.EnableDirectPlay) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetDirectPlayComponent(_taskVM, true), _taskVM); }
                       // Remove Folders from this pc Tweak
                       if (SettingsVM.Settings.RemoveFoldersFromThisPC) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveFoldersFromThisPC(_taskVM, true), _taskVM); }

                       // Starting all tasks
                       _vm.StartTasks();

                       if (_vm.TaskViewModels.Count != 0)
                           _dialogService.ShowDialog(_vm);
                       else
                           return;

                       if (DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogRebootCaption, DialogConsts.BaseDialogRebootMessage))
                       {
                           Helpers.RunByCMD("shutdown /r /t 0");
                       }
                   }));
            }
        }

        #endregion

        #region OtherCommands

        private RelayCommand _activateWindows;
        public RelayCommand ActivateWindows
        {
            get
            {
                return _activateWindows ??
                   (_activateWindows = new RelayCommand(async obj =>
                   {
                       (bool? close, byte method) tuple = DialogHelper.ShowMethodWindow(_dialogService, DialogConsts.BaseDialogActivateWindowsCaption, DialogConsts.BaseDialogActivateWindowsMessage);

                       if (tuple.close == false)
                           return;

                       await MainModel.CheckActivationServices(_dialogService, tuple.method);

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();
                       var _taskVM = new TaskViewModel();

                       _vm.AddTask(MainModel.ActivateWindows(_taskVM, tuple.method), _taskVM);
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);
                   }));
            }
        }

        private RelayCommand _installVC;
        public RelayCommand InstallVC
        {
            get
            {
                return _installVC ??
                   (_installVC = new RelayCommand(obj =>
                   {
                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogInstallVCCaption, DialogConsts.BaseDialogInstallVCMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();
                       var _taskVM = new TaskViewModel();

                       _vm.AddTask(MainModel.InstallVC(_taskVM, _dialogService), _taskVM);
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);
                   }));
            }
        }

        private RelayCommand _installMSStore;
        public RelayCommand InstallMSStore
        {
            get
            {
                return _installMSStore ??
                   (_installMSStore = new RelayCommand(async obj =>
                   {
                       (bool? close, byte method) tuple = DialogHelper.ShowMethodWindow(_dialogService, DialogConsts.BaseDialogInstallMSStoreCaption, DialogConsts.BaseDialogInstallMSStoreMessage);

                       if (tuple.close == false)
                           return;

                       MainModel.DownloadMSStore(_dialogService);

                       await MainModel.CheckMSStoreServices(_dialogService);

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();
                       var _taskVM = new TaskViewModel();

                       _vm.AddTask(MainModel.InstallMSStore(_taskVM, tuple.method), _taskVM);
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);
                   }));
            }
        }

        private RelayCommand _uninstallMSStore;
        public RelayCommand UninstallMSStore
        {
            get
            {
                return _uninstallMSStore ??
                   (_uninstallMSStore = new RelayCommand(obj =>
                   {
                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogUninstallMSStoreCaption, DialogConsts.BaseDialogUninstallMSStoreMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();
                       var _taskVM = new TaskViewModel();

                       _vm.AddTask(MainModel.UninstallMSStore(_taskVM), _taskVM);
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);
                   }));
            }
        }

        private RelayCommand _runAsTI;
        public RelayCommand RunAsTI
        {
            get
            {
                return _runAsTI ??
                   (_runAsTI = new RelayCommand(obj =>
                   {
                       Microsoft.Win32.OpenFileDialog _dialog = new Microsoft.Win32.OpenFileDialog
                       {
                           Filter = "Executable (*.exe;*.bat;*.reg)|*.exe;*.bat;*.reg;|All Files|*.*"
                       };

                       if (_dialog.ShowDialog() == true)
                       {
                           RunAsProcess.CMD($"\"{_dialog.FileName}\"", true, false);
                       }
                   }));
            }
        }

        private RelayCommand _runCMDAsTI;
        public RelayCommand RunCMDAsTI
        {
            get
            {
                return _runCMDAsTI ??
                   (_runCMDAsTI = new RelayCommand(obj =>
                   {
                       RunAsProcess.Binary($@"{Path.GetPathRoot(Environment.SystemDirectory)}\Windows\System32\cmd.exe", false, false);
                   }));
            }
        }

        private RelayCommand _runRegeditAsTI;
        public RelayCommand RunRegeditAsTI
        {
            get
            {
                return _runRegeditAsTI ??
                   (_runRegeditAsTI = new RelayCommand(obj =>
                   {
                       RunAsProcess.Binary($@"{Path.GetPathRoot(Environment.SystemDirectory)}\Windows\regedit.exe", false, false);
                   }));
            }
        }

        #endregion

        #region SettingsCommands

        private RelayCommand _updateLists;
        public RelayCommand UpdateLists
        {
            get
            {
                return _updateLists ??
                   (_updateLists = new RelayCommand(obj =>
                   {
                       SettingsVM.LoadPresetsList();
                       SettingsVM.TweaksVM.LoadPresetsList();
                       SettingsVM.ServicesVM.LoadPresetsList();
                       SettingsVM.LoadDefaultPreset();
                   }));
            }
        }

        private RelayCommand _openPresetsFolder;
        public RelayCommand OpenPresetsFolder
        {
            get
            {
                return _openPresetsFolder ??
                   (_openPresetsFolder = new RelayCommand(obj =>
                   {
                       MainModel.CreatePresetsFolder();
                       Process.Start(@"Presets");
                   }));
            }
        }

        private RelayCommand _reset;
        public RelayCommand Reset
        {
            get
            {
                return _reset ??
                   (_reset = new RelayCommand(obj =>
                   {
                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogResetCaption, DialogConsts.BaseDialogResetMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel();

                       // Timer Tweak
                       if (SettingsVM.Settings.DoTimerTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallImprovedTimer(_taskVM, false), _taskVM); }
                       // Scheme Tweak
                       if (SettingsVM.Settings.DoSchemeTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallPowerScheme(_taskVM, false), _taskVM); }
                       // GPU Tweak
                       if (SettingsVM.Settings.DoPrimaryGPUTweak || SettingsVM.Settings.DoSecondaryGPUTweak)
                       { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetGPUSettings(_taskVM, false, SettingsVM.Settings.DoPrimaryGPUTweak, SettingsVM.Settings.DoSecondaryGPUTweak), _taskVM); }
                       // Pagefile Tweak
                       if (SettingsVM.Settings.DoPagefileTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetPagefile(_taskVM, 0), _taskVM); }

                       // Shortcut icon Tweak
                       if (SettingsVM.Settings.RemoveShortcutIcon) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveShortcutIcon(_taskVM, false), _taskVM); }
                       // Increase boot speed Tweak
                       if (SettingsVM.Settings.IncreaseBootSpeed) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.IncreaseBootSpeed(_taskVM, false), _taskVM); }
                       // Auto Temp Cleaner Tweak
                       if (SettingsVM.Settings.DoTempTweak) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.InstallAutoTempCleaner(_taskVM, false), _taskVM); }
                       // Set Temp Variable Tweak
                       if (SettingsVM.Settings.SetTempVariable) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetTempVariables(_taskVM, false), _taskVM); }
                       // Remove Notification Icon Tweak
                       if (SettingsVM.Settings.RemoveNotificationIcon) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveNotificationIcon(_taskVM, false), _taskVM); }
                       // Enable DirectPlay Tweak
                       if (SettingsVM.Settings.EnableDirectPlay) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.SetDirectPlayComponent(_taskVM, false), _taskVM); }
                       // Remove Folders from this pc Tweak
                       if (SettingsVM.Settings.RemoveFoldersFromThisPC) { var _taskVM = new TaskViewModel(); _vm.AddTask(MainModel.RemoveFoldersFromThisPC(_taskVM, false), _taskVM); }

                       // Starting all tasks
                       _vm.StartTasks();

                       if (_vm.TaskViewModels.Count != 0)
                           _dialogService.ShowDialog(_vm);
                       else
                           return;
                   }));
            }
        }

        #endregion

        #region MiscCommands

        private RelayCommand _windowLoaded;
        public RelayCommand WindowLoaded
        {
            get
            {
                return _windowLoaded ??
                   (_windowLoaded = new RelayCommand(obj =>
                   {
                       using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\", true))
                       {
                           bool _doNotShowDisclaimer = false;

                           RegistryKey _wincryKey = _registryKey.OpenSubKey("WinCry", true);

                           if (_wincryKey != null)
                           {
                               if (_wincryKey.GetValue("DoNotShowDisclaimer") != null)
                                   _doNotShowDisclaimer = Byte.Parse((string)_wincryKey.GetValue("DoNotShowDisclaimer")) == 1 ? true : false;
                           }

                           if (!_doNotShowDisclaimer)
                           {
                               (bool doNotShow, bool doBackup) tuple = DialogHelper.ShowDisclaimer(_dialogService);

                               if (tuple.doNotShow)
                               {
                                   _registryKey.CreateSubKey("WinCry");
                                   _wincryKey = _registryKey.OpenSubKey("WinCry", true);
                                   _wincryKey.SetValue("DoNotShowDisclaimer", "1", RegistryValueKind.String);
                               }

                               if (tuple.doBackup)
                               {
                                   ProgressWindowViewModel _vm = new ProgressWindowViewModel() { DialogCaption = DialogConsts.ApplyingCaption };
                                   TaskViewModel _servicesTask = new TaskViewModel();

                                   ObservableCollection<Service> services = new ObservableCollection<Service>();

                                   foreach (Service service in SettingsVM.ServicesVM.Services)
                                   {
                                       services.Add(new Service(service));
                                   }

                                   foreach (Service service in services)
                                   {
                                       service.IsChecked = true;
                                   }

                                   _vm.AddTask(ServicesModel.ApplyTask(services, ServicesOption.Backup, _servicesTask), _servicesTask);
                                   _vm.StartTasks();

                                   _dialogService.ShowDialog(_vm);
                               }
                           }
                       }

                       try
                       {
                           ServicesModel.EnableTrustedInstallerService();
                           ServicesModel.EnableWMIService();
                       }
                       catch (InvalidOperationException)
                       {
                           if (DialogHelper.ShowDialog(_dialogService, DialogConsts.ServiceStartingError, DialogConsts.TrustedInstallerStartingError))
                           {
                               ServicesModel.RestoreTrustedInstallerService();
                               ServicesModel.RestoreWMIService();
                           }
                           else
                           {
                               Environment.Exit(0);
                           }
                       }

                       Helpers.RunByCMD("net stop \"wuauserv\"", false);
                   }));
            }
        }

        private RelayCommand _gotoChannel;
        public RelayCommand GotoChannel
        {
            get
            {
                return _gotoChannel ??
                   (_gotoChannel = new RelayCommand(obj =>
                   {
                       MainModel.GotoChannel();
                   }));
            }
        }

        private RelayCommand _gotoCommunity;
        public RelayCommand GotoCommunity
        {
            get
            {
                return _gotoCommunity ??
                   (_gotoCommunity = new RelayCommand(obj =>
                   {
                       MainModel.GotoCommunity();
                   }));
            }
        }

        private RelayCommand _gotoCommunityDiscussion;
        public RelayCommand GotoCommunityDiscussion
        {
            get
            {
                return _gotoCommunityDiscussion ??
                   (_gotoCommunityDiscussion = new RelayCommand(obj =>
                   {
                       MainModel.GotoCommunityDiscussion();
                   }));
            }
        }

        private RelayCommand _gotoDonation;
        public RelayCommand GotoDonation
        {
            get
            {
                return _gotoDonation ??
                   (_gotoDonation = new RelayCommand(obj =>
                   {
                       MainModel.GotoDonation();
                   }));
            }
        }

        private RelayCommand _gotoGitHub;
        public RelayCommand GotoGitHub
        {
            get
            {
                return _gotoGitHub ??
                   (_gotoGitHub = new RelayCommand(obj =>
                   {
                       MainModel.GotoGitHub();
                   }));
            }
        }

        #endregion

        #region Unhandled Exceptions

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            File.WriteAllText("log.txt", e.Exception.ToString());
            System.Windows.MessageBox.Show(e.Exception.ToString(), DialogConsts.Error);
            //DialogHelper.ShowMessageDialog(_dialogService, DialogConsts.Error, e.Exception.ToString());
        }

        #endregion
    }
}