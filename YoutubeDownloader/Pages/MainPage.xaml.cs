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
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Data;
using System.Diagnostics;
using Windows.Media;

namespace YoutubeDownloader
{
    public partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
            Settings.Init();
            DownloaderContent.Navigate(typeof(Pages.DownloadPage));
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
            DownloaderContent.Visibility = Visibility.Collapsed;
            MiscContent.Visibility = Visibility.Visible;
            MiscContent.Navigate(typeof(Pages.SettingsPage));
        }

        public void NavigateDownloader()
        {
            DownloaderContent.Visibility = Visibility.Visible;
            MiscContent.Visibility = Visibility.Collapsed;
        }

        internal void NavigateHistory()
        {
            DownloaderContent.Visibility = Visibility.Collapsed;
            MiscContent.Visibility = Visibility.Visible;
            MiscContent.Navigate(typeof(Pages.HistoryPage));
        }
        #region BottomAppbBar
        public void AppBarOpened()
        {
            RootGrid.Margin = new Thickness(0, 0, 0, 48);
        }
        public void AppBarClosed()
        {
            RootGrid.Margin = new Thickness(0, 0, 0, 0);
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
        private void PreviewMediaOpened(object sender, RoutedEventArgs e)
        {
            TrimControlsEndLabel.Text = String.Format("{0:mm\\:ss}", Preview.NaturalDuration.TimeSpan);
        }
        private void VideoPreviewCancel(object sender, RoutedEventArgs e)
        {
            Preview.Stop();
            TrimControls.Visibility = Visibility.Collapsed;
            ContentGrid.Margin = new Thickness(4, 10, 16, 56);
            MainMenu.SetValue(Grid.RowSpanProperty, 2);
        }

        VideoItem currentlyPreviewedItem;
        public void BeginVideoPreview(Uri uri, VideoItem caller)
        {
            Preview.Source = uri;
            Preview.Play();
            currentlyPreviewedItem = caller;
            TrimControls.Visibility = Visibility.Visible;
            MainMenu.SetValue(Grid.RowSpanProperty, 1);
            //if (VideoDetails.Visibility == Visibility.Visible)
            //    ContentGrid.Margin = new Thickness(4, 10, 16, 120);
        }
        private void TrimSetStart(object sender, RoutedEventArgs e)
        {
            var time = Preview.Position;
            currentlyPreviewedItem.trimStart = (int?)time.TotalSeconds;
            TrimControlsStartLabel.Text = String.Format("{0:mm\\:ss}", time);
        }

        private void TrimSetEnd(object sender, RoutedEventArgs e)
        {
            var time = Preview.Position;
            currentlyPreviewedItem.trimEnd = (int?)time.TotalSeconds;
            TrimControlsEndLabel.Text = String.Format("{0:mm\\:ss}", time);
        }

        internal Pages.DownloadPage GetDownloaderPage()
        {
            return DownloaderContent.Content as Pages.DownloadPage;
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

        


    }


}
