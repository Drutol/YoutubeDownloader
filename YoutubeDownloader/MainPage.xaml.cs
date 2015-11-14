using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using System;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.System;

namespace YoutubeDownloader
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Settings.Init();          
        }


        public ObservableCollection<VideoItem> vidListItems;
        public ObservableCollection<HistoryItem> historyListItems = new ObservableCollection<HistoryItem>();

        bool ShouldPopulateHistory = true;

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            BeginWork();
        }
        /// <summary>
        /// This is where it all begun...
        /// </summary>
        public async void BeginWork(string url = "")
        {
            VideoItem vidItem;
            EmptyNotice.Visibility = Visibility.Collapsed;
            SpinnerLoadingPlaylist.Visibility = Visibility.Visible;
            vidListItems = new ObservableCollection<VideoItem>(); //prevent from adding same ids
            string contentID = url == "" ? BoxID.Text : url; // this is going to be probably overwritten
            VideoList.ItemsSource = vidListItems;
            var inputData = await YTDownload.IsIdValid(contentID);
            contentID = inputData.Item2;
            switch (inputData.Item1)
            {
                case IdType.TYPE_VIDEO:
                    vidItem = new VideoItem(contentID, "", true);
                    vidListItems.Add(vidItem);
                    break;
                case IdType.TYPE_PLAYLIST:

                    bool setAlbumTag = Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_ALBUM_PLAYLIST_NAME);
                    var info = await YTDownload.GetPlaylistDetails(contentID);
                    HistoryManager.AddNewEntry(new HistoryEntry(info.Item3, info.Item1, info.Item2, contentID));
                    string playlistName = setAlbumTag ? info.Item1 : "";
                    List<string> videos = await YTDownload.GetVideosInPlaylist(contentID);
                    foreach (var video in videos)
                    {
                        vidItem = new VideoItem(video, playlistName);
                        vidListItems.Add(vidItem);
                    }
                    break;
                case IdType.INVALID:
                    MessageDialog dialog = new MessageDialog("Couldn't extract playlist or video id from input string.");
                    await dialog.ShowAsync();
                    EmptyNotice.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new Exception("Invalid enumm - id valid");
            }
            SpinnerLoadingPlaylist.Visibility = Visibility.Collapsed;
            
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
        #endregion

        #region Settings Controls
        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.IsPaneOpen = !MainMenu.IsPaneOpen;
            if (!MainMenu.IsPaneOpen)
                HideAllPaneGrids();
        }

        private void ChangeSetting(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;
            Settings.ChangeSetting(toggle.Name, Convert.ToString(toggle.IsOn));
        }

        private void ChangeSliderSetting(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (MainMenu.IsPaneOpen) //Cause it was triggering on app load. Seems like a reasonable workaroud ^^
            {
                Slider slider = (Slider)sender;
                Settings.ChangeSetting(slider.Name, (int)slider.Value);
                if(!slider.Name.Contains("Conv"))
                    QueueManager.Instance.MaxPararellDownloadChanged((int)slider.Value);
                else
                    QueueManager.Instance.MaxPararellConvChanged((int)slider.Value);
            }
        }

        private void ChangePrefferedFormat(object sender, object e)
        {
            ComboBox cmb = (ComboBox)sender;

            Settings.ChangeFormat((Settings.PossibleOutputFormats)cmb.SelectedIndex);
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
                SettingOutputFolder.Text = folder.Name;
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


        #endregion

        #region PaneGrids
        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (AreAllGridPanesClosed() || !MainMenu.IsPaneOpen)
            {
                HamburgerButton_Click(null, null); //Closes main pane
                GridSettings.Visibility = MainMenu.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                GridSettings.Visibility = GridSettings.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnSettingsInner_Click(object sender, RoutedEventArgs e)
        {
            GridSettings.Visibility = GridSettings.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (AreAllGridPanesClosed() || !MainMenu.IsPaneOpen)
            {
                HamburgerButton_Click(null, null);
                GridHistory.Visibility = MainMenu.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                GridHistory.Visibility = GridHistory.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;

            PopulateHistory();
        }

        private void BtnHistoryInner_Click(object sender, RoutedEventArgs e)
        {
            GridHistory.Visibility = GridHistory.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
            PopulateHistory();
        }

        private bool AreAllGridPanesClosed()
        {
            return !Utils.VisibilityConverter(GridHistory.Visibility) && !Utils.VisibilityConverter(GridSettings.Visibility);
        }
        #endregion

        #region Helpers
        private void HideAllPaneGrids()
        {
            GridSettings.Visibility = Visibility.Collapsed;
            GridHistory.Visibility = Visibility.Collapsed;
        }

        private void HideAllPaneGrids(Grid exception)
        {
            HideAllPaneGrids();
            exception.Visibility = Visibility.Visible;
        }
        private void HideAllPaneGrids(SplitView sender, object args)
        {
            HideAllPaneGrids();
        }
        #endregion

        #region SelectionManip
        private void SelectionInvert(object sender, RoutedEventArgs e)
        {
            List<string> disabledIds = new List<string>();
            foreach (var item in VideoList.SelectedItems)
            {
                VideoItem vid = (VideoItem)item;
                disabledIds.Add(vid.id);
            }
            VideoList.SelectedItems.Clear();
            foreach (var item in VideoList.Items)
            {
                VideoItem vid = (VideoItem)item;
                if (disabledIds.Find(video => video == vid.id) == null)
                    VideoList.SelectedItems.Add(item);
            }
        }

        private void SelectionAll(object sender, RoutedEventArgs e)
        {
            VideoList.SelectAll();
        }
        #endregion

        #region SelectionActions
        private void VideoItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (VideoList.SelectedItems.Count > 0)
                SelectionMenu.Visibility = Visibility.Visible;
            else
                SelectionMenu.Visibility = Visibility.Collapsed;
        }

        private void MassEditTags(object sender, RoutedEventArgs e)
        {
            foreach (VideoItem item in VideoList.SelectedItems)
            {
                item.tagAlbum = MassEditTagAlbum.Text;
                item.tagArtist = MassEditTagArtist.Text;
            }
            MassEditFlyout.Hide();
        }

        private void DownloadThumbnails(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<string, string> urls = new Dictionary<string, string>();
                foreach (VideoItem item in VideoList.SelectedItems)
                {
                    urls.Add(item.title, item.thumbUrl);
                }
                YTDownload.DownloadThumbnails(urls);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
            }
        }

        private void MassEditDownload(object sender, RoutedEventArgs e)
        {
            foreach (VideoItem item in VideoList.SelectedItems)
            {
                item.QueueDownload();
            }
        }

        private void SelectionRemove(object sender, RoutedEventArgs e)
        {
            foreach (VideoItem item in VideoList.SelectedItems)
            {
                vidListItems.Remove(item);
            }
        }
        #endregion

        #region History
        private async void PopulateHistory()
        {
            if (!ShouldPopulateHistory)
                return;
            HistoryWorking.Visibility = Visibility.Visible;
            historyListItems.Clear();

            var history = new List<HistoryEntry>();
            await Task.Run(() => { history = HistoryManager.GetHistoryEntries(); });
            foreach (var item in history)
            {
                historyListItems.Add(new HistoryItem(item));
            }
            HistoryWorking.Visibility = Visibility.Collapsed;
            HistoryList.ItemsSource = historyListItems;
            ShouldPopulateHistory = false;
        }

        #endregion

        private async void OpenOututFolder(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderAsync(await Settings.GetOutputFolder());
        }
    }
}
