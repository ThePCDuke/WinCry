using Microsoft.Win32;
using PСSLC.Core;
using System.Collections.ObjectModel;
using WinCry.Dialogs;
using WinCry.Memory;
using WinCry.Models;
using WinCry.Services;
using WinCry.Tweaks;
using WinCry.ViewModels;

namespace WinCry.Settings
{
    class SettingsViewModel : BaseViewModel
    {
        #region Private Members

        private readonly IDialogService _dialogService;

        #endregion

        #region Public Properties

        public Setting Settings
        {
            get { return SelectedSettingsPreset.Settings; }
            set
            {
                SelectedSettingsPreset.Settings = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<SettingsPreset> _presets;
        public ObservableCollection<SettingsPreset> Presets
        {
            get { return _presets; }
            set
            {
                _presets = value;
                OnPropertyChanged();
            }
        }

        public ServicesViewModel ServicesVM { get; set; }
        public TweaksViewModel TweaksVM { get; set; }
        public MemoryViewModel MemoryVM { get; set; }

        private SettingsPreset _currentSettingsPreset;
        public SettingsPreset CurrentSettingsPreset
        {
            get { return _currentSettingsPreset; }
            set
            {
                _currentSettingsPreset = value;
                OnPropertyChanged();
            }
        }

        private SettingsPreset _selectedSettingsPreset;
        public SettingsPreset SelectedSettingsPreset
        {
            get { return _selectedSettingsPreset; }
            set
            {
                if (value == null) 
                    return;

                if (value.Name == StringConsts.ExpertPreset)
                {
                    if (!DialogHelper.ShowExpertModeDisclaimer(_dialogService))
                    {
                        return;
                    }
                }

                ObservableCollection<string> _detectedGPUs = Helpers.GetInstalledGPUManufacturers();

                if (_detectedGPUs.Count == 0)
                {
                    value.Settings.DoPrimaryGPUTweak = false;
                }

                if (_detectedGPUs.Count != 2)
                {
                    value.Settings.DoSecondaryGPUTweak = false;
                }

                if (Helpers.GetWinBuild() >= 22000)
                {
                    value.Settings.RemoveNotificationIcon = false;
                }

                _selectedSettingsPreset = value;
                OnPropertyChanged();

                CurrentSettingsPreset = new SettingsPreset();
                SelectedServicesPreset = value.ServicesPreset;
                SelectedTweaksPreset = value.TweaksPreset;
                MemoryVM.Data = value.MemoryData;
                CurrentSettingsPreset = SelectedSettingsPreset;
            }
        }

        private ServicesPreset _selectedServicesPreset;
        public ServicesPreset SelectedServicesPreset
        {
            get { return _selectedServicesPreset; }
            set
            {
                if (value == null)
                    return;

                if (SelectedSettingsPreset.Name != StringConsts.ExpertPreset && value.Name == StringConsts.ExpertPreset)
                {
                    if (!DialogHelper.ShowExpertModeDisclaimer(_dialogService))
                    {
                        return;
                    }
                }

                ServicesVM.LoadPreset(value);

                _selectedServicesPreset = value;
                OnPropertyChanged();
            }
        }

        public TweaksPreset _selectedTweaksPreset;
        public TweaksPreset SelectedTweaksPreset
        {
            get { return _selectedTweaksPreset; }
            set
            {
                if (value == null)
                    return;

                if (SelectedSettingsPreset.Name != StringConsts.ExpertPreset && value.Name == StringConsts.ExpertPreset)
                {
                    if (!DialogHelper.ShowExpertModeDisclaimer(_dialogService))
                    {
                        return;
                    }
                }

                TweaksVM.LoadPreset(value);

                _selectedTweaksPreset = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public SettingsViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            TweaksVM = new TweaksViewModel(dialogService);
            ServicesVM = new ServicesViewModel(dialogService);
            MemoryVM = new MemoryViewModel(dialogService);

            LoadPresetsList();
            LoadDefaultPreset();

            if (MemoryModel.IsServiceInstalled)
            {
                RegistryKey localMachineKey = Registry.LocalMachine;
                RegistryKey softwareKey = localMachineKey.OpenSubKey(RegulationDataConsts.RegSoftwareKey, true);
                RegistryKey peaceDukeKey = softwareKey.OpenSubKey(RegulationDataConsts.RegISLCPeaceDukeKey, true);

                if (peaceDukeKey == null)
                {
                    RegulationsDataWritter writter = new RegulationsDataWritter();
                    writter.Write(RegulationsData.Default);
                }

                RegulationsData regulationsData = new RegulationsDataReader().Read();
                MemoryVM.Data = new MemoryData() { CachedRAMGreaterThan = regulationsData.StandbyMemory, FreeRAMLessThan = regulationsData.FreeMemory, ServiceThreadSleepSeconds = regulationsData.ServiceThreadSleepMilliseconds / 1000 };
            }
        }

        #endregion

        #region Commands

        private RelayCommand _savePreset;
        public RelayCommand SavePreset
        {
            get
            {
                return _savePreset ??
                   (_savePreset = new RelayCommand(obj =>
                   {
                       string fileName = DialogHelper.ShowSavePresetDialog(_dialogService, DialogConsts.MessageDialogSavingSettingsPresetCaption);

                       if (fileName != null)
                       {
                           string name = System.IO.Path.GetFileNameWithoutExtension(fileName);

                           CurrentSettingsPreset.TweaksPreset.Tweaks = TweaksVM.Tweaks;
                           CurrentSettingsPreset.TweaksPreset.Option = TweaksVM.Option;
                           CurrentSettingsPreset.ServicesPreset.Services = ServicesVM.Services;
                           CurrentSettingsPreset.ServicesPreset.Option = ServicesVM.Option;

                           SettingsPreset preset = new SettingsPreset();

                           preset.ServicesPreset = CurrentSettingsPreset.ServicesPreset;
                           preset.TweaksPreset = CurrentSettingsPreset.TweaksPreset;
                           preset.Settings = CurrentSettingsPreset.Settings;
                           preset.MemoryData = CurrentSettingsPreset.MemoryData;

                           preset.Name = name;
                           preset.TweaksPreset.Name = name;
                           preset.ServicesPreset.Name = name;

                           preset.Save();

                           //Presets.Add(preset);
                           //TweaksVM.Presets.Add(preset.TweaksPreset);
                           //ServicesVM.Presets.Add(preset.ServicesPreset);

                           LoadPresetsList();
                           SelectedSettingsPreset = new SettingsPreset(fileName);
                       }
                   }));
            }
        }

        #endregion

        #region Functions

        public void LoadPresetsList()
        {
            Presets = PresetController.LoadSettingsPresetsList();

            TweaksVM.LoadPresetsList();
            ServicesVM.LoadPresetsList();
        }

        public void LoadDefaultPreset()
        {
            SettingsPreset preset = Presets[0];

            SelectedSettingsPreset = preset;
        }

        #endregion
    }
}