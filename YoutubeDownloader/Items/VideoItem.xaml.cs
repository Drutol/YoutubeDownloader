using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExtractor;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace YoutubeDownloader
{
    public sealed partial class VideoItem : UserControl
    {
        public string id;
        public string origin;
        public string downloadUrl;
        public string thumbUrl;
        public string title;
        public string fileFormat;
        
        public string tagAlbum = "";
        public string tagTitle = "";
        public string tagArtist = "";

        SuggestedTagsPackage suggestions;


        public  VideoItem(string id,string origin = "") // as for playlist title
        {
            InitializeComponent();
            //Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            this.id = id;
            this.origin = origin;
            tagAlbum = origin;
            suggestions = new SuggestedTagsPackage();
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
                suggestions = TagParser.AttemptToParseTags(info["title"], info["details"], "",info["author"]);
                if (suggestions.suggestedAuthor != "")
                    tagArtist = suggestions.suggestedAuthor;
                if (suggestions.suggestedTitle != "")
                    tagTitle = suggestions.suggestedTitle;
                // Use dispatcher only to interact with the UI , putting the async method in there will block UI thread.
                // Info is obtained in the line above and populated on the UI thread.
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VideoThumb.Source = new BitmapImage(new Uri(info["thumbSmall"]));
                        VideoTitle.Text = info["title"];
                        VideoAuthor.Text = info["author"];

                        thumbUrl = info["thumbHigh"];

                        LoadingInfo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    });

                

                await System.Threading.Tasks.Task.Run(() =>
                    {
                        PopulateVideoDownloadInfo();
                    });

                if (Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_AUTO_DL))
                    QueueManager.Instance.QueueNewItem(this);
                //YTDownload.DownloadVideo(downloadUrl, Utils.CleanFileName(title + fileFormat), id,this);
                else
                {
                    // Wait for manual download action.
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        btnDownload.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    });
                }
                
                
            }
            catch(Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
            }
        }

        internal void SetConvProgress(int progress)
        {
            ConversionProgress.Value = progress;
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
                    title = vid.Title;
                    fileFormat = format;
                });
            }
            catch (Exception exc)
            {
                SetErrorState();
            }
        }

        public void SetProgress(int progress)
        {
            Progress.Value = progress;
        }

        public void StartDownload(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            YTDownload.DownloadVideo(downloadUrl, Utils.CleanFileName(title + fileFormat), id,this);
        }

        public void ForceDownload(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            QueueManager.Instance.ForceDownload(this);
        }

        public void QueueDownload()
        {
            QueueManager.Instance.QueueNewItem(this);
        }

        private void SetMusicTags(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            tagArtist = TagArtist.Text;
            tagAlbum = TagAlbum.Text;
            tagTitle = TagTitle.Text;
            VideoItemFlyout.Hide();         
        }

        private void PopulateFlyout(object sender, object e)
        {
            if(ComboTitles.Items.Count == 1 && suggestions.titles.Count != 0)
                foreach (var item in suggestions.titles)
                {
                    TextBlock btn = new TextBlock();
                    btn.Text = item;
                    btn.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    btn.Height = 35;
                    ComboTitles.Items.Add(btn);
                }
            if (ComboArtist.Items.Count == 1 && suggestions.authors.Count != 0)
                foreach (var item in suggestions.authors)
                {
                    TextBlock btn = new TextBlock();
                    btn.Text = item;
                    btn.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                    btn.Height = 35;
                    ComboArtist.Items.Add(btn);
                }
            TagAlbum.Text = tagAlbum;
            TagTitle.Text = tagTitle;
            TagArtist.Text = tagArtist;
            TagNumber.Text = "0";
        }

        private void MouseButtonDown(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                VideoItemFlyout.ShowAt(this);
            }
        }

        private async void SetErrorState()
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                VideoTitle.Text = "Failed parsing info , is video still available?";
                VideoAuthor.Text = "";
                ActionButtons.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ErrorButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //IsEnabled = false;
            });
        }

        private void SelectSuggestionTitle(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            TextBlock btn = (TextBlock)cmb.SelectedItem;
            TagTitle.Text = btn.Text;
            ComboTitles.SelectedIndex = 0;
            ComboTitles.IsDropDownOpen = false;
        }

        private void SelectSuggestionAuthor(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            TextBlock btn = (TextBlock)cmb.SelectedItem;
            TagArtist.Text = btn.Text;
            ComboArtist.SelectedIndex = 0;
            ComboArtist.IsDropDownOpen = false;
        }

        private async void OpenInBrowswer(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.youtube.com/watch?v=" + id));
        }

    }
}
