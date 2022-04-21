using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using WinCry.Dialogs;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Services
{
    class ServicesModel : DataGridBasedModel
    {
        private static readonly bool _isAMD = Helpers.IsAMDGPUInstalled;
        private static readonly bool _isNVIDIA = Helpers.IsNVIDIAGPUInstalled;

        /// <summary>
        /// Disables service via CMD as TI
        /// </summary>
        /// <param name="service">Service to disable</param>
        private static string Disable(Service service)
        {
            return RunAsProcess.CMD($"sc config \"{service.ShortName}\" start= disabled", true);
        }

        /// <summary>
        /// Enables service via CMD as TI
        /// </summary>
        /// <param name="service">Service to enable</param>
        private static string Enable(Service service)
        {
            return RunAsProcess.CMD($"sc config \"{service.ShortName}\" start= auto", true);
        }

        /// <summary>
        /// Removes service completely via CMD or Regedit as TI
        /// </summary>
        /// <param name="service">Service to remove</param>
        private static string Delete(Service service)
        {
            if (service.RemovingMethod == Service.ServiceRemovingMethod.CMD)
                return RunAsProcess.CMD($"sc delete \"{service.ShortName}\"", true);
            else if (service.RemovingMethod == Service.ServiceRemovingMethod.Regedit)
            {
                string _fullPath = $@"HKEY_LOCAL_MACHINE\System\CurrentControlSet\Services\{service.ShortName}";
                return RunAsProcess.CMD($"reg delete \"{_fullPath}\" /f", true);
            }
            return null;
        }

        /// <summary>
        /// Restores service from user's backup or embedded backup
        /// </summary>
        /// <param name="service">Service to restore</param>
        public static string Restore(Service service)
        {
            string _zipExtractionDirectory = StringConsts.ServicesRestorationPatchFolder;
            string _userCreatedDirectory = StringConsts.ServicesBackupFolder;

            if (File.Exists($"{_userCreatedDirectory}\\{service.ShortName}.reg"))
            {
                return RunAsProcess.CMD($@"reg import ""{_userCreatedDirectory}\{service.ShortName}.reg""", true);
            }
            else
            {
                if (!Directory.Exists(_zipExtractionDirectory))
                    ExtractRestorationFiles();

                if (File.Exists($"{_zipExtractionDirectory}\\{service.ShortName}.reg"))
                    return RunAsProcess.CMD($@"reg import ""{_zipExtractionDirectory}\\{service.ShortName}.reg""", true);
            }
            return null;
        }

        /// <summary>
        /// Backs up service from Regedit via CMD as TI
        /// </summary>
        /// <param name="service">Service to backup</param>
        private static string Backup(Service service)
        {
            if (!Directory.Exists(StringConsts.ServicesBackupFolder))
            {
                Directory.CreateDirectory(StringConsts.ServicesBackupFolder);
            }

            return RunAsProcess.CMD($@"regedit /e ""{Path.Combine(StringConsts.ServicesBackupFolder, service.ShortName + ".reg")}"" ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{service.ShortName}""", true);
        }

        /// <summary>
        /// Start service via CMD as TI
        /// </summary>
        /// <param name="service">Service to start</param>
        private static string Start(Service service)
        {
            return RunAsProcess.CMD($"net start \"{service.ShortName}\"", true, false);
        }

        /// <summary>
        /// Stops service via CMD as TI
        /// </summary>
        /// <param name="service">Service to stop</param>
        private static string Stop(Service service)
        {
            return RunAsProcess.CMD($"net stop \"{service.ShortName}\"", true, false);
        }

        /// <summary>
        /// Updates service's status and start type
        /// </summary>
        /// <param name="service">Service to update</param>
        private static string Update(Service service)
        {
            if (!IsExists(service))
            {
                service.Status = Service.StatusDeleted;
                service.Start = Service.StatusDeleted;
                return Service.StatusDeleted;
            }

            using (ServiceController _controller = new ServiceController(service.ShortName))
            {
                switch (_controller.Status)
                {
                    case ServiceControllerStatus.ContinuePending:
                        service.Status = Service.StatusPending;
                        break;
                    case ServiceControllerStatus.Paused:
                        service.Status = Service.StatusPaused;
                        break;
                    case ServiceControllerStatus.PausePending:
                        service.Status = Service.StatusPending;
                        break;
                    case ServiceControllerStatus.Running:
                        service.Status = Service.StatusRunning;
                        break;
                    case ServiceControllerStatus.StartPending:
                        service.Status = Service.StatusPending;
                        break;
                    case ServiceControllerStatus.Stopped:
                        service.Status = Service.StatusStopped;
                        break;
                    case ServiceControllerStatus.StopPending:
                        service.Status = Service.StatusPending;
                        break;
                }

                switch (_controller.StartType)
                {
                    case ServiceStartMode.Manual:
                        service.Start = Service.StartManual;
                        break;
                    case ServiceStartMode.Automatic:
                        service.Start = Service.StartAutomatic;
                        break;
                    case ServiceStartMode.Disabled:
                        service.Start = Service.StartDisabled;
                        break;
                    case ServiceStartMode.Boot:
                        break;
                    case ServiceStartMode.System:
                        break;
                }
            }

            return null;
        }

        /// <summary>
        /// Updates services` status and start type
        /// </summary>
        /// <param name="servicesCollection">Services collection to update</param>
        public static void Update(ObservableCollection<Service> servicesCollection)
        {
            foreach (Service _service in servicesCollection)
            {
                Update(_service);
            }
        }

        /// <summary>
        /// Applies <paramref name="option"/> to service
        /// </summary>
        /// <param name="service">Service to apply</param>
        /// <param name="option">Option to apply</param>
        private static string Apply(Service service, ServicesOption option)
        {
            switch (option)
            {
                case ServicesOption.Disable:
                    {
                        if (service.CanDisable || service.IsForcedToApply)
                        {
                            return $"{Disable(service)} {Stop(service)}";
                        }
                        else
                        {
                            return null;
                        }
                    }
                case ServicesOption.Enable:
                    {
                        return $"{Enable(service)} {Start(service)}";
                    }
                case ServicesOption.Delete:
                    {
                        if (service.CanRemove || service.IsForcedToApply)
                        {
                            return Delete(service);
                        }
                        else
                        {
                            return $"{Disable(service)} {Stop(service)}";
                        }
                    }
                case ServicesOption.Restore:
                    {
                        if (service.CanRecover || service.IsForcedToApply)
                        {
                            return Restore(service);
                        }
                        else
                        {
                            return null;
                        }
                    }
                case ServicesOption.Backup:
                    {
                        return Backup(service);
                    }
                case ServicesOption.Start:
                    {
                        return Start(service);
                    }
                case ServicesOption.Stop:
                    {
                        return Stop(service);
                    }
                case ServicesOption.Update:
                    {
                        return Update(service);
                    }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Applies services via CMD as TI
        /// </summary>
        /// <param name="servicesCollection">Services to apply</param>
        public static Task ApplyTask(ObservableCollection<Service> servicesCollection, ServicesOption option, TaskViewModel taskViewModel = null)
        {
            if (taskViewModel == null)
                taskViewModel = new TaskViewModel();

            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.Services;
                taskViewModel.Progress = 0;
                taskViewModel.IsSuccessfull = null;
                taskViewModel.IsCompleted = false;

                double _totalServices = servicesCollection.Where(t => t.IsChecked).Count();
                int _current = 0;

                taskViewModel.ShortMessage = DialogConsts.ApplyingCaption;
                taskViewModel.CreateMessage($"{DialogConsts.ApplyingStarted}", false);

                foreach (Service _service in servicesCollection.Where(s => s.IsChecked))
                {
                    try
                    {
                        taskViewModel.CreateMessage($"\"{_service.FullName}\": ");
                        taskViewModel.CreateMessage(Apply(_service, option), false, false);

                        _current += 1;

                        double _currentPercent = _current / _totalServices * 100;
                        taskViewModel.Progress = (int)_currentPercent;
                        taskViewModel.ShortMessage = $@"({_current}/{_totalServices}) {_service.FullName}";
                    }
                    catch (Exception ex)
                    {
                        //taskViewModel.CatchException(ex);
                        taskViewModel.CreateMessage(ex.Message, false, false);
                        continue;
                    }
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        /// <summary>
        /// Extracts embedded services' backup files to restore
        /// </summary>
        public static void ExtractRestorationFiles()
        {
            Helpers.UnzipFromByteArray(Properties.Resources.ServicesRestore, StringConsts.ServicesRestorationPatchFolder);
        }

        /// <summary>
        /// Check if service is exists in OS
        /// </summary>
        /// <param name="service">Service to check</param>
        /// <returns></returns>
        private static bool IsExists(Service service)
        {
            string _fullPath = $@"System\CurrentControlSet\Services\{service.ShortName}";
            RegistryKey _key = Registry.LocalMachine.OpenSubKey($"{_fullPath}");

            if (_key == null)
                return false;

            ServiceController[] _services = ServiceController.GetServices();

            var _service = _services.FirstOrDefault(s => s.ServiceName.ToLower() == service.ShortName.ToLower());
            return _service != null;
        }

        /// <summary>
        /// Checks if service's options is relevant to current system
        /// </summary>
        /// <param name="service">Service to check</param>
        /// <returns></returns>
        private static bool IsRelevant(Service service)
        {
            int _windowsBuild = Helpers.GetWinBuild();

            if (service.RequiredGPU == "AMD")
            {
                if (!_isAMD)
                    return false;
            }

            if (service.RequiredGPU == "NVIDIA")
            {
                if (!_isNVIDIA)
                    return false;
            }

            switch (service.RequiredWinBuild[service.RequiredWinBuild.Length - 1])
            {
                case '+':
                    {
                        if (!int.TryParse(service.RequiredWinBuild.Substring(0, service.RequiredWinBuild.Length - 1), out int _result))
                            return false;
                        if (_windowsBuild >= _result)
                            return true;
                        else
                            return false;
                    }
                case '-':
                    {
                        if (!int.TryParse(service.RequiredWinBuild.Substring(0, service.RequiredWinBuild.Length - 1), out int _result))
                            return false;
                        if (_windowsBuild <= _result)
                            return true;
                        else
                            return false;
                    }
                default:
                    {
                        if (!int.TryParse(service.RequiredWinBuild, out int _result))
                            return false;
                        if (_result == _windowsBuild)
                            return true;
                        else
                            return false;
                    }
            }
        }

        /// <summary>
        /// Loads service collection from the user created preset if exists, else loads default
        /// </summary>
        /// <param name="presetName">Name of the preset</param>
        /// <returns></returns>
        public static ObservableCollection<Service> SeekForRelevant(ObservableCollection<Service> servicesCollection)
        {
            ObservableCollection<Service> output = new ObservableCollection<Service>();

            foreach (Service _service in servicesCollection)
            {
                if (IsRelevant(_service))
                    output.Add(_service);
            }
            return output;
        }

        /// <summary>
        /// Gets all services' types from collection
        /// </summary>
        /// <param name="servicesCollection">Collection to get types from</param>
        /// <returns></returns>
        public static ObservableCollection<string> GetAllServicesTypes(ObservableCollection<Service> servicesCollection)
        {
            ObservableCollection<string> _types = new ObservableCollection<string>();

            foreach (Service _tweak in servicesCollection)
            {
                if (!_types.Contains(_tweak.Category))
                    _types.Add(_tweak.Category);
            }

            return _types;
        }

        /// <summary>
        /// Updates given <paramref name="service"/> to achive certain <paramref name="status"/>
        /// </summary>
        /// <param name="service">Service to update</param>
        /// <param name="status">Status to achive</param>
        /// <param name="interval">Interval between each try</param>
        /// <param name="retryCounts">Retry counts</param>
        /// <returns></returns>
        private static async Task UpdateServiceTillStatusEquals(Service service, string status, int interval, int retryCounts)
        {
            int _tryingCounts = 0;
            do
            {
                System.Diagnostics.Debug.Print($"Обновляю {service.ShortName}, {service.Status}");
                Update(service);

                if (service.Status == Service.StatusDeleted)
                    break;

                await Task.Delay(interval);
                System.Diagnostics.Debug.Print($"Статус {service.ShortName} - {service.Status}");
                _tryingCounts += 1;
            }
            while ((service.Status != status) && (_tryingCounts != retryCounts));
        }

        /// <summary>
        /// Gather corrupted (removed, disabled, stopped) services that MS Store depends on.
        /// Tries to enable them and update.
        /// </summary>
        /// <param name="interval">Interval between each try</param>
        /// <param name="retryCounts">Retry counts</param>
        /// <returns></returns>
        public static async Task UpdateMSStoreDependingServicesAsync(int interval = 1000, int retryCounts = 10)
        {
            ObservableCollection<Task> _tasks = new ObservableCollection<Task>();
            Helpers.DeserializeCollectionAsXML(Properties.Resources.MSStoreDependingServices,
                                               out ObservableCollection<Service> _dependingServices);

            foreach (Service _service in _dependingServices)
            {
                Update(_service);

                if (_service.Start == Service.StartDisabled)
                    Enable(_service);

                if (_service.Status != Service.StatusRunning)
                {
                    _tasks.Add(UpdateServiceTillStatusEquals(_service, Service.StatusRunning, interval, retryCounts));
                    Start(_service);
                }
            }

            Task[] _tasksArray = _tasks.ToArray();

            await Task.WhenAll(_tasksArray);
        }

        /// <summary>
        /// Gets collection of MS Store depending services that needs to restore
        /// </summary>
        /// <returns>Collection of services that aren't exist on current system and that MS Store depends on</returns>
        public static ObservableCollection<Service> MSStoreDependingServicesToRestore()
        {
            ObservableCollection<Service> _servicesToRestore = new ObservableCollection<Service>();
            Helpers.DeserializeCollectionAsXML(Properties.Resources.MSStoreDependingServices, out ObservableCollection<Service> _dependingServices);

            foreach (Service _service in _dependingServices)
            {
                if (!IsExists(_service))
                {
                    _servicesToRestore.Add(_service);
                    continue;
                }

                using (ServiceController _controller = new ServiceController(_service.ShortName))
                {
                    if (_controller.Status != ServiceControllerStatus.Running)
                        _servicesToRestore.Add(_service);
                }
            }

            return _servicesToRestore;
        }

        /// <summary>
        /// Restores TrustedInstaller service
        /// </summary>
        public static void RestoreTrustedInstallerService()
        {
            Helpers.ExtractEmbedFile(Properties.Resources.TrustedInstaller, "TrustedInstaller.reg");
            RunAsProcess.CMD($"reg import {Path.GetTempPath()}TrustedInstaller.reg", true, true, "lsass");
        }

        /// <summary>
        /// Checks collection for nonremovable services and asks if user insists to remove them
        /// </summary>
        /// <param name="servicesCollection">Service collection to search in</param>
        /// <param name="dialogService">Dialog service for prompt dialog</param>
        public static void CheckServicesForRemovability(ObservableCollection<Service> servicesCollection, IDialogService dialogService = null)
        {
            foreach (Service _service in servicesCollection)
            {
                if (_service.IsChecked)
                {
                    if (!_service.CanRemove)
                    {
                        bool _result = DialogHelper.ShowDialog(dialogService,
                            $@"{DialogConsts.DialogNonRemovableServiceCaption} ""{_service.FullName}""",
                            $@"{DialogConsts.DialogNonRemovableServiceMessage}{Environment.NewLine}{Environment.NewLine}Описание службы: {_service.Description}");

                        if (!_result)
                        {
                            _service.IsChecked = false;
                            _service.IsForcedToApply = false;
                        }
                        else
                        {
                            _service.IsForcedToApply = true;
                        }
                    }
                }
            }
        }
    }
}