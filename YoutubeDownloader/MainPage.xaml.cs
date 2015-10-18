﻿using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Storage;
using YoutubeExtractor;
using Windows.UI.Popups;
using System.Linq;
using System.IO;
using System;
using Windows.Media.Transcoding;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
using Windows.Storage.AccessCache;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls.Primitives;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace YoutubeDownloader
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Settings.Init();
        }


        public ObservableCollection<VideoItem> vidListItems;
        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            VideoItem vidItem;
            SpinnerLoadingPlaylist.Visibility = Visibility.Visible;
            vidListItems = new ObservableCollection<VideoItem>();
            switch (YTDownload.IsIdValid(BoxID.Text))
            {
                case IdType.TYPE_VIDEO:
                    vidItem = new VideoItem(BoxID.Text);
                    vidListItems.Add(vidItem);
                    break;
                case IdType.TYPE_PLAYLIST:
                    List<string> videos = await YTDownload.GetVideosInPlaylist(BoxID.Text);
                    foreach (var video in videos)
                    {
                        vidItem = new VideoItem(video);
                        vidListItems.Add(vidItem);
                    }
                    break;
                case IdType.INVALID:
                    MessageDialog dialog = new MessageDialog("YouTube video got injured in an horrible accident, sorry it's ID is INVALID","God F&#$%*@ D%&#");
                    await dialog.ShowAsync();
                    break;
                default:
                    throw new Exception("Invalid enumm - id valid");
            }
            SpinnerLoadingPlaylist.Visibility = Visibility.Collapsed;
            VideoList.ItemsSource = vidListItems;
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

        public void SetOutputQuality(int iQuality)
        {
            ComboOutputQuality.SelectedIndex = iQuality;
        }

        public void SetOutputFolderName(string name)
        {
            SettingOutputFolder.Text = name == "" ? "Music library" : name;
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


        #endregion

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
                picker.FileTypeFilter.Add(".fake"); //Avoid random files from displaying

                var folder = await picker.PickSingleFolderAsync();
                if (folder == null) return;

                StorageApplicationPermissions.FutureAccessList.AddOrReplace("outFolder", folder);
                
                Settings.SetOutputFolderName(folder.Name);
                SettingOutputFolder.Text = folder.Name;         
            }
            catch (TaskCanceledException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void OpenOutputFolder(object sender, RoutedEventArgs e)
        {

        }

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            HamburgerButton_Click(null,null);
            GridSettings.Visibility = MainMenu.IsPaneOpen ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnSettingsInner_Click(object sender, RoutedEventArgs e)
        {
            GridSettings.Visibility = GridSettings.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        #region Helpers
        private void HideAllPaneGrids()
        {
            GridSettings.Visibility = Visibility.Collapsed;
        }

        private void HideAllPaneGrids(Grid exception)
        {
            HideAllPaneGrids();
            exception.Visibility = Visibility.Visible;
        }


        #endregion

        private void EnableMultiSelection(object sender, RoutedEventArgs e)
        {
            var btn = (ToggleButton)sender;
            VideoList.SelectionMode = (bool)btn.IsChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
        }

        private void VideoItemSelected(object sender, SelectionChangedEventArgs e)
        {
            if (VideoList.SelectedItems.Count > 0)
                SelectionMenu.Visibility = Visibility.Visible;
            else
                SelectionMenu.Visibility = Visibility.Collapsed;
        }

        private void MassEditTags(object sender, RoutedEventArgs e)
        {
            foreach(VideoItem item in VideoList.SelectedItems)
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
    }
}
