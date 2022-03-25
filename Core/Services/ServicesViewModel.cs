using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using WinCry.Dialogs;
using WinCry.Dialogs.ViewModels;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Services
{
    class ServicesViewModel : BaseViewModel
    {
        #region Private Members

        private ObservableCollection<Service> _allServices;

        private readonly IDialogService _dialogService;

        #endregion

        #region Public Properties

        private ObservableCollection<Service> _services;
        public ObservableCollection<Service> Services
        {
            get { return _services; }
            set
            {
                _services = value;
                OnPropertyChanged();
            }
        }

        private ServicesOption _option;
        public ServicesOption Option
        {
            get { return _option; }
            set
            {
                _option = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<ServicesPreset> _presets;
        public ObservableCollection<ServicesPreset> Presets
        {
            get { return _presets; }
            set
            {
                _presets = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MenuItem> _selectionMenuItems;
        public ObservableCollection<MenuItem> SelectionMenuItems
        {
            get { return _selectionMenuItems; }
            set
            {
                _selectionMenuItems = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<MenuItem> _deselectionMenuItems;
        public ObservableCollection<MenuItem> DeselectionMenuItems
        {
            get { return _deselectionMenuItems; }
            set
            {
                _deselectionMenuItems = value;
                OnPropertyChanged();
            }
        }

        private string _textToSearch;
        public string TextToSearch
        {
            get { return _textToSearch; }
            set
            {
                _textToSearch = value;
                OnPropertyChanged();

                Services = CollectionSearch.GetAllServicesThatContains(_allServices, value);
            }
        }

        #endregion

        #region Constructor

        public ServicesViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Services = new ObservableCollection<Service>();
            LoadPresetsList();
            UpdateMenus();
        }

        #endregion

        #region Commands

        private RelayCommand _apply;
        public RelayCommand Apply
        {
            get
            {
                return _apply ??
                   (_apply = new RelayCommand(obj =>
                   {
                       if (Option == 0)
                           return;

                       if (Option == ServicesOption.Delete)
                       {
                           ServicesModel.CheckServicesForRemovability(Services, _dialogService);
                       }

                       if (Services.Where(s => s.IsChecked).Count() == 0)
                           return;

                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogApplyServicesCaption, DialogConsts.BaseDialogApplyServicesMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel() { DialogCaption = DialogConsts.ApplyingCaption };
                       TaskViewModel _servicesTask = new TaskViewModel();

                       _vm.AddTask(ServicesModel.ApplyTask(Services, Option, _servicesTask), _servicesTask);
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);

                       if (DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogRebootCaption, DialogConsts.BaseDialogRebootMessage))
                       {
                           Helpers.RunByCMD("shutdown /r /t 0");
                       }
                   }));
            }
        }

        private RelayCommand _update;
        public RelayCommand Update
        {
            get
            {
                return _update ??
                   (_update = new RelayCommand(obj =>
                   {
                       ServicesModel.Update(Services);
                   }));
            }
        }

        private RelayCommand _start;
        public RelayCommand Start
        {
            get
            {
                return _start ??
                   (_start = new RelayCommand(obj =>
                   {
                       ServicesModel.ApplyTask(Services, ServicesOption.Start).Start();
                   }));
            }
        }

        private RelayCommand _stop;
        public RelayCommand Stop
        {
            get
            {
                return _stop ??
                   (_stop = new RelayCommand(obj =>
                   {
                       ServicesModel.ApplyTask(Services, ServicesOption.Stop).Start();
                   }));
            }
        }

        private RelayCommand _savePreset;
        public RelayCommand SavePreset
        {
            get
            {
                return _savePreset ??
                   (_savePreset = new RelayCommand(obj =>
                   {
                       string _fileName = DialogHelper.ShowSavePresetDialog(_dialogService, DialogConsts.MessageDialogSavingServicesPresetCaption);

                       if (_fileName != null)
                       {
                           ServicesPreset preset = new ServicesPreset();
                           preset.Name = _fileName;
                           preset.Option = Option;
                           preset.Services = Services;
                           preset.Save();

                           Presets.Add(preset);
                       }
                       LoadPresetsList();
                   }));
            }
        }

        private RelayCommand _invertSelectionToAll;
        public RelayCommand InvertSelectionToAll
        {
            get
            {
                return _invertSelectionToAll ??
                   (_invertSelectionToAll = new RelayCommand(async obj =>
                   {
                       await Task.Run(() =>
                       {
                           ServicesModel.InvertIsSelectedProperty(Services);
                       });
                   }));
            }
        }

        private RelayCommand _invertSelectionSingle;
        public RelayCommand InvertSelectionSingle
        {
            get
            {
                return _invertSelectionSingle ??
                   (_invertSelectionSingle = new RelayCommand(obj =>
                   {
                       ServicesModel.InvertIsSelectedProperty(obj, false);
                   }));
            }
        }

        private RelayCommand _invertSelectionMultiple;
        public RelayCommand InvertSelectionMultiple
        {
            get
            {
                return _invertSelectionMultiple ??
                   (_invertSelectionMultiple = new RelayCommand(obj =>
                   {
                       ServicesModel.InvertIsSelectedProperty(obj, true);
                   }));
            }
        }

        private RelayCommand _menuSelectionAll;
        public RelayCommand MenuSelectionAll
        {
            get
            {
                return _menuSelectionAll ??
                   (_menuSelectionAll = new RelayCommand(obj =>
                   {
                       ServicesModel.ChangeIsSelectedPropertyToAll(Services, obj);
                   }));
            }
        }

        private RelayCommand _menuSelectionCategory;
        public RelayCommand MenuSelectionCategory
        {
            get
            {
                return _menuSelectionCategory ??
                   (_menuSelectionCategory = new RelayCommand(obj =>
                   {
                       ServicesModel.ChangeIsSelectedPropertyToCategory(Services, obj);
                   }));
            }
        }

        #endregion

        #region Functions

        public void LoadPresetsList()
        {
            Presets = PresetController.LoadServicesPresetsList();
        }

        public void LoadPreset(ServicesPreset preset)
        {
            try
            {
                if (preset != null)
                {
                    Services = ServicesModel.SeekForRelevant(preset.Services);
                    _allServices = Services;
                    Option = preset.Option;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                DialogHelper.ShowMessageDialog(_dialogService, DialogConsts.MessageDialogErrorLoadingServicesPresetCaption, DialogConsts.MessageDialogErrorLoadingServicesPresetMessage + ex.Message);
            }
            finally
            {
                ServicesModel.Update(Services);
                UpdateMenus();
            }
        }

        public void UpdateMenus()
        {
            ObservableCollection<string> _tweaks = ServicesModel.GetAllServicesTypes(Services);

            SelectionMenuItems = MenuItems.BuildFromCollection(_tweaks, MenuSelectionCategory, true);
            SelectionMenuItems.Insert(0, MenuItems.Build(StringConsts.All, MenuSelectionAll, true));
            DeselectionMenuItems = MenuItems.BuildFromCollection(_tweaks, MenuSelectionCategory, false);
            DeselectionMenuItems.Insert(0, MenuItems.Build(StringConsts.All, MenuSelectionAll, false));
        }

        #endregion
    }
}