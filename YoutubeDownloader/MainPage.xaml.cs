using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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
        }

        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {

            if (YTDownload.IsIdValid(BoxID.Text))
            {
                List<string> videos = await YTDownload.GetVideosInPlaylist(BoxID.Text);
                foreach (var video in videos)
                {
                    VideoList.Items.Add(new VideoItem(video));
                }
            }


        }

        public ListView GetVideosListView() { return VideoList; }
    }
}
