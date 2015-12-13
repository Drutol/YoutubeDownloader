﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

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

        public ObservableCollection<VideoItem> vidListItems = new ObservableCollection<VideoItem>();
        public ObservableCollection<HistoryItem> historyListItems = new ObservableCollection<HistoryItem>();

        public DownloadPage()
        {
            InitializeComponent();
        }

        private void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            BeginWork();
        }

        public bool AddVideoItem(VideoItem item)
        {        
            foreach (var vid in vidListItems)
                if (vid.Equals(item))
                    return false;
            vidListItems.Add(item);
            VideoList.ItemsSource = vidListItems;
            return true;
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
                    break;
                case IdType.TYPE_PLAYLIST:
                    bool setAlbumTag = Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_ALBUM_PLAYLIST_NAME);
                    var info = await YTDownload.GetPlaylistDetails(contentID);
                    HistoryManager.AddNewEntry(new HistoryEntry(info.Item3, info.Item1, info.Item2, contentID));
                    string playlistName = setAlbumTag ? info.Item1 : "";
                    Tuple<List<string>, string, string> playlistItems = new Tuple<List<string>, string, string>(new List<string>(),"",""); //Can I do this better?
                    await Task.Run(
                        async () =>
                        {
                            playlistItems = await YTDownload.GetVideosInPlaylist(contentID, token);
                        });               
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
            {
                SelectionMenu.Visibility = Visibility.Visible;
                Utils.GetMainPageInstance().AppBarOpened();
            }
            else
            {
                SelectionMenu.Visibility = Visibility.Collapsed;
                Utils.GetMainPageInstance().AppBarClosed();
            }
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
                Debug.WriteLine(exc.Message);
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

        #region Details

        private string _prevCoverPath;
        private VideoItem _currentlyEditedItem;
        public async void DetailsPopulate(VideoItem caller)
        {
            if (_currentlyEditedItem == null)
            {
                DetailsAnimationShow.Begin();
                VideoDetails.Visibility = Visibility.Visible;
            }
            _currentlyEditedItem = caller;
            //Clear
            if (_currentlyEditedItem.AlbumCoverPath == null)
            {
                TagAlbumCover.Source = null;
                IconBrowseCover.Visibility = Visibility.Visible;
            }
            else if (_prevCoverPath == null || _prevCoverPath != _currentlyEditedItem.AlbumCoverPath)
            {
                var thumb = await StorageFile.GetFileFromPathAsync(_currentlyEditedItem.AlbumCoverPath);
                using (var fileStream = await thumb.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(fileStream);
                    TagAlbumCover.Source = bitmapImage;
                }
                IconBrowseCover.Visibility = Visibility.Collapsed;
                _prevCoverPath = _currentlyEditedItem.AlbumCoverPath;
            }
            DetailsTitleSuggestsBox.Items.Clear();
            DetailsArtistSuggestsBox.Items.Clear();
            //Populate data
            if (!caller.suggestions.IsEmpty())
            {
                //title
                DetailsTitleSuggestsBox.Text = caller.tagTitle;
                foreach (var item in caller.suggestions.titles)
                {
                    DetailsTitleSuggestsBox.Items.Add(item);
                }
                //artist
                DetailsArtistSuggestsBox.Text = caller.tagArtist;
                foreach (var item in caller.suggestions.authors)
                {
                    DetailsArtistSuggestsBox.Items.Add(item);
                }
            }
            //Misc
            DetailsAlbum.Text = caller.tagAlbum;
            DetailsTrackNumber.Text = "0";
        }
        private void DetailsSuggestClicked(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox box = sender as AutoSuggestBox;
            box.IsSuggestionListOpen = true;
        }
        private void DetailsClose(object sender, RoutedEventArgs e)
        {
            _currentlyEditedItem = null;
            DetailsAnimationHide.Begin();
            DetailsAnimationHide.Completed += (o, o1) => { VideoDetails.Visibility = Visibility.Collapsed; };
        }
        private void DetailsTitleTextChange(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggestBox box = sender;
            _currentlyEditedItem.tagTitle = box.Text;
        }

        private void DetailsTitleSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            _currentlyEditedItem.tagTitle = args.SelectedItem.ToString();
        }

        private void DetailsArtistTextChange(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            AutoSuggestBox box = sender;
            _currentlyEditedItem.tagArtist = box.Text;
        }

        private void DetailsArtistSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            _currentlyEditedItem.tagArtist = args.SelectedItem.ToString();
        }

        private void DetailsAlbumTextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox box = sender as TextBox;
            _currentlyEditedItem.tagAlbum = box.Text;
        }

        private async void DetailsSelectAlbumCover(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");
            var result = await picker.PickSingleFileAsync();
            if(result == null) return;
            StorageApplicationPermissions.MostRecentlyUsedList.Add(result);
            using (var fileStream = await result.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                TagAlbumCover.Source = bitmapImage;
            }
            IconBrowseCover.Visibility = Visibility.Collapsed;
            _currentlyEditedItem.AlbumCoverPath = result.Path;
        }
        #endregion

    }
}
