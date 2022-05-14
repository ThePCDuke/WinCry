using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Services
{
    public class Service : BaseViewModel, ISelectable
    {
        public enum ServiceRemovingMethod
        {
            CMD,
            Regedit
        }

        public enum ServiceRemovingCondition
        {
            No,
            WithCaution,
            Yes
        }

        public Service() { }

        public Service(Service service)
        {
            FullName = service.FullName;
            ShortName = service.ShortName;
            Description = service.Description;
            Category = service.Category;
            Status = service.Status;
            Start = service.Start;
            IsChecked = service.IsChecked;
            CanDisable = service.CanDisable;
            CanEnable = service.CanEnable;
            CanBackup = service.CanBackup;
            CanRemove = service.CanRemove;
            CanRestore = service.CanRestore;
            Requirements = service.Requirements;
            RequiredGPU = service.RequiredGPU;
            RemovingMethod = service.RemovingMethod;
        }

        public const string StatusNotDetected = "Не обнаружена";
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

        private string _start;
        [JsonIgnore]
        public string Start
        {
            get { return _start; }
            set
            {
                _start = value;
                OnPropertyChanged();
            }
        }

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

        [JsonIgnore]
        public bool IsVisible { get; set; }

        [JsonIgnore]
        public bool CanDisable { get; set; }

        [JsonIgnore]
        public bool CanEnable { get; set; }

        [JsonIgnore]
        public bool CanRestore { get; set; }

        [JsonIgnore]
        public bool CanBackup { get; set; }

        [JsonIgnore]
        public ServiceRemovingCondition CanRemove { get; set; }

        public string RequiredGPU { get; set; }

        public ServiceRemovingMethod RemovingMethod { get; set; }

        public ServiceReqs Requirements { get; set; }

        public int DefaultStart { get; set; }
    }
}