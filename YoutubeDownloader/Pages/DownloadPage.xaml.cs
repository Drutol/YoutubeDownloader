using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
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
    public sealed partial class DownloadPage : Page
    {
        private string nextPageToken = "";
        private string prevPageToken = "";

        public ObservableCollection<VideoItem> vidListItems;
        public ObservableCollection<HistoryItem> historyListItems = new ObservableCollection<HistoryItem>();

        bool ShouldPopulateHistory = true;

        public DownloadPage()
        {
            this.InitializeComponent();
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            BeginWork();
        }

        /// <summary>
        /// This is where it all begun...
        /// </summary>
        public async void BeginWork(string url = "", string token = "", bool reset = true)
        {

            EmptyNotice.Visibility = Visibility.Collapsed;
            SpinnerLoadingPlaylist.Visibility = Visibility.Visible;
            if (vidListItems == null || reset)
                vidListItems = new ObservableCollection<VideoItem>(); //prevent from adding same ids
            string contentID = url == "" ? BoxID.Text : url; // this is going to be probably overwritten
            VideoList.ItemsSource = vidListItems;
            var inputData = await YTDownload.IsIdValid(contentID);
            contentID = inputData.Item2;
            switch (inputData.Item1)
            {
                case IdType.TYPE_VIDEO:
                    ResetPageTokens();
                    vidListItems.Add(new VideoItem(contentID, "", true));
                    ShouldPopulateHistory = true;
                    break;
                case IdType.TYPE_PLAYLIST:
                    bool setAlbumTag = Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_ALBUM_PLAYLIST_NAME);
                    var info = await YTDownload.GetPlaylistDetails(contentID);
                    HistoryManager.AddNewEntry(new HistoryEntry(info.Item3, info.Item1, info.Item2, contentID));
                    string playlistName = setAlbumTag ? info.Item1 : "";
                    var playlistItems = await YTDownload.GetVideosInPlaylist(contentID, token);
                    List<string> allVideos = new List<string>(playlistItems.Item1);
                    string nextToken = playlistItems.Item2;
                    if (Settings.GetValueForSetting(Settings.PossibleValueSettings.SETTING_PER_PAGE) == 31)
                    {
                        while (true)
                        {
                            if (nextToken == null) break;
                            var morePlaylistItems = await YTDownload.GetVideosInPlaylist(contentID, nextToken);
                            allVideos.AddRange(morePlaylistItems.Item1);
                            nextToken = morePlaylistItems.Item2;
                        }
                        ResetPageTokens();
                    }
                    else
                        ProcessPageTokens(playlistItems.Item3, playlistItems.Item2); //next,prev

                    foreach (var video in allVideos)
                    {
                        vidListItems.Add(new VideoItem(video, playlistName));
                    }
                    ShouldPopulateHistory = true;
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

        #region Pages
        private void ProcessPageTokens(string tp, string tn) // previous and next
        {
            if (tp != null || tn != null)
            {
                Pages.Visibility = Visibility.Visible;
                BoxID.SetValue(Grid.ColumnSpanProperty, 1);
            }
            else
            {
                Pages.Visibility = Visibility.Collapsed;
                BoxID.SetValue(Grid.ColumnSpanProperty, 2);
            }

            if (tp != null)
            {
                prevPageToken = tp;
                PrevPage.IsEnabled = true;
                SymbolIcon ico = PrevPage.Content as SymbolIcon;
                ico.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as Brush;
            }
            else
            {
                PrevPage.IsEnabled = false;
                SymbolIcon ico = PrevPage.Content as SymbolIcon;
                ico.Foreground = new SolidColorBrush(Colors.Black);
            }

            if (tn != null)
            {
                nextPageToken = tn;
                NextPage.IsEnabled = true;
                SymbolIcon ico = NextPage.Content as SymbolIcon;
                ico.Foreground = Application.Current.Resources["SystemControlBackgroundAccentBrush"] as Brush;
            }
            else
            {
                NextPage.IsEnabled = false;
                SymbolIcon ico = NextPage.Content as SymbolIcon;
                ico.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            BeginWork("", nextPageToken);
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            BeginWork("", prevPageToken);
        }

        private void ResetPageTokens()
        {
            nextPageToken = "";
            prevPageToken = "";
            Pages.Visibility = Visibility.Collapsed;
            BoxID.SetValue(Grid.ColumnSpanProperty, 2);
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

    }
}
