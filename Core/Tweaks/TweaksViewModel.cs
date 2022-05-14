using System.Collections.ObjectModel;
using WinCry.Dialogs;
using WinCry.Dialogs.ViewModels;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Tweaks
{
    class TweaksViewModel : BaseViewModel
    {
        #region Private Fields

        private readonly IDialogService _dialogService;

        #endregion

        #region Public Properties

        private ObservableCollection<Tweak> _tweaks;
        public ObservableCollection<Tweak> Tweaks
        {
            get { return _tweaks; }
            set
            {
                _tweaks = value;
                OnPropertyChanged();
            }
        }

        private TreeBranchViewModel<string> _tweaksVM;
        public TreeBranchViewModel<string> TweaksVM
        {
            get { return _tweaksVM; }
            set
            {
                _tweaksVM = value;
                OnPropertyChanged();
            }
        }

        private TweaksOption _option;
        public TweaksOption Option
        {
            get { return _option; }
            set
            {
                _option = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<TweaksPreset> _presets;
        public ObservableCollection<TweaksPreset> Presets
        {
            get { return _presets; }
            set
            {
                _presets = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Constructor

        public TweaksViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            Tweaks = new ObservableCollection<Tweak>();

            LoadPresetsList();
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

                       if (Tweaks.Count == 0)
                           return;

                       if (!DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogApplyTweaksCaption, DialogConsts.BaseDialogApplyTweaksMessage))
                           return;

                       ProgressWindowViewModel _vm = new ProgressWindowViewModel() { DialogCaption = DialogConsts.ApplyingCaption };
                       TaskViewModel _tweaksTask = new TaskViewModel();

                       _vm.AddTask(TweaksModel.Apply(Tweaks, Option, _tweaksTask), _tweaksTask);
                       _vm.StartTasks();

                       _dialogService.ShowDialog(_vm);

                       TweaksModel.Update(Tweaks);
                       TweaksVM = TweaksModel.BuildTree(Tweaks);

                       if (DialogHelper.ShowDialog(_dialogService, DialogConsts.BaseDialogRebootCaption, DialogConsts.BaseDialogRebootMessage))
                       {
                           Helpers.RunByCMD("shutdown /r /t 0");
                       }
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
                       string _fileName = DialogHelper.ShowSavePresetDialog(_dialogService, DialogConsts.MessageDialogSavingTweaksPresetCaption);

                       if (_fileName != null)
                       {
                           TweaksPreset preset = new TweaksPreset();
                           preset.Name = _fileName;
                           preset.Option = Option;
                           preset.Tweaks = Tweaks;
                           preset.Save();

                           Presets.Add(preset);
                       }
                       LoadPresetsList();
                   }));
            }
        }

        #endregion

        #region Functions

        public void LoadPresetsList()
        {
            Presets = PresetController.LoadTweaksPresetsList();
        }

        public void LoadPreset(TweaksPreset preset)
        {
            try
            {
                if (preset != null)
                {
                    Tweaks = TweaksModel.SeekForRelevant(preset.Tweaks);
                    Option = preset.Option;
                }
            }
            catch (System.InvalidOperationException ex)
            {
                DialogHelper.ShowMessageDialog(_dialogService, DialogConsts.MessageDialogErrorLoadingTweaksPresetCaption, DialogConsts.MessageDialogErrorLoadingTweaksPresetMessage + ex.Message);
            }
            finally
            {
                TweaksModel.Update(Tweaks);
                TweaksVM = TweaksModel.BuildTree(Tweaks);
            }
        }

        #endregion
    }
}