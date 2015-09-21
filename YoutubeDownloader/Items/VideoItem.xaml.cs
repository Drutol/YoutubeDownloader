using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExtractor;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace YoutubeDownloader
{
    public sealed partial class VideoItem : UserControl
    {
        public string id;
        public string downloadUrl;
        public string title;
        public string fileFormat;

        public  VideoItem(string id)
        {
            InitializeComponent();
            Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.id = id;
            System.Threading.Tasks.Task.Run(() =>
            {
                PopulateVideoInfo();
            });

        }

        private async void PopulateVideoInfo()
        {
            try
            {
                Dictionary<string, string> info = await YTDownload.GetVideoDetails(id);
                // Use dispatcher only to interact with the UI , putting the async method in there will block UI thread.
                // Info is obtained in the line above and populated on the UI thread.
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                      {
                          VideoThumb.Source = new BitmapImage(new Uri(info["thumbSmall"]));
                          VideoTitle.Text = info["title"];
                          VideoAuthor.Text = info["author"];

                          Visibility = Windows.UI.Xaml.Visibility.Visible;
                      });
                await System.Threading.Tasks.Task.Run(() =>
                {
                    PopulateVideoDownloadInfo();
                });
                YTDownload.DownloadVideo(downloadUrl,Utils.CleanFileName(title + fileFormat),id);
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
            }
        }

        private async void PopulateVideoDownloadInfo()
        {
            try
            {
                string link = "https://www.youtube.com/watch?v=" + id;

                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

                VideoInfo vid = null;

                string format = "";

                foreach (var item in videoInfos)
                {

                    if (item.DownloadUrl.Contains("mime=audio/mp4"))
                    {
                        vid = item;
                        format = ".mp4";
                        break;
                    }
                }
                if (vid == null)
                {
                    foreach (var item in videoInfos)
                    {
                        if (item.DownloadUrl.Contains("mime=audio"))
                        {
                            vid = item;
                            format = ".webm";
                            break;
                        }
                    }
                }

                if (vid != null)
                {
                    if (vid.RequiresDecryption)
                    {
                        DownloadUrlResolver.DecryptDownloadUrl(vid);
                    }
                }
                // Same thing here , don't block UI thread;
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
                {                                  
                    downloadUrl = vid.DownloadUrl;
                    fileFormat = format;
                });
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
            }
        }

        public void SetProgress(int progress)
        {
            Progress.Value = progress;
            VideoAuthor.Text = Convert.ToString(progress);
        }
    }
}
