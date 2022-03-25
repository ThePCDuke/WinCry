using System.IO;
namespace WinCry.Models
{
    class StringConsts
    {
        public const string URLChannel = @"https://www.youtube.com/c/ThePeaceDuke";
        public const string URLDonation = @"https://www.donationalerts.com/r/thepeaceduke";
        public const string URLCommunity = @"https://vk.com/thepeaceduke";
        public const string URLCommunityDiscussion = @"https://vk.com/topic-206972023_48260256";
        public const string URLVC = @"https://getfile.dokpub.com/yandex/get/https://disk.yandex.ru/d/EkhxSJWgkcvovg/WinCry/VC.zip";
        public const string URLMSStore = @"https://getfile.dokpub.com/yandex/get/https://disk.yandex.ru/d/EkhxSJWgkcvovg/WinCry/WindowsStore.zip";
        public const string URLMSStoreLTSB = @"https://getfile.dokpub.com/yandex/get/https://disk.yandex.ru/d/EkhxSJWgkcvovg/WinCry/WindowsStoreLTSB.zip";

        public const string MD5MSStore = "3c94bbd1a8e474106492c7273b0b16a2";
        public const string MD5MSStoreLSTB = "cabbb2d42735a87551fa2ac996078e74";
        public const string MD5VC = "bf5b30dddf9f65bc7442a38069dc8730";

        public const string All = "Все";

        public const string ExpertPreset = "WinCry - Экспертный";

        public const string SettingsPresetsFolder = @"Presets\Settings\";
        public const string ServicesPresetsFolder = @"Presets\Services\";
        public const string TweaksPresetsFolder = @"Presets\Tweaks\";

        public static string ServicesBackupFolder { get; } = Path.Combine(System.Environment.CurrentDirectory, "Backup", "Services");
        public static string ServicesRestorationPatchFolder { get; } = Path.Combine(Path.GetTempPath(), @"ServicesRestore\");
        public static string WindowsStoreZipFilePath { get; } = Path.GetTempPath() + "WindowsStore.zip";
        public static string VCZipFilePath { get; } = Path.GetTempPath() + "VC.zip";
    }
}