using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WinCry.Dialogs;
using WinCry.Services;
using WinCry.ViewModels;

namespace WinCry.Models
{
    class MainModel
    {
        /// <summary>
        /// Opens YouTube channel link with default browser
        /// </summary>
        public static void GotoChannel()
        {
            System.Diagnostics.Process.Start(StringConsts.URLChannel);
        }

        /// <summary>
        /// Opens community hub link with default browser
        /// </summary>
        public static void GotoCommunity()
        {
            System.Diagnostics.Process.Start(StringConsts.URLCommunity);
        }

        /// <summary>
        /// Opens community app discussion topic link with default browser
        /// </summary>
        public static void GotoCommunityDiscussion()
        {
            System.Diagnostics.Process.Start(StringConsts.URLCommunityDiscussion);
        }

        /// <summary>
        /// Opens community app discussion topic link with default browser
        /// </summary>
        public static void GotoDonation()
        {
            System.Diagnostics.Process.Start(StringConsts.URLDonation);
        }

        /// <summary>
        /// Executed when MainWindow is done loading
        /// </summary>
        public static void Loaded(IDialogService dialogService)
        {
            using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\", true))
            {
                bool _doNotShowDisclaimer = false;

                RegistryKey _wincryKey = _registryKey.OpenSubKey("WinCry", true);

                if (_wincryKey != null)
                {
                    if (_wincryKey.GetValue("DoNotShowDisclaimer") != null)
                        _doNotShowDisclaimer = Byte.Parse((string)_wincryKey.GetValue("DoNotShowDisclaimer")) == 1 ? true : false;
                }

                if (!_doNotShowDisclaimer)
                {
                    if (DialogHelper.ShowDisclaimer(dialogService))
                    {
                        _registryKey.CreateSubKey("WinCry");
                        _wincryKey = _registryKey.OpenSubKey("WinCry", true);
                        _wincryKey.SetValue("DoNotShowDisclaimer", "1", RegistryValueKind.String);
                    }
                }
            }

            try
            {
                RunAsProcess.StartTrustedInstallerService();
            }
            catch (InvalidOperationException)
            {
                if (DialogHelper.ShowDialog(dialogService, DialogConsts.ServiceStartingError, DialogConsts.TrustedInstallerStartingError))
                {
                    ServicesModel.RestoreTrustedInstallerService();
                }
                else
                {
                    Environment.Exit(0);
                }
            }
        }

        /// <summary>
        /// Creates "Presets" folder at application's location
        /// </summary>
        public static void CreatePresetsFolder()
        {
            if (!Directory.Exists(@"Presets"))
                Directory.CreateDirectory(@"Presets");
        }

        /// <summary>
        /// Applies SetTimerResolution tweak
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task InstallImprovedTimer(TaskViewModel taskViewModel, bool install)
        {
            string _filePath = @"C:\Windows\SetTimerResolutionService.exe";
            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.TimerTweak;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.TimerTweakInstallingDependecy;
                    taskViewModel.CreateMessage($"{DialogConsts.TimerTweakInstallingDependecy} ");

                    TweaksCollection.VCTweak.InstallDependent(); //Replace

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 33;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.TimerTweakExtracting;
                    taskViewModel.CreateMessage($"{DialogConsts.TimerTweakExtracting} ");

                    if (!File.Exists(_filePath))
                    {
                        Helpers.ExtractEmbedFile(Properties.Resources.SetTimerResolutionService, "SetTimerResolutionService.exe", @"C:\Windows");
                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    }
                    else
                    {
                        taskViewModel.CreateMessage(DialogConsts.AlreadyExists, false, false);
                    }

                    taskViewModel.Progress += 33;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                try
                {
                    if (install)
                    {
                        // Setting bcdedit
                        taskViewModel.ShortMessage = DialogConsts.TimerTweakInstalling;
                        taskViewModel.CreateMessage($"{DialogConsts.TimerTweakInstalling} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD(@"C:\Windows\SetTimerResolutionService -install", true), false, false);

                        taskViewModel.Progress += 17;

                        // Installing Timer
                        taskViewModel.ShortMessage = DialogConsts.TimerTweakReset;
                        taskViewModel.CreateMessage($"{DialogConsts.TimerTweakReset} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD(@"bcdedit /deletevalue useplatformclock", true), true, false);
                        taskViewModel.CreateMessage(RunAsProcess.CMD(@"bcdedit /deletevalue tscsyncpolicy", true), true, false);
                        taskViewModel.CreateMessage(RunAsProcess.CMD(@"bcdedit /set disabledynamictick yes", true), true, false);

                        taskViewModel.Progress += 17;
                    }
                    else
                    {
                        taskViewModel.ShortMessage = DialogConsts.TimerTweakUninstalling;
                        taskViewModel.CreateMessage($"{DialogConsts.TimerTweakUninstalling} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD(@"C:\Windows\SetTimerResolutionService -uninstall", true), false, false);

                        if (File.Exists(_filePath))
                        {
                            taskViewModel.CreateMessage(RunAsProcess.CMD($"taskkill /im SetTimerResolutionService.exe /f", true));
                            taskViewModel.CreateMessage(RunAsProcess.CMD(@"del /s C:\Windows\SetTimerResolutionService.exe", true));
                        }

                        taskViewModel.Progress += 34;
                    }
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        } // Change VS

        /// <summary>
        /// Applies configured ultimate performance power scheme
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task InstallPowerScheme(TaskViewModel taskViewModel, bool install)
        {
            return new Task(() =>
            {
                string _filePath = Path.GetTempPath() + "PCDuke_Scheme.pow";
                taskViewModel.Name = DialogConsts.SchemeTweak;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    if (install)
                    {
                        // Extracting
                        taskViewModel.ShortMessage = DialogConsts.SchemeTweakExtracting;
                        taskViewModel.CreateMessage($"{DialogConsts.SchemeTweakExtracting} ");

                        if (!File.Exists(_filePath))
                        {
                            Helpers.ExtractEmbedFile(Properties.Resources.PCDuke_Scheme, "PCDuke_Scheme.pow");
                            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                        }
                        else
                        {
                            taskViewModel.CreateMessage(DialogConsts.AlreadyExists, false, false);
                        }

                        taskViewModel.Progress += 33;

                        //Importing
                        taskViewModel.ShortMessage = DialogConsts.SchemeTweakImporting;
                        taskViewModel.CreateMessage($"{DialogConsts.SchemeTweakImporting} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD($"powercfg -import \"{_filePath}\" 77777777-7777-7777-7777-777777777777", true), false, false);

                        taskViewModel.Progress += 33;

                        // Setting up as active
                        taskViewModel.ShortMessage = DialogConsts.SchemeTweakApplying;
                        taskViewModel.CreateMessage($"{DialogConsts.SchemeTweakApplying} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD("powercfg -setactive 77777777-7777-7777-7777-777777777777", true), false, false);

                        taskViewModel.Progress += 34;
                    }
                    else
                    {
                        //Setting up default scheme
                        taskViewModel.ShortMessage = DialogConsts.SchemeTweakApplyingDefault;
                        taskViewModel.CreateMessage($"{DialogConsts.SchemeTweakApplyingDefault} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD("powercfg -setactive \"8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c\"", true), false, false);

                        taskViewModel.Progress += 33;

                        // Deleting perf scheme from list
                        taskViewModel.ShortMessage = DialogConsts.SchemeTweakRemovingFromList;
                        taskViewModel.CreateMessage($"{DialogConsts.SchemeTweakRemovingFromList} ");

                        taskViewModel.CreateMessage(RunAsProcess.CMD("powercfg -d 77777777-7777-7777-7777-777777777777", true), false, false);

                        taskViewModel.Progress += 33;

                        // Deleting perf scheme from disk
                        taskViewModel.ShortMessage = DialogConsts.SchemeTweakRemovingFromDisk;
                        taskViewModel.CreateMessage($"{DialogConsts.SchemeTweakRemovingFromDisk} ");

                        if (File.Exists(_filePath))
                        {
                            taskViewModel.CreateMessage(RunAsProcess.CMD($@"del /s ""{_filePath}""", true));
                        }
                        else
                        {
                            taskViewModel.CreateMessage(DialogConsts.DoesntExist, false, false);
                        }
                        
                        taskViewModel.Progress += 34;
                    }
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        public static Task SetGPUSettings(TaskViewModel taskViewModel, bool install, bool doPrimary, bool doSecondary)
        {
            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.GPUTweak;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    ObservableCollection<string> _gpus = Helpers.GetInstalledGPUManufacturers();
                    taskViewModel.Progress += 50;

                    if (install)
                    {
                        if (doPrimary && _gpus.Count > 0) { ApplyGPUTweak(taskViewModel, _gpus[0], true); };
                        if (doSecondary && _gpus.Count > 1) { ApplyGPUTweak(taskViewModel, _gpus[1], true); };
                    }
                    else
                    {
                        if (doPrimary && _gpus.Count > 0) { ApplyGPUTweak(taskViewModel, _gpus[0], false); };
                        if (doSecondary && _gpus.Count > 1) { ApplyGPUTweak(taskViewModel, _gpus[1], false); };
                    }
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

        private static void ApplyGPUTweak(TaskViewModel taskViewModel, string GPU, bool install)
        {
            switch (GPU)
            {
                case "AMD":
                    {
                        if (install) { InstallAMD(taskViewModel); }
                        else { UninstallAMD(taskViewModel); };

                        break;
                    }
                case "NVIDIA":
                    {
                        if (install) { ImportNVIDIAProfile(taskViewModel, Properties.Resources.nvidiaConfigured); }
                        else { ImportNVIDIAProfile(taskViewModel, Properties.Resources.nvidiaDefault); }

                        break;
                    }
            }

        }

        private static void ImportNVIDIAProfile(TaskViewModel taskViewModel, byte[] file)
        {
            taskViewModel.CreateMessage($"{DialogConsts.GPUTweakNVIDIAExtracting} ");
            Helpers.ExtractEmbedFile(file, "nvidia");
            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

            taskViewModel.CreateMessage($"{DialogConsts.GPUTweakNVIDIAImporting} ");
            NvAPIWrapper.DRS.DriverSettingsSession _session = NvAPIWrapper.DRS.DriverSettingsSession.CreateAndLoad(Path.GetTempPath() + "nvidia");
            _session.Save();
            _session.CurrentGlobalProfile.Session.Save();
            _session.Dispose();
            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
        }

        private static void InstallAMD(TaskViewModel taskViewModel)
        {
            taskViewModel.CreateMessage($"{DialogConsts.GPUTweakAMDEditingRegKeys} ");
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", true))
            {
                _registryKey.SetValue("StutterMode", 0, RegistryValueKind.DWord);
                _registryKey.SetValue("EnableUlps", 0, RegistryValueKind.DWord);
                _registryKey.SetValue("PP_ThermalAutoThrottlingEnable", 0, RegistryValueKind.DWord);
                _registryKey.SetValue("DisableDrmdmaPowerGating", 0, RegistryValueKind.DWord);
            }
            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
        }

        private static void UninstallAMD(TaskViewModel taskViewModel)
        {
            taskViewModel.CreateMessage($"{DialogConsts.GPUTweakAMDEditingRegKeys} ");
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}\0000", true))
            {
                if (_registryKey.GetValue("DisableDMACopy") != null)
                    _registryKey.DeleteValue("DisableDMACopy");

                if (_registryKey.GetValue("DisableBlockWrite") != null)
                    _registryKey.DeleteValue("DisableBlockWrite");

                _registryKey.SetValue("StutterMode", 2, RegistryValueKind.DWord);

                _registryKey.SetValue("EnableUlps", 1, RegistryValueKind.DWord);

                if (_registryKey.GetValue("PP_ThermalAutoThrottlingEnable") != null)
                    _registryKey.DeleteValue("PP_ThermalAutoThrottlingEnable");

                if (_registryKey.GetValue("DisableDrmdmaPowerGating") != null)
                    _registryKey.DeleteValue("DisableDrmdmaPowerGating");
            }
            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
        }

        public static Task SetPagefile(TaskViewModel taskViewModel, byte option)
        {
            int _value = option * 4096;
            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.PagefileTweak;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.PagfileTweakApplying;
                    taskViewModel.CreateMessage($"{DialogConsts.PagfileTweakApplying} ");

                    using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management\", true))
                    {
                        if (_value == 0)
                        {
                            _registryKey.SetValue("PagingFiles", @"?:\pagefile.sys");
                        }
                        else
                        {
                            _registryKey.SetValue("PagingFiles", $@"c:\pagefile.sys {_value} {_value}");
                        }
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
        /// Applies tweaks to increase booting speed time
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task IncreaseBootSpeed(TaskViewModel taskViewModel, bool install)
        {
            string _filePath = Path.GetTempPath() + "BootSpeedTweak.bat";
            string _message = null;
            byte[] _resource = null;

            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.IncreaseOSSpeedTweak;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    if (install)
                    {
                        _message = DialogConsts.IncreaseOSTweakInstalling;
                        _resource = Properties.Resources.IncreaseBootSpeed;
                    }
                    else
                    {
                        _message = DialogConsts.IncreaseOSTweakUninstalling;
                        _resource = Properties.Resources.DecreaseBootSpeed;
                    }

                    taskViewModel.Progress += 50;

                    // Executing bat file
                    taskViewModel.ShortMessage = _message;
                    taskViewModel.CreateMessage($"{_message} ");

                    using (StreamReader _stream = new StreamReader(new MemoryStream(_resource)))
                    {
                        while (_stream.Peek() >= 0)
                        {
                            string _line = _stream.ReadLine();
                            taskViewModel.CreateMessage($"{_line} ");
                            taskViewModel.CreateMessage(RunAsProcess.CMD(_line, true), true, false);
                        }
                    }

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
        /// Removes arrow icon from shortcuts
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task RemoveShortcutIcon(TaskViewModel taskViewModel, bool install)
        {
            string _filePath = @"C:\Windows\blank.ico";
            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.RemoveShortcutIconTweak;
                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    if (install)
                    {
                        // Extracting icon on disk
                        taskViewModel.ShortMessage = DialogConsts.RemoveShortcutIconTweakExtracting;
                        taskViewModel.CreateMessage($"{DialogConsts.RemoveShortcutIconTweakExtracting} ");

                        if (!File.Exists(_filePath))
                        {
                            Helpers.ExtractEmbedFile(Properties.Resources.BlankIco, "blank.ico", @"C:\Windows");
                            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                        }
                        else
                        {
                            taskViewModel.CreateMessage(DialogConsts.AlreadyExists, false, false);
                        }

                        taskViewModel.Progress += 50;

                        // Creating registry keys
                        taskViewModel.ShortMessage = DialogConsts.CreatingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.CreatingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\", true))
                        {
                            _registryKey.CreateSubKey("Shell Icons").SetValue("29", @"%windir%\blank.ico,0", RegistryValueKind.String);
                        }

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

                        taskViewModel.Progress += 50;
                    }
                    else
                    {
                        // Removing registry keys
                        taskViewModel.ShortMessage = DialogConsts.RemovingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.RemovingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\", true))
                        {
                            RegistryKey _ret = _registryKey.OpenSubKey("Shell Icons");

                            if (_ret != null)
                            {
                                _registryKey.DeleteSubKeyTree("Shell Icons");
                                taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                            }
                            else
                            {
                                taskViewModel.CreateMessage(DialogConsts.DoesntExist, false, false);
                            }
                        }
                        taskViewModel.Progress += 50;

                        // Removing icon from disk
                        taskViewModel.ShortMessage = DialogConsts.RemoveShortcutIconTweakRemovingFromDisk;
                        taskViewModel.CreateMessage($"{DialogConsts.RemoveShortcutIconTweakRemovingFromDisk} ");

                        if(File.Exists(_filePath))
                        {
                            taskViewModel.CreateMessage(RunAsProcess.CMD($@"del /s ""{_filePath}""", true), false, false);
                        }
                        else
                        {
                            taskViewModel.CreateMessage(DialogConsts.DoesntExist, false, false);
                        }
                        
                        taskViewModel.Progress += 50;
                    }
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
        /// Activates windows
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <returns></returns>
        public static Task ActivateWindows(TaskViewModel taskViewModel)
        {
            string _filePath = Path.GetTempPath() + "HWID.bat";
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.ActivateWindows;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    taskViewModel.ShortMessage = DialogConsts.ExtractingScripts;
                    taskViewModel.CreateMessage($"{DialogConsts.ExtractingScripts} ");
                    Helpers.ExtractEmbedFile(Properties.Resources.HWID, "HWID.bat");
                    Helpers.ExtractEmbedFile(Properties.Resources.KMS38, "KMS38.bat");
                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

                    taskViewModel.Progress += 50;

                    taskViewModel.ShortMessage = DialogConsts.ApplyingScripts;
                    taskViewModel.CreateMessage($"{DialogConsts.ApplyingScripts} ");
                    RunAsProcess.CMD(Path.GetTempPath() + "HWID.bat", true, true);
                    RunAsProcess.CMD(Path.GetTempPath() + "KMS38.bat", true, true);
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
        /// Downloads and installs Microsoft VC++
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="dialogService">DialogService for dialog prompts</param>
        /// <returns></returns>
        public static Task InstallVC(TaskViewModel taskViewModel, IDialogService dialogService)
        {
            string _zipPath = Path.GetTempPath() + "VC.zip";
            string _zipExtractionDirectory = Path.GetTempPath() + @"VC\";

            bool _isDownloaded = false;
            bool _isCorrupted = false;

            if (File.Exists(StringConsts.VCZipFilePath))
            {
                try
                {
                    using (HashAlgorithm _hashAlgorithm = MD5.Create())
                    {
                        byte[] _bytes = File.ReadAllBytes(StringConsts.VCZipFilePath);
                        byte[] _hash = _hashAlgorithm.ComputeHash(_bytes);

                        StringBuilder _builder = new StringBuilder();
                        foreach (byte _byte in _hash)
                            _builder.Append(_byte.ToString("x2").ToLower());

                        if (_builder.ToString() != StringConsts.MD5VC)
                        {
                            _isCorrupted = true;
                        }
                    }
                }
                catch
                {
                    _isCorrupted = true;
                }

                if (_isCorrupted == false)
                {
                    _isDownloaded = true;
                }
            }

            if (_isDownloaded)
            {
                if (DialogHelper.ShowDialog(dialogService, DialogConsts.BaseDialogInstallVCCaption, DialogConsts.BaseDialogInstallVCOverwriteMessage))
                    _isDownloaded = DialogHelper.ShowDownloadDialog(dialogService, StringConsts.URLVC, "VC.zip");
            }
            else
            {
                _isDownloaded = DialogHelper.ShowDownloadDialog(dialogService, StringConsts.URLVC, "VC.zip");
            }

            return new Task(() =>
            {
                taskViewModel.Name = DialogConsts.InstallMSVC;
                
                if (!_isDownloaded)
                {
                    taskViewModel.CreateFailureMessage(DialogConsts.InstallMSVCCanceled);
                    return;
                }

                taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.InstallMSVCExtracting;
                    taskViewModel.CreateMessage($"{DialogConsts.InstallMSVCExtracting} ");

                    Helpers.UnzipFromFile(_zipPath, _zipExtractionDirectory);

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 50;
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                try
                {
                    taskViewModel.ShortMessage = DialogConsts.InstallMSVCInstalling;
                    taskViewModel.CreateMessage($"{DialogConsts.InstallMSVCInstalling} ");

                    RunAsProcess.CMD($@"{_zipExtractionDirectory}install_all.bat", true, true);

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
        /// Applies Auto Temp Cleaner tweak
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task InstallAutoTempCleaner(TaskViewModel taskViewModel, bool install)
        {
            string _filePath = @"C:\Windows\TempCleaner.exe";
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.TempTweak;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    if (install)
                    {
                        // Extracting app on disk
                        taskViewModel.ShortMessage = DialogConsts.TempTweakExtracting;
                        taskViewModel.CreateMessage($"{DialogConsts.TempTweakExtracting} ");

                        if (!File.Exists(_filePath))
                        {
                            Helpers.ExtractEmbedFile(Properties.Resources.TempCleaner, "TempCleaner.exe", @"C:\Windows");
                            taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                        }
                        else
                        {
                            taskViewModel.CreateMessage(DialogConsts.AlreadyExists, false, false);
                        }

                        taskViewModel.Progress += 33;

                        // Creating registry keys
                        taskViewModel.ShortMessage = DialogConsts.CreatingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.CreatingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true))
                        {
                            _registryKey.SetValue("Auto Temp Cleaner", "\"C:\\Windows\\TempCleaner.exe\"", RegistryValueKind.String);
                        }

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

                        taskViewModel.Progress += 33;

                        // Editing registry keys
                        taskViewModel.ShortMessage = DialogConsts.EditingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.EditingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy\", true))
                        {
                            _registryKey.SetValue("01", 0, RegistryValueKind.DWord);
                            _registryKey.SetValue("04", 0, RegistryValueKind.DWord);
                        }

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

                        taskViewModel.Progress += 34;
                    }
                    else
                    {
                        // Removing registry keys
                        taskViewModel.ShortMessage = DialogConsts.RemovingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.RemovingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\", true))
                        {
                            if (_registryKey.GetValue("Auto Temp Cleaner") != null)
                            {
                                _registryKey.DeleteValue("Auto Temp Cleaner");
                                taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                            }
                            else
                            {
                                taskViewModel.CreateMessage(DialogConsts.DoesntExist, false, false);
                            }
                        }

                        taskViewModel.Progress += 33;

                        // Editing registry keys
                        taskViewModel.ShortMessage = DialogConsts.EditingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.EditingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy\", true))
                        {
                            _registryKey.SetValue("01", 1, RegistryValueKind.DWord);
                            _registryKey.SetValue("04", 1, RegistryValueKind.DWord);
                        }

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                        taskViewModel.Progress += 33;

                        // Removing app from disk
                        taskViewModel.ShortMessage = DialogConsts.TempTweakRemoving;
                        taskViewModel.CreateMessage($"{DialogConsts.TempTweakRemoving} ");

                        if (File.Exists(_filePath))
                        {
                            taskViewModel.CreateMessage(RunAsProcess.CMD($@"del /s ""{_filePath}""", true), false, false);
                        }
                        else
                        {
                            taskViewModel.CreateMessage(DialogConsts.DoesntExist, false, false);
                        }

                        taskViewModel.Progress += 34;
                    }
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
        /// Sets TEMP and TMP variables
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task SetTempVariables(TaskViewModel taskViewModel, bool install)
        {
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.TempVariableTweak;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    if (install)
                    {
                        taskViewModel.ShortMessage = DialogConsts.TempVariableTweakApplying;
                        taskViewModel.CreateMessage($"{DialogConsts.TempVariableTweakApplying} ");

                        Helpers.RunByCMD(@"setx temp ""C:\Windows\Temp""");
                        Helpers.RunByCMD(@"setx /m temp ""C:\Windows\Temp""");
                        Helpers.RunByCMD(@"setx tmp ""C:\Windows\Temp""");
                        Helpers.RunByCMD(@"setx /m tmp ""C:\Windows\Temp""");

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                        taskViewModel.Progress = 100;
                    }
                    else
                    {
                        string _localAppdataPath = Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%");

                        taskViewModel.ShortMessage = DialogConsts.TempVariableTweakApplying;
                        taskViewModel.CreateMessage($"{DialogConsts.TempVariableTweakApplying} ");

                        Helpers.RunByCMD($@"setx temp ""{_localAppdataPath}\Temp""");
                        Helpers.RunByCMD($@"setx /m temp C:\Windows\Temp");
                        Helpers.RunByCMD($@"setx tmp ""{_localAppdataPath}\Temp""");
                        Helpers.RunByCMD($@"setx /m tmp C:\Windows\Temp");

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                        taskViewModel.Progress = 100;
                    }
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
        /// Removes Notification Icon from task bar
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task RemoveNotificationIcon(TaskViewModel taskViewModel, bool install)
        {
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.RemoveNotificationIconTweak;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    if (install)
                    {
                        // Creating registry keys
                        taskViewModel.ShortMessage = DialogConsts.CreatingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.CreatingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\", true))
                        {
                            RegistryKey _explorerKey = _registryKey.OpenSubKey("Explorer");

                            if (_explorerKey == null)
                                _registryKey.CreateSubKey("Explorer");

                            _explorerKey = _registryKey.OpenSubKey("Explorer", true);
                            _explorerKey.SetValue("DisableNotificationCenter", 1, RegistryValueKind.DWord);
                        }

                        taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    }
                    else
                    {
                        // Removing registry keys
                        taskViewModel.ShortMessage = DialogConsts.RemovingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.RemovingRegKeys} ");

                        using (RegistryKey _registryKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\", true))
                        {
                            RegistryKey _explorerKey = _registryKey.OpenSubKey("Explorer", true);

                            if (_explorerKey != null && _explorerKey.GetValue("DisableNotificationCenter") != null)
                            {
                                _explorerKey.DeleteValue("DisableNotificationCenter");
                                taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                            }
                            else
                            {
                                taskViewModel.CreateMessage(DialogConsts.DoesntExist, false, false);
                            }
                        }
                    }
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
        /// Removes folders from "This PC"
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task RemoveFoldersFromThisPC(TaskViewModel taskViewModel, bool install)
        {
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.RemoveFoldersFromThisPCTweak;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    if (install)
                    {
                        // Removing registry keys
                        taskViewModel.ShortMessage = DialogConsts.RemovingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.RemovingRegKeys} ");

                        RemoveFoldersFromThisPC32(taskViewModel);

                        if (Environment.Is64BitOperatingSystem)
                        {
                            RemoveFoldersFromThisPC64(taskViewModel);
                        }
                    }
                    else
                    {
                        // Creating registry keys
                        taskViewModel.ShortMessage = DialogConsts.CreatingRegKeys;
                        taskViewModel.CreateMessage($"{DialogConsts.CreatingRegKeys} ");

                        CreateFoldersOnThisPC32(taskViewModel);

                        if (Environment.Is64BitOperatingSystem)
                        {
                            CreateFoldersOnThisPC64(taskViewModel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    taskViewModel.CatchException(ex);
                    return;
                }

                taskViewModel.CreateSuccessMessage(DialogConsts.ApplyingDoneMessage);
            });
        }

        private static ObservableCollection<String> FoldersInThisPC()
        {
            return new ObservableCollection<string>()
            {
                "0DB7E03F-FC29-4DC6-9020-FF41B59E513A",
                "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}",
                "{A8CDFF1C-4878-43be-B5FD-F8091C1C60D0}",
                "{d3162b92-9365-467a-956b-92703aca08af}",
                "{374DE290-123F-4565-9164-39C4925E467B}",
                "{088e3905-0323-4b02-9826-5d99428e115f}",
                "{1CF1260C-4DD0-4ebb-811F-33C572699FDE}",
                "{3dfdf296-dbec-4fb4-81d1-6a3438bcf4de}",
                "{3ADD1653-EB32-4cb0-BBD7-DFA0ABB5ACCA}",
                "{24ad3ad4-a569-4530-98e1-ab02f9417aa8}",
                "{A0953C92-50DC-43bf-BE83-3742FED03C9C}",
                "{f86fa3ab-70d2-4fc7-9c99-fcbf05467f3a}",
            };
        }

        private static void RemoveFoldersFromThisPC32(TaskViewModel taskViewModel)
        {
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\", true))
            {
                foreach (String folder in FoldersInThisPC())
                {
                    RegistryKey key = _registryKey.OpenSubKey(folder, true);

                    if (key != null)
                    {
                        _registryKey.DeleteSubKey(folder);
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.Successful}", true, false);
                    }
                    else
                    {
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.DoesntExist}", true, false);
                    }
                }
            }
        }

        private static void RemoveFoldersFromThisPC64(TaskViewModel taskViewModel)
        {
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\", true))
            {
                foreach (String folder in FoldersInThisPC())
                {
                    RegistryKey key = _registryKey.OpenSubKey(folder, true);

                    if (key != null)
                    {
                        _registryKey.DeleteSubKey(folder);
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.Successful}", true, false);
                    }
                    else
                    {
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.DoesntExist}", true, false);
                    }
                }
            }
        }

        private static void CreateFoldersOnThisPC32(TaskViewModel taskViewModel)
        {
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\", true))
            {
                foreach (String folder in FoldersInThisPC())
                {
                    RegistryKey key = _registryKey.OpenSubKey(folder, true);

                    if (key == null)
                    {
                        _registryKey.CreateSubKey(folder);
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.Successful}", true, false);
                    }
                    else
                    {
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.AlreadyExists}", true, false);
                    }
                }
            }
        }

        private static void CreateFoldersOnThisPC64(TaskViewModel taskViewModel)
        {
            using (RegistryKey _registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\", true))
            {
                foreach (String folder in FoldersInThisPC())
                {
                    RegistryKey key = _registryKey.OpenSubKey(folder, true);

                    if (key == null)
                    {
                        _registryKey.CreateSubKey(folder);
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.Successful}", true, false);
                    }
                    else
                    {
                        taskViewModel.CreateMessage($"{folder} - {DialogConsts.AlreadyExists}", true, false);
                    }
                }
            }
        }

        /// <summary>
        /// Enables or disables DirectPlay component
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <param name="install">Install or uninstall</param>
        /// <returns></returns>
        public static Task SetDirectPlayComponent(TaskViewModel taskViewModel, bool install)
        {
            return new Task(() =>
            {
                string _script;
                try
                {
                    taskViewModel.Name = DialogConsts.SetDirectPlayComponentTweak;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    if (install)
                    {
                        _script = @"Enable-WindowsOptionalFeature -Online -FeatureName ""DirectPlay"" -All";
                    }
                    else
                    {
                        _script = @"Disable-WindowsOptionalFeature -Online -FeatureName ""DirectPlay""";
                    }

                    // Creating registry keys
                    taskViewModel.ShortMessage = DialogConsts.ApplyingScripts;
                    taskViewModel.CreateMessage($"{DialogConsts.ApplyingScripts} ");

                    RunAsProcess.CMD($@"powershell {_script}", true);

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
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
        /// Removes MS Store
        /// </summary>
        /// <param name="taskViewModel">TaskViewModel for result catching</param>
        /// <returns></returns>
        public static Task UninstallMSStore(TaskViewModel taskViewModel)
        {
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.UninstallMSStore;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    taskViewModel.ShortMessage = DialogConsts.UninstallMSStoreRemovingPackages;
                    taskViewModel.CreateMessage($"{DialogConsts.UninstallMSStoreRemovingPackages} ");

                    ProcessStartInfo _info = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        UseShellExecute = false,
                        Arguments = "Get-AppxPackage *windowsstore*|Remove-AppxPackage",
                        CreateNoWindow = true
                    };
                    Process.Start(_info).WaitForExit();

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);

                    taskViewModel.Progress += 50;

                    taskViewModel.ShortMessage = DialogConsts.UninstallMSStoreRemovingFiles;
                    taskViewModel.CreateMessage($"{DialogConsts.UninstallMSStoreRemovingFiles} ");

                    foreach (string _folder in Directory.GetDirectories(@"C:\Program Files\WindowsApps"))
                    {
                        if (_folder.StartsWith(@"C:\Program Files\WindowsApps\Microsoft.WindowsStore"))
                        {
                            RunAsProcess.CMD($@"RMDIR /S /Q ""{_folder}""", true);
                        }
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

        public static async Task InstallMSStorePrereqs(IDialogService dialogService)
        {
            bool _willDownload = true;
            bool _isCorrupted = false;

            if (File.Exists(StringConsts.WindowsStoreZipFilePath))
            {
                _willDownload = false;

                try
                {
                    using (HashAlgorithm _hashAlgorithm = MD5.Create())
                    {
                        byte[] _bytes = File.ReadAllBytes(StringConsts.WindowsStoreZipFilePath);
                        byte[] _hash = _hashAlgorithm.ComputeHash(_bytes);

                        StringBuilder _builder = new StringBuilder();
                        foreach (byte _byte in _hash)
                            _builder.Append(_byte.ToString("x2").ToLower());

                        if (_builder.ToString() != StringConsts.MD5MSStore && _builder.ToString() != StringConsts.MD5MSStoreLSTB)
                        {
                            _isCorrupted = true;
                            _willDownload = true;
                        }
                    }
                }
                catch
                {
                    _isCorrupted = true;
                }

                if (_isCorrupted == false)
                {
                    if (DialogHelper.ShowDialog(dialogService, DialogConsts.BaseDialogInstallMSStoreCaption, DialogConsts.BaseDialogInstallMSStoreOverwriteMessage))
                        _willDownload = true;
                }
            }

            if (_willDownload)
            {
                string _URL = StringConsts.URLMSStore;
                if (Helpers.GetWinBuild() == 14393)
                    _URL = StringConsts.URLMSStoreLTSB;

                if (!DialogHelper.ShowDownloadDialog(dialogService, _URL, "WindowsStore.zip"))
                {
                    if (!File.Exists(StringConsts.WindowsStoreZipFilePath))
                        File.Delete(StringConsts.WindowsStoreZipFilePath);

                    return;
                }
            }

            await ServicesModel.UpdateMSStoreDependingServicesAsync();

            ObservableCollection<Service> _servicesToRestore = ServicesModel.MSStoreDependingServicesToRestore();

            if (_servicesToRestore.Count > 0)
            {
                string _message = DialogConsts.BaseDialogInstallMSStoreDependingServicesMessage + Environment.NewLine;

                foreach (Service _service in _servicesToRestore)
                {
                    _message += $"- {_service.ShortName}\n";
                }

                if (DialogHelper.ShowDialog(dialogService, DialogConsts.BaseDialogInstallMSStoreCaption, _message))
                {
                    ServicesModel.ExtractRestorationFiles();
                    foreach (Service _service in _servicesToRestore)
                    {
                        ServicesModel.Restore(_service);
                    }

                    if (DialogHelper.ShowDialog(dialogService, DialogConsts.BaseDialogInstallMSStoreCaption, DialogConsts.BaseDialogInstallMSStoreRebootMessage))
                    {
                        Helpers.RunByCMD("shutdown /r /t 0");
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    return;
                }
            }
        }

        public static Task InstallMSStore(TaskViewModel taskViewModel, IDialogService dialogService)
        {
            return new Task(() =>
            {
                try
                {
                    taskViewModel.Name = DialogConsts.InstallMSStore;
                    taskViewModel.CreateMessage(DialogConsts.ApplyingStarted, false);

                    string _zipPath = Path.GetTempPath() + "WindowsStore.zip";
                    string _zipExtractionDirectory = Path.GetTempPath() + @"WindowsStore\";

                    if (!File.Exists(_zipPath))
                    {
                        throw new Exception(DialogConsts.MessageDialogDownloadErrorMessage);
                    }

                    taskViewModel.ShortMessage = DialogConsts.InstallMSVCExtracting;
                    taskViewModel.CreateMessage($"{DialogConsts.InstallMSVCExtracting} ");

                    Helpers.UnzipFromFile(_zipPath, _zipExtractionDirectory);

                    taskViewModel.CreateMessage(DialogConsts.Successful, false, false);
                    taskViewModel.Progress += 50;

                    taskViewModel.ShortMessage = DialogConsts.InstallMSStoreInstalling;
                    taskViewModel.CreateMessage($"{DialogConsts.InstallMSStoreInstalling} ");

                    RunAsProcess.CMD($@"{_zipExtractionDirectory}install.bat", true, true);

                    taskViewModel.CreateMessage(DialogConsts.Done, false, false);
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
    }
}