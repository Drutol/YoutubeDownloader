using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace YoutubeDownloader
{
    public sealed partial class VideoItem : UserControl
    {
        public VideoItem(string id)
        {
            InitializeComponent();
            PopulateVideoInfo(id);
        }

        private async void PopulateVideoInfo(string id)
        {
            Dictionary<string, string> info = await YTDownload.GetVideoDetails(id);

            VideoThumb.Source = new BitmapImage(new Uri(info["thumbSmall"]));
            VideoThumb.MinHeight = 50;
            VideoThumb.MinWidth = 50;
            VideoTitle.Text = info["title"];
        }
    }
}
