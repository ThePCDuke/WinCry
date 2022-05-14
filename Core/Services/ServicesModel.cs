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
            string zipExtractionDirectory = StringConsts.ServicesRestorationPatchFolder;
            string userCreatedDirectory = StringConsts.ServicesBackupFolder;

            string servicesPath = $@"System\CurrentControlSet\Services";
            string fullPath = $@"System\CurrentControlSet\Services\{service.ShortName}";

            RegistryKey fullKey = Registry.LocalMachine.OpenSubKey(fullPath, true);
            
            if (IsExists(service) == false)
            {
                if (fullKey != null)
                {
                    RegistryKey servicesKey = Registry.LocalMachine.OpenSubKey(servicesPath, true);
                    servicesKey.DeleteSubKeyTree(service.ShortName);

                    servicesKey.Close();
                }

                if (File.Exists($"{userCreatedDirectory}\\{service.ShortName}.reg"))
                {
                    return $"{DialogConsts.RestoringServicesAppliedUser} {RunAsProcess.CMD($@"reg import ""{userCreatedDirectory}\{service.ShortName}.reg""", true)}";
                }
                else
                {
                    ExtractRestorationFiles();

                    if (File.Exists($"{zipExtractionDirectory}\\{service.ShortName}.reg"))
                        return $"{DialogConsts.RestoringServicesAppliedEmbeded} {RunAsProcess.CMD($@"reg import ""{zipExtractionDirectory}\\{service.ShortName}.reg""", true)}";
                }
            }
            else
            {
                fullKey.SetValue("Start", service.DefaultStart, RegistryValueKind.DWord);
                
                return DialogConsts.RestoringServicesAppliedDefaultStart;
            }

            if (fullKey != null) fullKey.Close();

            return DialogConsts.RestoringServicesError;
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

            RunAsProcess.CMD($@"regedit /e ""{Path.Combine(StringConsts.ServicesBackupFolder, service.ShortName + ".reg")}"" ""HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{service.ShortName}""", true);

            return DialogConsts.Done;
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
            bool? _isExists = IsExists(service);

            if (_isExists == false)
            {
                service.Status = Service.StatusDeleted;
                service.Start = Service.StatusDeleted;
                return Service.StatusDeleted;
            }
            else if (_isExists == null)
            {
                service.Status = service.Status = Service.StatusNotDetected;
                return Service.StatusNotDetected;
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
        public static string Apply(Service service, ServicesOption option)
        {
            switch (option)
            {
                case ServicesOption.Disable:
                    {
                        if (service.CanDisable)
                        {
                            string disable = Disable(service);
                            Stop(service);

                            return disable;
                        }
                        else
                        {
                            return null;
                        }
                    }
                case ServicesOption.Enable:
                    {
                        if (service.CanEnable)
                        {
                            string enable = Enable(service);
                            Start(service);

                            return enable;
                        }
                        else
                        {
                            return null;
                        }
                    }
                case ServicesOption.Delete:
                    {
                        if (service.CanRemove == Service.ServiceRemovingCondition.No)
                        {
                            string disable = Disable(service);
                            Stop(service);

                            return disable;
                        }
                        else
                        {
                            return Delete(service);
                        }
                    }
                case ServicesOption.Restore:
                    {
                        if (service.CanRestore)
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
                        if (service.CanBackup)
                        {
                            return Backup(service);
                        }
                        else
                        {
                            return null;
                        }
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
        private static bool? IsExists(Service service)
        {
            string fullPath = $@"System\CurrentControlSet\Services\{service.ShortName}";
            RegistryKey key = Registry.LocalMachine.OpenSubKey(fullPath);

            if (key == null)
                return false;

            ServiceController[] services = ServiceController.GetServices();

            var _service = services.FirstOrDefault(s => s.ServiceName.ToLower() == service.ShortName.ToLower());

            if (_service == null)
            {
                var start = key.GetValue("Start");
                var type = key.GetValue("Type");
                var error = key.GetValue("ErrorControl");

                if (start == null || type == null || error == null)
                    return false;

                byte startResult;
                Byte.TryParse(start.ToString(), out startResult);

                switch (startResult)
                {
                    case (byte)ServiceStartMode.Manual:
                        service.Start = Service.StartManual;
                        break;
                    case (byte)ServiceStartMode.Automatic:
                        service.Start = Service.StartAutomatic;
                        break;
                    case (byte)ServiceStartMode.Disabled:
                        service.Start = Service.StartDisabled;
                        break;
                    case (byte)ServiceStartMode.Boot:
                        break;
                    case (byte)ServiceStartMode.System:
                        break;
                }

                return null;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Checks if service's options is relevant to current system
        /// </summary>
        /// <param name="service">Service to check</param>
        /// <returns></returns>
        private static bool IsRelevant(Service service)
        {
            int _windowsBuild = Helpers.GetWinBuild();

            //service.Requirements = new ServiceReqs
            //{
            //    VisibleOn = new string[] { service.RequiredWinBuild },
            //    CanDisableOn = new string[] { service.RequiredWinBuild },
            //    CanEnableOn = new string[] { service.RequiredWinBuild },
            //    CanDeleteOn = new ServiceWinCondition[] { new ServiceWinCondition { Version = service.RequiredWinBuild, Condition = Service.ServiceRemovingCondition.Yes} },
            //    CanRestoreOn = new string[] { service.RequiredWinBuild },
            //    CanBackupOn = new string[] { service.RequiredWinBuild }
            //};
            //if (service.Category == "Опциональные")
            //{
            //    service.Requirements.CanDeleteOn = new ServiceWinCondition[] { new ServiceWinCondition { Version = service.Requirements.VisibleOn[0], Condition = Service.ServiceRemovingCondition.Yes } };
            //}

            //var array = File.ReadAllLines($@"Services\{service.ShortName}.reg");

            //foreach (string line in array)
            //{
            //    if (line.Contains(@"""Start"""))
            //    {
            //        string linex = line.Substring(line.Length - 1, 1);
            //        service.DefaultStart = int.Parse(linex);
            //    }
            //}

            //return true;


            CheckService(service, _windowsBuild);

            if (service.RequiredGPU != null)
            {
                switch (service.RequiredGPU)
                {
                    case "AMD":
                        {
                            if (!_isAMD)
                                return false;

                            break;
                        }
                    case "NVIDIA":
                        {
                            if (!_isNVIDIA)
                                return false;

                            break;
                        }
                    default:
                        break;
                }
            }

            return service.IsVisible;
        }

        public static void CheckService(Service service, int requiredVersion)
        {
            foreach (string version in service.Requirements.VisibleOn)
            {
                if (IsVersionRelevant(requiredVersion, version) == null)
                {
                    service.IsVisible = true;
                    break;
                }
                else if (IsVersionRelevant(requiredVersion, version) == true)
                {
                    service.IsVisible = true;
                }
                else
                {
                    return;
                }
            }

            foreach (string version in service.Requirements.CanBackupOn)
            {
                if (IsVersionRelevant(requiredVersion, version) == null)
                {
                    service.CanBackup = true;
                    break;
                }
                else if (IsVersionRelevant(requiredVersion, version) == true)
                {
                    service.CanBackup = true;
                }
            }

            foreach (ServiceWinCondition version in service.Requirements.CanDeleteOn)
            {
                if (IsVersionRelevant(requiredVersion, version.Version) == null)
                {
                    service.CanRemove = version.Condition;
                    break;
                }
                else if (IsVersionRelevant(requiredVersion, version.Version) == true)
                {
                    service.CanRemove = version.Condition;
                }
            }

            foreach (string version in service.Requirements.CanDisableOn)
            {
                if (IsVersionRelevant(requiredVersion, version) == null)
                {
                    service.CanDisable = true;
                    break;
                }
                else if (IsVersionRelevant(requiredVersion, version) == true)
                {
                    service.CanDisable = true;
                }
            }

            foreach (string version in service.Requirements.CanEnableOn)
            {
                if (IsVersionRelevant(requiredVersion, version) == null)
                {
                    service.CanEnable = true;
                    break;
                }
                else if (IsVersionRelevant(requiredVersion, version) == true)
                {
                    service.CanEnable = true;
                }
            }

            foreach (string version in service.Requirements.CanRestoreOn)
            {
                if (IsVersionRelevant(requiredVersion, version) == null)
                {
                    service.CanRestore = true;
                    break;
                }
                else if (IsVersionRelevant(requiredVersion, version) == true)
                {
                    service.CanRestore = true;
                }
            }
        }

        private static bool? IsVersionRelevant(int currentVersion, string requiredVersion)
        {
            if (requiredVersion == "0+")
                return true;

            switch (requiredVersion[requiredVersion.Length - 1])
            {
                case '+':
                    {
                        if (!int.TryParse(requiredVersion.Substring(0, requiredVersion.Length - 1), out int result))
                            return false;
                        if (currentVersion >= result)
                            return true;
                        else
                            return false;
                    }
                case '-':
                    {
                        if (!int.TryParse(requiredVersion.Substring(0, requiredVersion.Length - 1), out int result))
                            return false;
                        if (currentVersion <= result)
                            return true;
                        else
                            return false;
                    }
                default:
                    {
                        if (!int.TryParse(requiredVersion, out int result))
                            return false;
                        if (currentVersion == result)
                            return null;
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
                Update(service);

                if (service.Status == Service.StatusDeleted)
                    break;

                await Task.Delay(interval);
                _tryingCounts += 1;
            }
            while ((service.Status != status) && (_tryingCounts != retryCounts));
        }

        /// <summary>
        /// Gather corrupted (removed, disabled, stopped) services.
        /// Tries to enable them and update.
        /// </summary>
        /// <param name="interval">Interval between each try</param>
        /// <param name="retryCounts">Retry counts</param>
        /// <returns></returns>
        public static async Task UpdateDependingServicesAsync(byte[] resource, int interval = 1000, int retryCounts = 10)
        {
            ObservableCollection<Task> tasks = new ObservableCollection<Task>();
            Helpers.DeserializeCollectionAsXML(resource, out ObservableCollection<Service> dependingServices);

            foreach (Service service in dependingServices)
            {
                Update(service);

                if (service.Start == Service.StartDisabled)
                    Enable(service);

                if (service.Status != Service.StatusRunning)
                {
                    tasks.Add(UpdateServiceTillStatusEquals(service, Service.StatusRunning, interval, retryCounts));
                    Start(service);
                }
            }

            Task[] tasksArray = tasks.ToArray();

            await Task.WhenAll(tasksArray);
        }

        /// <summary>
        /// Gets collection of services that needs to restore
        /// </summary>
        /// <returns>Collection of services that aren't exist on current system</returns>
        public static ObservableCollection<Service> GetServicesFromXMLAndCheck(byte[] resource)
        {
            ObservableCollection<Service> servicesToRestore = new ObservableCollection<Service>();
            Helpers.DeserializeCollectionAsXML(resource, out ObservableCollection<Service> dependingServices);

            foreach (Service service in dependingServices)
            {
                if (IsExists(service) == false)
                {
                    servicesToRestore.Add(service);
                    continue;
                }

                using (ServiceController _controller = new ServiceController(service.ShortName))
                {
                    if (_controller.Status != ServiceControllerStatus.Running)
                        servicesToRestore.Add(service);
                }
            }

            return servicesToRestore;
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
        /// Restores Winmgmt service
        /// </summary>
        public static void RestoreWMIService()
        {
            Helpers.ExtractEmbedFile(Properties.Resources.Winmgmt, "Winmgmt.reg");
            RunAsProcess.CMD($"reg import {Path.GetTempPath()}Winmgmt.reg", true, true, "lsass");
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
                    if (_service.CanRemove == Service.ServiceRemovingCondition.WithCaution)
                    {
                        bool _result = DialogHelper.ShowDialog(dialogService,
                            $@"{DialogConsts.DialogNonRemovableServiceCaption} ""{_service.FullName}""",
                            $@"{DialogConsts.DialogNonRemovableServiceMessage}{Environment.NewLine}{Environment.NewLine}Описание службы: {_service.Description}");

                        if (!_result)
                        {
                            _service.IsChecked = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to enable 'Winmgmt' service
        /// </summary>
        public static void EnableWMIService()
        {
            using (ServiceController controller = new ServiceController("Winmgmt"))
            {
                if (controller.StartType == ServiceStartMode.Disabled)
                    Helpers.RunByCMD($"sc config Winmgmt start= demand");
            }
        }

        /// <summary>
        /// Tries to enable 'TrustedInstaller' service
        /// </summary>
        public static void EnableTrustedInstallerService()
        {
            using (ServiceController controller = new ServiceController("TrustedInstaller"))
            {
                if (controller.StartType == ServiceStartMode.Disabled)
                    Helpers.RunByCMD($"sc config TrustedInstaller start= demand");
            }
        }
    }
}