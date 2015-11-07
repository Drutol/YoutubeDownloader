using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YoutubeDownloader
{
    public static class Settings
    {
        public enum PossibleSettingsBool
        {
            SETTING_AUTO_DL,
            SETTING_ALBUM_PLAYLIST_NAME,
            SETTING_AUTO_RENAME,
        }

        public enum PossibleOutputFormats
        {
            FORMAT_MP3 = 1,
            FORMAT_MP4 = 2,
        }

        public enum PossibleValueSettings
        {
            SETTINGS_PARARELL_DL,
        }
        //private static IEnumerable<string> SettingsKeys = new List<string>() { "SettingAutoDownload" };

        public static async void Init()
        {
            CheckDefaultSettings();
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = (Frame)Window.Current.Content;
                var page = (MainPage)frame.Content;

                // TODO : Move this to main page class.
                page.SetAutoDownloadSetting((string)ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"]);
                page.SetSetAlbumAsPlaylistNameSetting((string)ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"]);
                page.SetRenameSetting((string)ApplicationData.Current.LocalSettings.Values["SettingRenameFile"]);
                page.SetOutputFormat((int)ApplicationData.Current.LocalSettings.Values["outFormat"]);
                page.SetOutputFolderName((string)ApplicationData.Current.LocalSettings.Values["outFolder"]);
                page.SetOutputQuality((int)ApplicationData.Current.LocalSettings.Values["outQuality"]);
                page.SetMaxPararellDownloads((int)ApplicationData.Current.LocalSettings.Values["SettingMaxPararellDownloads"]);
            });

        }
        /// <summary>
        /// Function sets default values for each setting if they do not exist.
        /// </summary>
        public static void CheckDefaultSettings()
        {
            //bools
            if (ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] = "True";

            if (ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"] = "True";

            if (ApplicationData.Current.LocalSettings.Values["SettingRenameFile"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingRenameFile"] = "True";

            //format
            if (ApplicationData.Current.LocalSettings.Values["outFormat"] == null)
                ApplicationData.Current.LocalSettings.Values["outFormat"] = (int)PossibleOutputFormats.FORMAT_MP3;

            //quality
            if (ApplicationData.Current.LocalSettings.Values["outQuality"] == null)
                ApplicationData.Current.LocalSettings.Values["outQuality"] = (int)AudioEncodingQuality.High;

            //output
            if (ApplicationData.Current.LocalSettings.Values["outFolder"] == null)
                ApplicationData.Current.LocalSettings.Values["outFolder"] = ""; //empty as for default Music folder.

            //slider
            if (ApplicationData.Current.LocalSettings.Values["SettingMaxPararellDownloads"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingMaxPararellDownloads"] = 3; //empty as for default Music folder.

        }

        public static bool GetBoolSettingValueForKey(PossibleSettingsBool setting)
        {
            string value;
            switch (setting)
            {
                case PossibleSettingsBool.SETTING_AUTO_DL:
                    value = (string)ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"];
                    break;
                case PossibleSettingsBool.SETTING_ALBUM_PLAYLIST_NAME:
                    value = (string)ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"];
                    break;
                case PossibleSettingsBool.SETTING_AUTO_RENAME:
                    value = (string)ApplicationData.Current.LocalSettings.Values["SettingRenameFile"];
                    break;
                default:
                    throw new Exception("Ivalid enum");
            }

            return value == "True" ? true : false;
        }

        public static int GetValueForSetting(PossibleValueSettings setting)
        {
            switch (setting)
            {
                case PossibleValueSettings.SETTINGS_PARARELL_DL:
                    return (int)ApplicationData.Current.LocalSettings.Values["SettingMaxPararellDownloads"];
                default:
                    break;
            }
            throw new Exception("Wrong Enum");
        }


        public static PossibleOutputFormats GetPrefferedOutputFormat()
        {
            return (PossibleOutputFormats)ApplicationData.Current.LocalSettings.Values["outFormat"];
        }

        public static AudioEncodingQuality GetPrefferedOutputQuality()
        {
            return (AudioEncodingQuality)ApplicationData.Current.LocalSettings.Values["outQuality"];
        }

        public static void ChangeSetting(string key,string value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }
        public static void ChangeSetting(string key, int value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }

        public static void ChangeFormat(PossibleOutputFormats format)
        {
            ApplicationData.Current.LocalSettings.Values["outFormat"] = (int)format;
        }

        public static void ChangeQuality(AudioEncodingQuality quality)
        {
            ApplicationData.Current.LocalSettings.Values["outQuality"] = (int)quality;
        }

        public static void SetOutputFolderName(string name)
        {
            ApplicationData.Current.LocalSettings.Values["outFolder"] = name;
        }

        public static MediaEncodingProfile GetPrefferedEncodingProfile()
        {
            switch (GetPrefferedOutputFormat())
            {
                case PossibleOutputFormats.FORMAT_MP3:
                    return MediaEncodingProfile.CreateMp3(GetPrefferedOutputQuality());
                case PossibleOutputFormats.FORMAT_MP4:
                    throw new Exception("Source is MP4");
                default:
                    break;
            }

            throw new Exception("Unknown format");
            
        }
        /// <summary>
        /// Returns folder set by user , returns Music Library by default.
        /// </summary>
        /// <returns>
        /// Output folder.
        /// </returns>
        public static async Task<IStorageFolder> GetOutputFolder()
        {
            if((string)ApplicationData.Current.LocalSettings.Values["outFolder"] == "")
            {
                return KnownFolders.MusicLibrary;
            }
            return await StorageApplicationPermissions.FutureAccessList.GetFolderAsync("outFolder",AccessCacheOptions.None);
        }
    }
}
