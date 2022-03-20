using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
using WinCry.Models;

namespace WinCry.Services
{
    public class Service : ISelectable, INotifyPropertyChanged
    {
        public Service() { }

        public Service(Service service)
        {
            FullName = service.FullName;
            ShortName = service.ShortName;
            Description = service.Description;
            Category = service.Category;
            Status = service.Status;
            IsForcedToApply = service.IsForcedToApply;
            Start = service.Start;
            RequiredWinBuild = service.RequiredWinBuild;
            IsChecked = service.IsChecked;
            CanDisable = service.CanDisable;
            CanRemove = service.CanRemove;
            CanRecover = service.CanRecover;
            RequiredGPU = service.RequiredGPU;
            RemovingMethod = service.RemovingMethod;
        }

        public enum ServiceRemovingMethod
        {
            CMD,
            Regedit
        }

        public const string StatusRunning = "Запущена";
        public const string StatusPaused = "Пауза";
        public const string StatusPending = "Ожидание";
        public const string StatusStopped = "Остановлена";
        public const string StatusDeleted = "Удалена";

        public const string TypeMain = "Основная";
        public const string TypeOptional = "Опциональная";

        public const string StartManual = "Вручную";
        public const string StartAutomatic = "Авто";
        public const string StartDisabled = "Отключена";
        public const string StartBoot = "Boot";
        public const string StartSystem = "System";

        public string FullName { get; set; }

        private string _shortName;
        public string ShortName
        {
            get { return _shortName; }
            set
            {
                _shortName = value;
                OnPropertyChanged();
            }
        }

        public string Description { get; set; }

        private string _category;
        public string Category
        {
            get { return _category; }
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }

        private string _status;
        [JsonIgnore]
        public string Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
            }
        }

        [JsonIgnore]
        public bool IsForcedToApply { get; set; }

        private string _start;
        public string Start
        {
            get { return _start; }
            set
            {
                _start = value;
                OnPropertyChanged();
            }
        }

        public string RequiredWinBuild { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public bool CanDisable { get; set; }

        public bool CanRemove { get; set; }

        public bool CanRecover { get; set; }

        public string RequiredGPU { get; set; }

        public ServiceRemovingMethod RemovingMethod { get; set; }

        #region OnProperty Changed
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
        #endregion
    }
}