using System;
using System.IO;

namespace WinCry.Memory
{
    public static class MemoryStrings
    {
        #region Installer

        public const string ServiceName = "PСSLC Service";

        public static string ServicePath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}Windows\PСSLC_Service.exe";
        public static string ServiceLibraryPath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}Windows\PСSLC.Core.dll";
        public static string ServiceLogPath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}Windows\PСSLC_Service.InstallLog";
        public static string ServiceFolderPath = $@"{Path.GetPathRoot(Environment.SystemDirectory)}\Windows";
        public static string StartArguments64 = $@"/c cd {Path.GetPathRoot(Environment.SystemDirectory)}Windows\Microsoft.NET\Framework64\v4.0.30319 & InstallUtil.exe";
        public static string StartArguments32 = $@"/c cd {Path.GetPathRoot(Environment.SystemDirectory)}Windows\Microsoft.NET\Framework\v4.0.30319 & InstallUtil.exe";
        public const string DeleteArgument = "/u";
        public const string FileName = "cmd.exe";
        public static string WorkingDirectory = Path.GetPathRoot(Environment.SystemDirectory);

        public const int RetryCount = 50;

        #endregion

        #region Info

        public const string ServiceIsNotInstalled = "Служба не установлена";
        public const string ServiceIsInstalled = "Служба установлена";
        public const string ServiceIsStopped = "Служба остановлена";
        public const string ServiceIsRunning = "Служба запущена";
        public const string ServiceIsDeleted = "Служба удалена";
        public const string ServiceIsNotSuccessfulyInstalled = "Не удалось установить службу";
        public const string ServiceIsNotSuccessfulyStarted = "Не удалось запустить службу";
        public const string ServiceIsNotSuccessfulyStopped = "Не удалось остановить службу";
        public const string ServiceIsNotSuccessfulyRemoved = "Не удалось удалить службу";

        #endregion
    }
}
