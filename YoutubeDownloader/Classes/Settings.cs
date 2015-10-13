using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Media.MediaProperties;
using Windows.Storage;
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
        }

        public enum PossibleOutputFormats
        {
            FORMAT_MP3 = 1,
            FORMAT_MP4 = 2,
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
                page.SetOutputFormat((int)ApplicationData.Current.LocalSettings.Values["outFormat"]);

            });

        }

        public static void CheckDefaultSettings()
        {
            //bools
            if (ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] = "True";

            if (ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"] == null)
                ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"] = "True";

            //format
            if (ApplicationData.Current.LocalSettings.Values["outFormat"] == null)
                ApplicationData.Current.LocalSettings.Values["outFormat"] = (int)PossibleOutputFormats.FORMAT_MP3;

            //quality
            if (ApplicationData.Current.LocalSettings.Values["outQuality"] == null)
                ApplicationData.Current.LocalSettings.Values["outQuality"] = (int)AudioEncodingQuality.High;

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

        public static void ChangeFormat(PossibleOutputFormats format)
        {
            ApplicationData.Current.LocalSettings.Values["outFormat"] = (int)format;
        }

        public static void ChangeQuality(AudioEncodingQuality quality)
        {
            ApplicationData.Current.LocalSettings.Values["outQuality"] = (int)quality;
        }
    }
}
