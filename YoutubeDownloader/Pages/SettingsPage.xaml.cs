using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.MediaProperties;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace YoutubeDownloader.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        #region Setting Setters
        public void SetSetAlbumAsPlaylistNameSetting(string val)
        {
            SettingSetAlbumAsPlaylistName.IsOn = val == "True" ? true : false;
        }

        public void SetAutoDownloadSetting(string val)
        {
            SettingAutoDownload.IsOn = val == "True" ? true : false;
        }
        public void SetParseTagsSetting(string val)
        {
            SettingAttemptToParseTags.IsOn = val == "True" ? true : false;
        }
        public void SetOutputFormat(int iFormat)
        {
            ComboOutputFormat.SelectedIndex = iFormat;
        }

        public void SetRenameSetting(string val)
        {
            SettingRenameFile.IsOn = val == "True" ? true : false;
        }

        public void SetOutputQuality(int iQuality)
        {
            ComboOutputQuality.SelectedIndex = iQuality;
        }

        public void SetOutputFolderName(string name)
        {
            SettingOutputFolder.Text = name == "" ? "Music library" : name;
        }
        public void SetMaxPararellDownloads(int value)
        {
            SettingMaxPararellDownloads.Value = value;
        }
        internal void SetMaxPararellConv(int value)
        {
            SettingMaxPararellConv.Value = value;
        }
        internal void SetResultsPerPage(int value)
        {
            SettingResultsPerPage.Value = value;
        }
        #endregion

        #region Settings Controls


        private void ChangeSetting(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;
            Settings.ChangeSetting(toggle.Name, Convert.ToString(toggle.IsOn));
        }

        private void ChangeSliderSetting(object sender, RangeBaseValueChangedEventArgs e)
        {
            Slider slider = (Slider)sender;
            Settings.ChangeSetting(slider.Name, (int)slider.Value);
            if (!slider.Name.Contains("Conv"))
                QueueManager.Instance.MaxPararellDownloadChanged((int)slider.Value);
            else
                QueueManager.Instance.MaxPararellConvChanged((int)slider.Value);
        }

        private void ChangePrefferedFormat(object sender, object e)
        {
            ComboBox cmb = (ComboBox)sender;

            Settings.ChangeFormat((Settings.PossibleOutputFormats)cmb.SelectedIndex);
            //foreach (var item in vidListItems)
            //{
            //    item.outputFormat = (Settings.PossibleOutputFormats)cmb.SelectedIndex;
            //}
        }

        private void ChangePrefferedQuality(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;

            Settings.ChangeQuality((AudioEncodingQuality)cmb.SelectedIndex);
        }

        private async void SelectOutputFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FolderPicker { ViewMode = PickerViewMode.List };
                picker.FileTypeFilter.Add(".fake"); //Avoid random files from displaying , is there .fake extension?

                var folder = await picker.PickSingleFolderAsync();
                if (folder == null) return; //No folder no fun

                StorageApplicationPermissions.FutureAccessList.AddOrReplace("outFolder", folder);

                Settings.SetOutputFolderName(folder.Name);
                //SettingOutputFolder.Text = folder.Name;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}


