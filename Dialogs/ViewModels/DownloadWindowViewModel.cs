using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Dialogs.ViewModels
{
    class DownloadWindowViewModel : BaseViewModel, IDialogRequestClose
    {
        #region Constructor

        public DownloadWindowViewModel(string downloadURL, string fileName, string path)
        {
            if (path == null)
                path = Path.GetTempPath();

            string _fullPath = Path.Combine(path, fileName);

            DownloadFile(downloadURL, _fullPath, true);
        }

        #endregion

        #region Private Members

        private WebClient _webClient;
        private readonly Stopwatch _sw = new Stopwatch();

        #endregion

        #region Public Properties

        private int _downloadPercent;
        public int DownloadPercent
        {
            get { return _downloadPercent; }
            set
            {
                _downloadPercent = value;
                OnPropertyChanged();
            }
        }

        private string _downloadSpeed;
        public string DownloadSpeed
        {
            get { return _downloadSpeed; }
            set
            {
                _downloadSpeed = value;
                OnPropertyChanged();
            }
        }

        private string _downloadedInfo;
        public string DownloadedInfo
        {
            get { return _downloadedInfo; }
            set
            {
                _downloadedInfo = value;
                OnPropertyChanged();
            }
        }

        private string _dialogCaption;
        public string DialogCaption
        {
            get { return _dialogCaption; }
            set
            {
                _dialogCaption = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Functions

        public async void DownloadFile(string urlAddress, string fullPath, bool skipCertificateValidation)
        {
            if (skipCertificateValidation)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            }

            using (_webClient = new WebClient())
            {
                _webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(Completed);
                _webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(ProgressChanged);

                Uri _URL = new Uri(urlAddress);

                _sw.Start();

                try
                {
                    await _webClient.DownloadFileTaskAsync(_URL, fullPath);
                }
                catch
                {

                }
            }
        }

        public async Task Wait(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        #endregion

        #region Commands

        private RelayCommand _cancel;
        public RelayCommand Cancel
        {
            get
            {
                return _cancel ??
                   (_cancel = new RelayCommand(obj =>
                   {
                       _webClient.CancelAsync();
                       CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false));
                   }));
            }
        }

        #endregion

        #region Events

        private void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadSpeed = string.Format("{0} Mb/s", (e.BytesReceived / 1024d / 1024d / _sw.Elapsed.TotalSeconds).ToString("0.00"));
            DownloadPercent = e.ProgressPercentage;
            DownloadedInfo = string.Format("{0} Mb's / {1} Mb's", (e.BytesReceived / 1024d / 1024d).ToString("0.00"), (e.TotalBytesToReceive / 1024d / 1024d).ToString("0.00"));
        }

        private async void Completed(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                await Wait(1000);
                CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false));
            }

            if (e.Cancelled == true)
            {
                CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(false));
            }
            else
            {
                CloseRequested?.Invoke(this, new DialogCloseRequestedEventArgs(true));
            }
        }

        public event EventHandler<DialogCloseRequestedEventArgs> CloseRequested;

        #endregion
    }
}