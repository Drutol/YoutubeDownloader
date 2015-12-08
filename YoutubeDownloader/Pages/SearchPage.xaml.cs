using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class SearchPage : Page
    {
        public SearchPage()
        {
            this.InitializeComponent();
        }

        ObservableCollection<SearchItem> searchItems;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            searchItems = new ObservableCollection<SearchItem>();
            var videos = await YTDownload.GetSearchResults("monogatari");
            foreach (var item in videos)
            {
                searchItems.Add(new SearchItem(item.Key, item.Value));
            }
            VideoList.ItemsSource = searchItems;
        }
    }
}
