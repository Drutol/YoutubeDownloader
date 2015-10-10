using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YoutubeDownloader
{
    static class Settings
    {
        public enum PossibleSettingsBool
        {
            SETTING_AUTO_DL,
            SETTING_ALBUM_PLAYLIST_NAME,
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
            });

        }

        public static void CheckDefaultSettings()
        {
            //bools
            if (ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] = "True";

            if (ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"] = "True";
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
                default:
                    throw new Exception("Ivalid enum");
            }

            return value == "True" ? true : false;
        }


        public static void ChangeSetting(string key,string value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }
    }
}
