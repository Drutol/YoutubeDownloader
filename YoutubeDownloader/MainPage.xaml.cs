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
using Windows.Media.Transcoding;


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
            Settings.Init();
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


            //foreach (VideoItem videoItem in vidListItems)
            //{
            //    try
            //    {
            //        await System.Threading.Tasks.Task.Run(() =>
            //         {
            //             // Our test youtube link
            //             string link = "https://www.youtube.com/watch?v=" + videoItem.id;

            //             IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

            //             VideoInfo vid = null;

            //             string format = "";

            //             foreach (var item in videoInfos)
            //             {

            //                 if (item.DownloadUrl.Contains("mime=audio/mp4"))
            //                 {
            //                     vid = item;
            //                     format = ".mp4";
            //                     break;
            //                 }
            //             }
            //             if (vid == null)
            //             {
            //                 foreach (var item in videoInfos)
            //                 {
            //                     if (item.DownloadUrl.Contains("mime=audio"))
            //                     {
            //                         vid = item;
            //                         format = ".webm";
            //                         break;
            //                     }
            //                 }
            //             }

            //             if (vid != null)
            //             {
            //                 if (vid.RequiresDecryption)
            //                 {
            //                     DownloadUrlResolver.DecryptDownloadUrl(vid);
            //                 }

            //                 System.Diagnostics.Debug.WriteLine("Found for :" + vid.Title);

            //                 YTDownload.DownloadVideo(vid.DownloadUrl, , videoItem.id);
            //             }
            //         });
            //    }
            //    catch (Exception exc)
            //    {
            //        MessageDialog dialog = new MessageDialog(exc.Message);
            //        await dialog.ShowAsync();
            //    }
            //}


            //BoxID.Text = vid.DownloadUrl;
            //MediaTranscoder trans = new MediaTranscoder();
            // trans.


        }



        public ListView GetVideosListView() { return VideoList; }
        public void SetAutoDownloadSetting(string val)
        {
            SettingAutoDownload.IsOn = val == "True" ? true : false;
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            MainMenu.IsPaneOpen = !MainMenu.IsPaneOpen;
        }

        private void ChangeSetting(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggle = (ToggleSwitch)sender;
            Settings.ChangeSetting(toggle.Name, Convert.ToString(toggle.IsOn));
        }
    }
}
