using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.UI;
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

        private ObservableCollection<VideoItem> VidListItems = new ObservableCollection<VideoItem>();

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
            foreach (var vid in VidListItems)
                if (vid.Equals(item))
                    return false;
            VidListItems.Add(item);
            VideoList.ItemsSource = VidListItems;
            return true;
        }

        /// <summary>
        /// This is where it all begun...
        /// </summary>
        private async void BeginWork(string url = "", string token = "", bool reset = true)
        {
            EmptyNotice.Visibility = Visibility.Collapsed;
            SpinnerLoadingPlaylist.Visibility = Visibility.Visible;
            if (VidListItems == null || reset)
                VidListItems = new ObservableCollection<VideoItem>(); //prevent from adding same ids
            string contentID = url == "" ? BoxID.Text : url; // this is going to be probably overwritten
            VideoList.ItemsSource = VidListItems;
            var inputData = await YTDownload.IsIdValid(contentID);
            contentID = inputData.Item2;
            switch (inputData.Item1)
            {
                case IdType.TYPE_VIDEO:
                    ResetPageTokens();
                    VidListItems.Add(new VideoItem(contentID, "", true));
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
                        VidListItems.Add(new VideoItem(video, playlistName));
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
            List<string> disabledIds = (from VideoItem vid in VideoList.SelectedItems select vid.id).ToList();
            VideoList.SelectedItems.Clear();
            foreach (var item in from item in VideoList.Items let vid = (VideoItem)item where disabledIds.Find(video => video == vid.id) == null select item)
            {
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
                AppBarSetCover.Visibility = VideoList.SelectedItems.Count == 1 ? Visibility.Collapsed : Visibility.Visible;
                SelectionMenu.Visibility = Visibility.Visible;
                Utils.GetMainPageInstance().AppBarOpened();
            }
            else
            {
                SelectionMenu.Visibility = Visibility.Collapsed;
                Utils.GetMainPageInstance().AppBarClosed();
            }
        }

        private void SelectionSetThisCover(object sender, RoutedEventArgs e)
        {
            VideoItem currItem = VideoList.SelectedItems.First() as VideoItem;
            foreach (var item in VidListItems)
            {
                item.AlbumCoverPath = currItem?.AlbumCoverPath;
            }
        }

        private async void SelectionSetCover(object sender, RoutedEventArgs e)
        {
            var result = await Utils.SelectCoverFile();
            if (result == null) return;
            foreach (var item in VidListItems)
            {
                item.AlbumCoverPath = result.Path;
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
                VidListItems.Remove(item);
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
                _prevCoverPath = null;
                IconBrowseCover.Visibility = Visibility.Visible;             
            }
            else if (_prevCoverPath == null || _prevCoverPath != _currentlyEditedItem.AlbumCoverPath)
            {
                var uri = new Uri(_currentlyEditedItem.AlbumCoverPath);
                if (uri.IsFile)
                {
                    var thumb = await StorageFile.GetFileFromPathAsync(uri.LocalPath);
                    using (var fileStream = await thumb.OpenAsync(Windows.Storage.FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.SetSource(fileStream);
                        TagAlbumCover.Source = bitmapImage;
                    }
                }
                else
                {
                    TagAlbumCover.Source = new BitmapImage(new Uri(_currentlyEditedItem.AlbumCoverPath));
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
            DetailsTrimStart.Text = caller.trimStart == null ? "" : caller.trimStart.ToString();
            DetailsTrimEnd.Text = caller.trimEnd == null ? "" : caller.trimEnd.ToString();
        }
        private void DetailsSuggestClicked(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox box = sender as AutoSuggestBox;
            box.IsSuggestionListOpen = true;
        }
        private void DetailsClose(object sender, RoutedEventArgs e)
        {
            //_currentlyEditedItem.DisableTextSelection();
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
            var result = await Utils.SelectCoverFile();
            if(result == null) return;
            StorageApplicationPermissions.MostRecentlyUsedList.Add(result);
            using (var fileStream = await result.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.SetSource(fileStream);
                TagAlbumCover.Source = bitmapImage;
            }
            IconBrowseCover.Visibility = Visibility.Collapsed;
            _currentlyEditedItem.AlbumCoverPath = result.Path;
        }

        private void DetailsSelectStockThumb(object sender, RoutedEventArgs e)
        {
            _currentlyEditedItem.AlbumCoverPath = _currentlyEditedItem.thumbUrl;
            TagAlbumCover.Source = new BitmapImage(new Uri(_currentlyEditedItem.AlbumCoverPath));
            IconBrowseCover.Visibility = Visibility.Collapsed;
        }

        private void DetailRemoveCover(object sender, RoutedEventArgs e)
        {
            _currentlyEditedItem.AlbumCoverPath = null;
            TagAlbumCover.Source = null;
            IconBrowseCover.Visibility = Visibility.Visible;
        }

        public void DetailsSetTrimEnd(string id, int secs)
        {
            if(id != _currentlyEditedItem.id)
                return;

            DetailsTrimEnd.Text = secs.ToString();
        }

        public void DetailsSetTrimStart(string id, int secs)
        {
            if (id != _currentlyEditedItem.id)
                return;

            DetailsTrimStart.Text = secs.ToString();
        }

        private void DetailsTrimStartChanged(object sender, TextChangedEventArgs e)
        {
            int? start;
            if (DetailsTrimStart.Text == "")
                start = null;
            else
            {
                int time;
                bool success = int.TryParse(DetailsTrimStart.Text, out time);
                if (!success)
                {
                    DetailsTrimStart.Text = "";
                    start = null;
                }
                else
                {
                    start = time;
                }
            }
           
            _currentlyEditedItem.trimStart = start;
            Utils.GetMainPageInstance().TrimSetStart(start);
            if (start != null)
            {
                int end;
                bool success = int.TryParse(DetailsTrimEnd.Text, out end);
                if (success && start < end)
                {
                    _currentlyEditedItem.trimEnd = end;
                    Utils.GetMainPageInstance().TrimSetEnd(end);
                }
            }
        }

        private void DetailsTrimEndChanged(object sender, TextChangedEventArgs e)
        {
            int? final;
            if (DetailsTrimEnd.Text == "")
                final = null;
            else
            {
                int time;
                bool success = int.TryParse(DetailsTrimEnd.Text, out time);
                if (!success)
                {
                    DetailsTrimStart.Text = "";
                    final = null;
                }
                else
                    final = time;
            }
            int? end = _currentlyEditedItem.trimEnd ?? 0; // ?? - is null?
            if (final == null || final >= end)
            {
                _currentlyEditedItem.trimEnd = final;
                Utils.GetMainPageInstance().TrimSetEnd(final);
                TrimEndGreaterNotice.Visibility = Visibility.Collapsed;
            }
            else
            {
                _currentlyEditedItem.trimEnd = null;
                Utils.GetMainPageInstance().TrimSetEnd(null);
                TrimEndGreaterNotice.Visibility = Visibility.Visible;
            }
            if (final != null)
            {
                int start;
                bool success = int.TryParse(DetailsTrimStart.Text, out start);
                if (success && start < end)
                {
                    _currentlyEditedItem.trimStart = start;
                    Utils.GetMainPageInstance().TrimSetStart(start);
                }              
            }
        }
        #endregion

    }
}
