using WinCry.Dialogs;
using WinCry.Dialogs.ViewModels;

namespace WinCry.Models
{
    class DialogHelper
    {
        public static bool ShowDialog(IDialogService dialogService, string dialogCaption, string dialogText)
        {
            DialogWindowViewModel _vm = new DialogWindowViewModel()
            {
                DialogCaption = dialogCaption,
                DialogText = dialogText
            };

            return (bool)dialogService.ShowDialog(_vm);
        }

        public static bool ShowDisclaimer(IDialogService dialogService)
        {
            DisclaimerWindowViewModel _vm = new DisclaimerWindowViewModel();

            dialogService.ShowDialog(_vm);

            return _vm.DoNotShow;
        }

        public static void ShowMessageDialog(IDialogService dialogService, string dialogCaption, string dialogText)
        {
            MessageWindowViewModel _vm = new MessageWindowViewModel()
            {
                DialogCaption = dialogCaption,
                DialogText = dialogText
            };

            dialogService.ShowDialog(_vm);
        }

        public static string ShowSavePresetDialog(IDialogService dialogService, string caption)
        {
            SavePresetWindowViewModel _vm = new SavePresetWindowViewModel() { Caption = caption };

            if (dialogService.ShowDialog(_vm) == true)
            {
                return _vm.FileName;
            }
            else
            {
                return null;
            }
        }

        public static bool ShowDownloadDialog(IDialogService dialogService, string url, string name, string path = null)
        {
            DownloadWindowViewModel _vm = new DownloadWindowViewModel(url, name, path)
            {
                DialogCaption = DialogConsts.MessageDialogDownloadCaption
            };

            return (bool)dialogService.ShowDialog(_vm);
        }
    }
}