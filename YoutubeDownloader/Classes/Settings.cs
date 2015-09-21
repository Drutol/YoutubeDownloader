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
        //private static IEnumerable<string> SettingsKeys = new List<string>() { "SettingAutoDownload" };

        public static async void Init()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var frame = (Frame)Window.Current.Content;
                var page = (MainPage)frame.Content;
                if (ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] == null)
                    ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"] = "True";
                page.SetAutoDownloadSetting((string)ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"]);
            });

        }

        public static void ChangeSetting(string key,string value)
        {
            ApplicationData.Current.LocalSettings.Values[key] = value;
        }
    }
}
