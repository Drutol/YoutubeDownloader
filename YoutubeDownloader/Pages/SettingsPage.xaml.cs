using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace YoutubeDownloader.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private bool initDone;
        public SettingsPage()
        {
            InitializeComponent();
            //Current Values
            SetAutoDownloadSetting((string)ApplicationData.Current.LocalSettings.Values["SettingAutoDownload"]);
            SetSetAlbumAsPlaylistNameSetting((string)ApplicationData.Current.LocalSettings.Values["SettingSetAlbumAsPlaylistName"]);
            SetRenameSetting((string)ApplicationData.Current.LocalSettings.Values["SettingRenameFile"]);
            SetOutputFormat((int)ApplicationData.Current.LocalSettings.Values["outFormat"]);
            SetOutputFolderName((string)ApplicationData.Current.LocalSettings.Values["outFolder"]);
            SetOutputQuality((int)ApplicationData.Current.LocalSettings.Values["outQuality"]);
            SetMaxPararellDownloads((int)ApplicationData.Current.LocalSettings.Values["SettingMaxPararellDownloads"]);
            SetMaxPararellConv((int)ApplicationData.Current.LocalSettings.Values["SettingMaxPararellConv"]);
            SetResultsPerPage((int)ApplicationData.Current.LocalSettings.Values["SettingResultsPerPage"]);
            SetParseTagsSetting((string)ApplicationData.Current.LocalSettings.Values["SettingAttemptToParseTags"]);
            SetAutoCover((string)ApplicationData.Current.LocalSettings.Values["SettingSetDefaultCover"]);
            initDone = true;
        }

        #region Setting Setters

        private void SetSetAlbumAsPlaylistNameSetting(string val)
        {
            SettingSetAlbumAsPlaylistName.IsOn = val == "True";
        }

        private void SetAutoDownloadSetting(string val)
        {
            SettingAutoDownload.IsOn = val == "True";
        }

        private void SetParseTagsSetting(string val)
        {
            SettingAttemptToParseTags.IsOn = val == "True";
        }

        private void SetOutputFormat(int iFormat)
        {
            ComboOutputFormat.SelectedIndex = iFormat;
        }

        private void SetRenameSetting(string val)
        {
            SettingRenameFile.IsOn = val == "True";
        }

        private void SetOutputQuality(int iQuality)
        {
            ComboOutputQuality.SelectedIndex = iQuality;
        }

        private void SetOutputFolderName(string name)
        {
            SettingOutputFolder.Text = name == "" ? "Music library" : name;
        }

        private void SetMaxPararellDownloads(int value)
        {
            SettingMaxPararellDownloads.Value = value;
        }

        private void SetMaxPararellConv(int value)
        {
            SettingMaxPararellConv.Value = value;
        }

        private void SetResultsPerPage(int value)
        {
            SettingResultsPerPage.Value = value;
        }

        private void SetAutoCover(string val)
        {
            SettingSetDefaultCover.IsOn = val == "True";
        }
        #endregion

        #region Settings Controls


        private void ChangeSetting(object sender, RoutedEventArgs e)
        {
            if (!initDone)
                return;
            ToggleSwitch toggle = (ToggleSwitch)sender;
            Settings.ChangeSetting(toggle.Name, Convert.ToString(toggle.IsOn));
        }

        private void ChangeSliderSetting(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!initDone)
                return;
            Slider slider = (Slider)sender;
            Settings.ChangeSetting(slider.Name, (int)slider.Value);
            if (!slider.Name.Contains("Conv"))
                QueueManager.Instance.MaxPararellDownloadChanged((int)slider.Value);
            else
                QueueManager.Instance.MaxPararellConvChanged((int)slider.Value);
        }

        private void ChangePrefferedFormat(object sender, object e)
        {
            if (!initDone)
                return;
            ComboBox cmb = (ComboBox)sender;

            Settings.ChangeFormat((Settings.PossibleOutputFormats)cmb.SelectedIndex);
            //foreach (var item in VidListItems)
            //{
            //    item.outputFormat = (Settings.PossibleOutputFormats)cmb.SelectedIndex;
            //}
        }

        private void ChangePrefferedQuality(object sender, SelectionChangedEventArgs e)
        {
            if (!initDone)
                return;
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
                SettingOutputFolder.Text = folder.Name;
            }
            catch (TaskCanceledException ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        #endregion
    }
}


