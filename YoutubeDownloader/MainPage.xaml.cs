using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.Storage;
using YoutubeExtractor;
using Windows.UI.Popups;
using System.Linq;
using System.IO;
using System;


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


        public ObservableCollection<VideoItem> vidListItems;
        private async void BtnDownload_Click(object sender, RoutedEventArgs e)
        {
            VideoItem vidItem;
            if (YTDownload.IsIdValid(BoxID.Text))
            {
                vidListItems = new ObservableCollection<VideoItem>();
                SpinnerLoadingPlaylist.Visibility = Visibility.Visible;
                List<string> videos = await YTDownload.GetVideosInPlaylist(BoxID.Text);
                SpinnerLoadingPlaylist.Visibility = Visibility.Collapsed;
                foreach (var video in videos)
                {
                    vidItem = new VideoItem(video);
                    vidListItems.Add(vidItem);
                }

                VideoList.ItemsSource = vidListItems;
            }


            BoxID.Text = ApplicationData.Current.LocalFolder.Path;
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                 {
                     // Our test youtube link
                     string link = "https://www.youtube.com/watch?v=a97Acuqudxo";

                     IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

                     VideoInfo vid = videoInfos
                         .Where(info => info.CanExtractAudio)
                         .OrderByDescending(info => info.AudioBitrate)
                         .First();

                     if (vid.RequiresDecryption)
                     {
                         DownloadUrlResolver.DecryptDownloadUrl(vid);
                     }

                     YTDownload.DownloadVideo(vid.DownloadUrl, "file.lol", "a97Acuqudxo");

                     //HttpClient http = new System.Net.Http.HttpClient();
                     //HttpResponseMessage response = await http.GetAsync(vid.DownloadUrl);

                     //Stream webresponse = await response.Content.ReadAsStreamAsync();


                     //StreamReader reader = new StreamReader(webresponse);


                     //reader.BaseStream.Seek(0, SeekOrigin.Begin);
                     //reader.BaseStream.CopyTo(fileStream);
                 });
            }
            catch (Exception exc)
            {
                MessageDialog dialog = new MessageDialog(exc.Message);
                await dialog.ShowAsync();
            }


            //BoxID.Text = vid.DownloadUrl;



        }

        public ListView GetVideosListView() { return VideoList; }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.IsPaneOpen = !MainMenu.IsPaneOpen;
        }
    }
}
