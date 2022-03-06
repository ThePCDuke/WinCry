using System.Diagnostics;
using System.IO;

namespace WinCry.Models
{
    class TweaksCollection
    {
        public static class VCTweak
        {
            public static void InstallDependent()
            {
                Helpers.ExtractEmbedFile(Properties.Resources.vcredist2010_x86, "vcredist2010_x86.exe");
                Helpers.RunByCMD($"start /wait {Path.GetTempPath()}vcredist2010_x86.exe /q /norestart");
            }
        }

        public static class MSStore
        {
            public static void Install()
            {
                string _zipExtractionDirectory = Path.GetTempPath() + @"WindowsStore\";
                string _zipPath = Path.GetTempPath() + "WindowsStore.zip";

                Helpers.UnzipFromFile(_zipPath, _zipExtractionDirectory);
                Helpers.RunByCMD($@"{_zipExtractionDirectory}install.bat");
            }

            public static void Uninstall()
            {
                ProcessStartInfo _info = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    UseShellExecute = false,
                    Arguments = "Get-AppxPackage *windowsstore*|Remove-AppxPackage",
                    CreateNoWindow = true
                };

                Process.Start(_info);
            }
        }
    }
}