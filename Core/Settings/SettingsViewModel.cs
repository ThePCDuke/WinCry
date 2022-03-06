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
                if (value == null) return;

                ObservableCollection<string> _detectedGPUs = Helpers.GetInstalledGPUManufacturers();

                if (_detectedGPUs.Count == 0)
                {
                    value.Settings.DoPrimaryGPUTweak = false;
                }

                if (_detectedGPUs.Count != 2)
                {
                    value.Settings.DoSecondaryGPUTweak = false;
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
                _selectedServicesPreset = value;
                OnPropertyChanged();

                ServicesVM.LoadPreset(value);
            }
        }

        public TweaksPreset _selectedTweaksPreset;
        public TweaksPreset SelectedTweaksPreset
        {
            get { return _selectedTweaksPreset; }
            set
            {
                _selectedTweaksPreset = value;
                OnPropertyChanged();

                TweaksVM.LoadPreset(value);
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

            if (MemoryModel.IsServiceInstalled)
            {
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

                           Presets.Add(preset);
                           TweaksVM.Presets.Add(preset.TweaksPreset);
                           ServicesVM.Presets.Add(preset.ServicesPreset);
                       }
                       LoadPresetsList();
                   }));
            }
        }

        #endregion

        #region Functions

        public void LoadPresetsList()
        {
            Presets = PresetController.LoadSettingsPresetsList();

            SettingsPreset preset = Presets[0];

            SelectedSettingsPreset = preset;
        }

        #endregion
    }
}