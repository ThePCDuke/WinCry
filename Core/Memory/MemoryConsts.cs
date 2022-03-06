namespace WinCry.Memory
{
    public static class MemoryConsts
    {
        #region Installer

        public const string ServiceName = "PСSLC Service";

        public const string ServicePath = @"C:/Windows/PСSLC_Service.exe";
        public const string ServiceLibraryPath = @"C:/Windows/PСSLC.Core.dll";
        public const string ServiceLogPath = @"C:/Windows/PСSLC_Service.InstallLog";
        public const string ServiceFolderPath = @"C:/Windows";
        public const string StartArguments = "/c cd C:/Windows/Microsoft.NET/Framework64/v4.0.30319 & InstallUtil.exe";
        public const string DeleteArgument = "/u";
        public const string FileName = "cmd.exe";
        public const string WorkingDirectory = "C:";

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

        #region Recovery

        public const string Arguments = "/c cd C:/windows/system32 & lodctr /r & cd C:/windows/SysWOW64 & lodctr /r";

        #endregion
    }
}
