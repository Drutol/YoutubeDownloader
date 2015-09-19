using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Net;
using YoutubeExtractor;
using System;
using System.Net.Http;
using Windows.UI.Popups;
using Windows.Storage;

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
                SpinnerLoadingPlaylist.Visibility = Visibility.Visible;
                List<string> videos = await YTDownload.GetVideosInPlaylist(BoxID.Text);
                SpinnerLoadingPlaylist.Visibility = Visibility.Collapsed;
                foreach (var video in videos)
                {
                    VideoList.Items.Add(new VideoItem(video));
                }
            }


            //BoxID.Text = ApplicationData.Current.LocalFolder.Path;
            //try
            //{
            //    await System.Threading.Tasks.Task.Run(async () =>
            //     {
            //         StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            //         StorageFile file = await storageFolder.CreateFileAsync("filez.lol");
            //         var fileStream = await file.OpenStreamForWriteAsync();
            //        // Our test youtube link
            //        string link = "https://www.youtube.com/watch?v=wrdq_N_sLPY";

            //         IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            //         VideoInfo vid = videoInfos
            //             .Where(info => info.CanExtractAudio)
            //             .OrderByDescending(info => info.AudioBitrate)
            //             .First();

            //         if (vid.RequiresDecryption)
            //         {
            //             DownloadUrlResolver.DecryptDownloadUrl(vid);
            //         }

            //         HttpClient http = new System.Net.Http.HttpClient();
            //         HttpResponseMessage response = await http.GetAsync(vid.DownloadUrl);
            //         Stream webresponse = await response.Content.ReadAsStreamAsync();

            //         StreamReader reader = new StreamReader(webresponse);


            //         reader.BaseStream.Seek(0, SeekOrigin.Begin);
            //         reader.BaseStream.CopyTo(fileStream);
            //     });
            //}
            //catch (Exception exc)
            //{
            //    MessageDialog dialog = new MessageDialog(exc.Message);
            //    await dialog.ShowAsync();
            //}
           // fileStream.Dispose();


            //BoxID.Text = vid.DownloadUrl;



        }

        public ListView GetVideosListView() { return VideoList; }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.IsPaneOpen = !MainMenu.IsPaneOpen;
        }
    }
}
