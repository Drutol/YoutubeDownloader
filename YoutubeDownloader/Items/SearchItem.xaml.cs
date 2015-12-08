using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using YoutubeExtractor;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace YoutubeDownloader
{
    public sealed partial class SearchItem : UserControl
    {
        public string id;
        public string title;
        public string author;
        public string desc;
        public string downloadUrl;
        public string targetedFileFormat;
        public string thumbUrl;
        public string thumbDownloadUrl;

        public SearchItem(string id, Dictionary<string, string> info)
        {
            this.InitializeComponent();
            Task.Run(async () =>
            {
                try
                {
                    // Use dispatcher only to interact with the UI , putting the async method in there will block UI thread.
                    // Info is obtained in the line above and populated on the UI thread.
                    //Data loaded , 1/2 steps
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VideoThumb.Source = new BitmapImage(new Uri(info["thumbSmall"]));
                        thumbUrl = info["thumbSmall"];
                        VideoTitle.Text = info["title"];
                        VideoAuthor.Text = info["author"];
                        thumbDownloadUrl = info["thumbHigh"];

                        LoadingInfo.Visibility = Visibility.Collapsed;
                    });
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                }
            });
        }

        public void SetProgress(int progress)
        {
            Progress.Value = progress;
        }

        public void ForceDownload(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var page = Utils.GetMainPageInstance().GetDownloaderPage();
            page.vidListItems.Add(VideoItem.FromSerachItem(this));
        }

        private async void SetErrorState()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                VideoTitle.Text += "  -  Failed parsing info , is video still available?";
                ErrorButton.Visibility = Visibility.Visible;
                progressYoutubeExtraction.Visibility = Visibility.Collapsed;
                ErrorImage.Visibility = Visibility.Visible;
                ActionButtons.Visibility = Visibility.Collapsed;
                CompactContainer.Visibility = Visibility.Collapsed;
            });
        }

        private async void OpenInBrowswer(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.youtube.com/watch?v=" + id));
        }

        private async void PopulateVideoDownloadInfo()
        {
            try
            {
                string link = "https://www.youtube.com/watch?v=" + id;
                //TODO : I've id -> why bother extractor with id extracting? -> Edit Extractor code
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

                VideoInfo vid = null;
                List<VideoInfo> items = new List<VideoInfo>();

                foreach (var item in videoInfos)
                {
                    if (item.DownloadUrl.Contains("mime=audio/mp4"))
                    {
                        items.Add(item);
                    }
                }
                // Almost always there will be one entry , almost..
                vid = items.OrderByDescending(info => info.AudioBitrate).First();

                if (vid != null)
                {
                    if (vid.RequiresDecryption)
                    {
                        DownloadUrlResolver.DecryptDownloadUrl(vid);
                    }
                    // Same thing here , restrain from intense work down there!
                    //Data loaded from youtube , 2/2
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        downloadUrl = vid.DownloadUrl;
                        progressYoutubeExtraction.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        PreviewVideo(null,null);
                    });
                }


            }
            catch (Exception)
            {
                SetErrorState();
            }
        }

        private void btnAddToDownload_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PreviewVideo(object sender, RoutedEventArgs e)
        {
            GetMainPageInstance().BeginVideoPreview(new Uri(downloadUrl));
        }

        /// <summary>
        /// Call only from UI thread
        /// </summary>
        /// <returns></returns>
        private static MainPage GetMainPageInstance()
        {
            return Utils.GetMainPageInstance();
        }

        public static VideoItem FromSerachItem(SearchItem item)
        {
            throw new NotImplementedException();
        }
    }
}
