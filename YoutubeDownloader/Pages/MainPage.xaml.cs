using System;
using Windows.Storage.AccessCache;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using YoutubeDownloader.Pages;

namespace YoutubeDownloader
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Settings.Init();
            StorageApplicationPermissions.MostRecentlyUsedList.Clear(); // Reset
            DownloaderContent.Navigate(typeof(DownloadPage));
            SearchContent.Navigate(typeof(SearchPage));
        }

        public void ReversePane()
        {
            MainMenu.IsPaneOpen = !MainMenu.IsPaneOpen;
            if (MainMenu.IsPaneOpen)
                HamburgerPane.PaneOpened();            
        }

        private void MainMenu_PaneClosed(SplitView sender, object args)
        {
            HamburgerPane.PaneClosed();
        }

        public void NavigateSettings()
        {
            PersistentContent.Visibility = Visibility.Collapsed;
            MiscContent.Visibility = Visibility.Visible;
            MiscContent.Navigate(typeof(SettingsPage));
        }

        public void NavigateDownloader()
        {
            PersistentContent.Visibility = Visibility.Visible;
            DownloaderContent.Visibility = Visibility.Visible;
            MiscContent.Visibility = Visibility.Collapsed;
        }

        internal void NavigateHistory()
        {
            PersistentContent.Visibility = Visibility.Collapsed;
            MiscContent.Visibility = Visibility.Visible;
            MiscContent.Navigate(typeof(HistoryPage));
        }

        internal void NavigateSearch()
        {
            PersistentContent.Visibility = Visibility.Visible;
            DownloaderContent.Visibility = Visibility.Collapsed;
            MiscContent.Visibility = Visibility.Collapsed;
        }
        #region BottomAppbBar

        private bool _isAppBarOpen;
        public void AppBarOpened()
        {
            _isAppBarOpen = true;
            SetBestMarginForContent();
            PreviewCancel.VerticalAlignment = VerticalAlignment.Top;
            PreviewContainer.Margin = new Thickness(0, 0, 0, 48);
        }
        public void AppBarClosed()
        {
            _isAppBarOpen = false;
            SetBestMarginForContent();
            PreviewCancel.VerticalAlignment = VerticalAlignment.Bottom;
            PreviewContainer.Margin = new Thickness(0);
        }
        #endregion

        #region History
        //private async void PopulateHistory()
        //{
        //    if (!ShouldPopulateHistory)
        //        return;
        //    HistoryWorking.Visibility = Visibility.Visible;
        //    historyListItems.Clear();

        //    var history = new List<HistoryEntry>();
        //    await Task.Run(() => { history = HistoryManager.GetHistoryEntries(); });
        //    foreach (var item in history)
        //    {
        //        historyListItems.Add(new HistoryItem(item));
        //    }
        //    HistoryWorking.Visibility = Visibility.Collapsed;
        //    HistoryList.ItemsSource = historyListItems;
        //    ShouldPopulateHistory = false;
        //}

        #endregion

        #region Preview

        private VideoItem _currentlyPreviewedItem;
        private void PreviewMediaOpened(object sender, RoutedEventArgs e)
        {
            TrimControlsEndLabel.Text = string.Format("{0:mm\\:ss}", Preview.NaturalDuration.TimeSpan);
        }
        private void VideoPreviewCancel(object sender, RoutedEventArgs e)
        {
            Preview.Stop();
            _currentlyPreviewedItem = null;
            PreviewInfo.Visibility = Visibility.Collapsed;
            SetBestMarginForContent();
        }

        public void BeginVideoPreview(Uri uri, VideoItem caller,ImageSource src = null)
        {
            Preview.Source = uri;
            Preview.Play();
            _currentlyPreviewedItem = caller;
            PreviewInfo.Visibility = Visibility.Visible;

            if (src != null)
            {
                PreviewThumb.Visibility = Visibility.Visible;
                PreviewTitle.Visibility = Visibility.Visible;
                PreviewThumb.Source = src;
                PreviewTitle.Text = caller.title;
            }
            else
            {
                PreviewThumb.Visibility = Visibility.Collapsed;
                PreviewTitle.Visibility = Visibility.Collapsed;
            }

            SetBestMarginForContent();
        }

        internal void BeginVideoPreview(Uri uri,ImageSource src = null,string title = "")
        {
            Preview.Source = uri;
            Preview.Play();
            PreviewInfo.Visibility = Visibility.Visible;
            ShowTrim.Visibility = Visibility.Collapsed;
            if(src != null)
            {
                PreviewThumb.Visibility = Visibility.Visible;
                PreviewTitle.Visibility = Visibility.Visible;
                PreviewThumb.Source = src;
                PreviewTitle.Text = title;
            }
            else
            {
                PreviewThumb.Visibility = Visibility.Collapsed;
                PreviewTitle.Visibility = Visibility.Collapsed;
            }
                
            SetBestMarginForContent(true);
        }



        private void TrimSetStart(object sender, RoutedEventArgs e)
        {
            var time = Preview.Position;
            _currentlyPreviewedItem.trimStart = (int?)time.TotalSeconds;
            TrimControlsStartLabel.Text = string.Format("{0:mm\\:ss}", time);
        }

        private void TrimSetEnd(object sender, RoutedEventArgs e)
        {
            var time = Preview.Position;
            _currentlyPreviewedItem.trimEnd = (int?)time.TotalSeconds;
            TrimControlsEndLabel.Text = string.Format("{0:mm\\:ss}", time);
        }
        private void ShowTrimChangeState(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(ShowTrimBg.Opacity == 0)
            {
                ShowTrimBg.Opacity = .75;
                TrimControls.Visibility = Visibility.Visible;
            }
            else
            {
                ShowTrimBg.Opacity = 0;
                TrimControls.Visibility = Visibility.Collapsed;
            }
        }

        public void TrimResetStart()
        {
            TrimControlsStartLabel.Text = "00:00";
        }
        public void TrimResetEnd()
        {
            TrimControlsEndLabel.Text = String.Format("{0:mm\\:ss}", Preview.NaturalDuration.TimeSpan);
        }

        #endregion
        #region Helpers
        private void SetBestMarginForContent(bool force=false) //force when there's no caller - search
        {
            if(_currentlyPreviewedItem != null || force)
                if(_isAppBarOpen)
                    MainMenu.Margin = new Thickness(0, 0, 0, 96);
                else
                    MainMenu.Margin = new Thickness(0, 0, 0, 48);
            else
                MainMenu.Margin = new Thickness(0, 0, 0, 0);
        }
        internal DownloadPage GetDownloaderPage()
        {
            return DownloaderContent.Content as DownloadPage;
        }
        internal SearchPage GetSearchPage()
        {
            return SearchContent.Content as SearchPage;
        }
        #endregion



    }


}
