using System.Windows;
using WinCry.Dialogs;
using WinCry.Dialogs.ViewModels;
using WinCry.Dialogs.Views;
using WinCry.ViewModels;

namespace WinCry.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            IDialogService _dialogService = new DialogService(this);

            _dialogService.Register<DialogWindowViewModel, DialogWindow>();
            _dialogService.Register<DownloadWindowViewModel, DownloadWindow>();
            _dialogService.Register<SavePresetWindowViewModel, SavePresetWindow>();
            _dialogService.Register<ProgressWindowViewModel, ProgressWindow>();
            _dialogService.Register<MessageWindowViewModel, MessageWindow>();
            _dialogService.Register<DisclaimerWindowViewModel, DisclaimerWindow>();
            _dialogService.Register<ExpertModeDisclaimerWindowViewModel, ExpertModeDisclaimerWindow>();
            _dialogService.Register<MethodsWindowViewModel, MethodsWindow>();

            DataContext = new MainWindowViewModel(_dialogService);

            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Models.WindowBlur.ApplyBlurBehind(this);
        }
    }
}
