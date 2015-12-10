using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace YoutubeDownloader.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        ObservableCollection<SearchItem> searchItems;
        private string nextPageToken;
        private string prevPageToken;


        private void StartQuery(object sender, RoutedEventArgs e)
        {
            StartQuery(SearchQuery.Text);
        }

        public void StartRelatedQuery(string relatedId)
        {
            StartQuery("", "", relatedId);
        }

        private async void StartQuery(string query,string token = "",string relatedId = "")
        {
            SpinnerLoadingSearch.Visibility = Visibility.Visible;
            EmptyNotice.Visibility = Visibility.Collapsed;
            searchItems = new ObservableCollection<SearchItem>();
            var videos = await YTDownload.GetSearchResults(query,(QueryType.SelectedIndex == 0 ? "video" : "playlist"),relatedId,token);      
            ProcessPageTokens(videos["tokens"]["prev"],videos["tokens"]["next"]);
            videos.Remove("tokens");
            foreach (var item in videos)
            {
                searchItems.Add(new SearchItem(item.Key, item.Value));
            }
            VideoList.ItemsSource = searchItems;
            SpinnerLoadingSearch.Visibility = Visibility.Collapsed;        
        }

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

        private void MassDownload(object sender, RoutedEventArgs e)
        {
            foreach (SearchItem item in VideoList.SelectedItems)
            {
                Utils.GetMainPageInstance().GetDownloaderPage().AddVideoItem(VideoItem.FromSerachItem(item));
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            StartQuery("", nextPageToken);
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            StartQuery("", prevPageToken);
        }

        private void ResetPageTokens()
        {
            nextPageToken = "";
            prevPageToken = "";
            Pages.Visibility = Visibility.Collapsed;
            SearchQuery.SetValue(Grid.ColumnSpanProperty, 2);
        }

        private void ProcessPageTokens(string tp, string tn) // previous and next
        {
            if (tp != null || tn != null)
            {
                Pages.Visibility = Visibility.Visible;
                SearchQuery.SetValue(Grid.ColumnSpanProperty, 1);
            }
            else
            {
                Pages.Visibility = Visibility.Collapsed;
                SearchQuery.SetValue(Grid.ColumnSpanProperty, 2);
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

    }
}
