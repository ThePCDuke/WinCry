using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;
using PСSLC.Core;
using WinCry.Dialogs;
using WinCry.Models;
using WinCry.ViewModels;

namespace WinCry.Memory
{
    class MemoryModel
    {
        /// <summary>
        /// Installs PCSLC Service on current system
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="cachedRam">Cached memory is more than</param>
        /// <param name="freeMemory">Free memory is less than</param>
        /// <param name="interval">Interval between statements check</param>
        /// <returns></returns>
        public static Task InstallService(TaskViewModel taskViewModel, ulong cachedRam, ulong freeMemory, int interval)
        {
            return new Task(async () =>
            {
                taskViewModel.Name = DialogConsts.Memory;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                // Checking if already installed
                if (IsServiceInstalled)
                {
                    taskViewModel.CreateMessage($"{DialogConsts.AlreadyInstalled}", true);
                    taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
                    return;
                }
                // Extracting
                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakExtracting;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakExtracting} ");

                    Helpers.ExtractEmbedFile(Properties.Resources.PСSLC_Core, "PСSLC.Core.dll", MemoryConsts.ServiceFolderPath);
                    Helpers.ExtractEmbedFile(Properties.Resources.PСSLC_Service, "PСSLC_Service.exe", MemoryConsts.ServiceFolderPath);

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 25;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }
                // Installing
                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakInstalling;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakInstalling} ");

                    RegulationsDataWritter writter = new RegulationsDataWritter();
                    writter.Write(RegulationsData.Default);

                    string arguments = $"{MemoryConsts.StartArguments} {MemoryConsts.ServicePath}";
                    using (Process installProcess = new Process())
                    {
                        installProcess.StartInfo.WorkingDirectory = MemoryConsts.WorkingDirectory;
                        installProcess.StartInfo.FileName = MemoryConsts.FileName;
                        installProcess.StartInfo.Arguments = arguments;
                        installProcess.StartInfo.CreateNoWindow = true;
                        installProcess.StartInfo.UseShellExecute = false;
                        installProcess.Start();
                        installProcess.WaitForExit();
                    }

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 25;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }
                // Checking
                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakChecking;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakChecking} ");

                    int retryCount = 0;
                    while (!IsServiceInstalled && retryCount < 10)
                    {
                        await Task.Delay(1000);
                        retryCount++;
                    }
                    if (retryCount == 10)
                    {
                        throw new InvalidOperationException(MemoryConsts.ServiceIsNotSuccessfulyInstalled);
                    }

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 25;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }
                // Starting
                try
                {
                    var _task = StartService(taskViewModel, cachedRam, freeMemory, interval);
                    _task.Start();
                    _task.Wait();
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }
            });
        }

        /// <summary>
        /// Uninstalls PCSLC Service from current system
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <returns></returns>
        public static Task UninstallService(TaskViewModel taskViewModel)
        {
            return new Task(async () =>
            {
                taskViewModel.Name = DialogConsts.Memory;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                // Checking if not installed
                if (!IsServiceInstalled)
                {
                    taskViewModel.CreateMessage($"{DialogConsts.NotInstalled}", true);
                    taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
                    return;
                }
                // Uninstalling
                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakUninstalling;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakUninstalling} ");

                    var arguments = $"{MemoryConsts.StartArguments} {MemoryConsts.ServicePath} {MemoryConsts.DeleteArgument}";
                    using (Process deleteProcess = new Process())
                    {
                        deleteProcess.StartInfo.WorkingDirectory = MemoryConsts.WorkingDirectory;
                        deleteProcess.StartInfo.FileName = MemoryConsts.FileName;
                        deleteProcess.StartInfo.Arguments = arguments;
                        deleteProcess.StartInfo.CreateNoWindow = true;
                        deleteProcess.StartInfo.UseShellExecute = false;
                        deleteProcess.Start();
                        deleteProcess.WaitForExit();
                    }

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 50;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }
                // Checking
                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakChecking;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakChecking} ");

                    int retryCount = 0;
                    while (IsServiceInstalled && retryCount < 10)
                    {
                        await Task.Delay(1000);
                        retryCount++;
                    }
                    if (retryCount == 10)
                    {
                        throw new InvalidOperationException(MemoryConsts.ServiceIsNotSuccessfulyRemoved);
                    }
                    else
                    {
                        RegulationsDataWritter writter = new RegulationsDataWritter();
                        writter.RemoveAll();

                        if (File.Exists(MemoryConsts.ServicePath))
                            File.Delete(MemoryConsts.ServicePath);

                        if (File.Exists(MemoryConsts.ServiceLibraryPath))
                            File.Delete(MemoryConsts.ServiceLibraryPath);

                        if (File.Exists(MemoryConsts.ServiceLogPath))
                            File.Delete(MemoryConsts.ServiceLogPath);
                    }

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 50;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        /// <summary>
        /// Starts PCSLC Service on current system
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="cachedRam">Cached memory is more than</param>
        /// <param name="freeMemory">Free memory is less than</param>
        /// <param name="interval">Interval between statements check</param>
        /// <returns></returns>
        public static Task StartService(TaskViewModel taskViewModel, ulong cachedRam, ulong freeMemory, int interval)
        {
            return new Task(async () =>
            {
                if (taskViewModel.Name == null)
                    taskViewModel.Name = DialogConsts.Memory;
                if (taskViewModel.Details == null)
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakStarting;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakStarting} ");

                    RegulationsData _data = new RegulationsData(cachedRam, freeMemory, interval);
                    WriteUserPreferences(_data);

                    if (!IsServiceInstalled)
                    {
                        taskViewModel.CreateFailureMessage(MemoryConsts.ServiceIsNotInstalled);
                        taskViewModel.CreateMessage(DialogConsts.ApplyingDoneMessage);
                        return;
                    }
                    if (IsServiceRunning)
                    {
                        taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
                        return;
                    }

                    GetService().Start();

                    int retryCount = 0;
                    while (GetService().Status != ServiceControllerStatus.Running && retryCount < 10)
                    {
                        await Task.Delay(1000);
                        retryCount++;
                    }

                    if (GetService().Status != ServiceControllerStatus.Running)
                    {
                        taskViewModel.CreateFailureMessage(MemoryConsts.ServiceIsNotSuccessfulyStarted);
                        taskViewModel.CreateMessage(DialogConsts.ApplyingDoneMessage);
                        return;
                    }

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress = 100;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        /// <summary>
        /// Stops PCSLC Service on current system
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <returns></returns>
        public static Task StopService(TaskViewModel taskViewModel)
        {
            return new Task(async () =>
            {
                if (taskViewModel.Name == null)
                    taskViewModel.Name = DialogConsts.Memory;
                if (taskViewModel.Details == null)
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.MemoryTweakStopping;
                    taskViewModel.CreateMessage($"{DialogConsts.MemoryTweakStopping} ");

                    if (!IsServiceInstalled)
                    {
                        taskViewModel.CreateFailureMessage(MemoryConsts.ServiceIsNotInstalled);
                        taskViewModel.CreateMessage(DialogConsts.ApplyingDoneMessage);
                        return;
                    }
                    if (!IsServiceRunning)
                    {
                        taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
                        return;
                    }

                    GetService().Stop();

                    int retryCount = 0;
                    while (GetService().Status != ServiceControllerStatus.Stopped && retryCount < 10)
                    {
                        await Task.Delay(100);
                        retryCount++;
                    }

                    if (GetService().Status != ServiceControllerStatus.Stopped)
                    {
                        taskViewModel.CreateFailureMessage(MemoryConsts.ServiceIsNotSuccessfulyStarted);
                        taskViewModel.CreateMessage(DialogConsts.ApplyingDoneMessage);
                    }

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress = 100;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        /// <summary>
        /// Writes user service's preferences to registry
        /// </summary>
        /// <param name="servicePrefs"></param>
        public static void WriteUserPreferences(RegulationsData servicePrefs)
        {
            var data = servicePrefs;
            var writter = new RegulationsDataWritter();
            try
            {
                writter.Write(data);
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Indicates whether service is installed or not
        /// </summary>
        public static bool IsServiceInstalled
        {
            get
            {
                return GetService() != null;
            }
        }

        /// <summary>
        /// Indicates whether service is running or not
        /// </summary>
        public static bool IsServiceRunning
        {
            get
            {
                if (!IsServiceInstalled)
                {
                    return false;
                }
                return GetService().Status == ServiceControllerStatus.Running;
            }
        }

        /// <summary>
        /// Gets PCSLC Service
        /// </summary>
        /// <returns></returns>
        private static ServiceController GetService()
        {
            ServiceController[] services;
            try
            {
                services = ServiceController.GetServices();
            }
            catch (Exception)
            {
                throw;
            }
            var service = services.FirstOrDefault(s => s.ServiceName == MemoryConsts.ServiceName);
            return service;
        }
    }
}