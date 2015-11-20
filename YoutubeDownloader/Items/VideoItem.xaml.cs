﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExtractor;
using System.Threading.Tasks;
using System.Linq;

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
        public string targetedFileFormat;
        public string sourceFileFormat = ".mp4";
        public string fileName;
        public bool requiresConv = true;

        public string tagAlbum = "";
        public string tagTitle = "";
        public string tagArtist = "";

        bool report; //history

        SuggestedTagsPackage suggestions;


        public  VideoItem(string id,string origin = "",bool report = false) // as for playlist title
        {
            InitializeComponent();

            this.id = id;
            this.origin = origin;
            this.report = report;
            if (Settings.GetPrefferedOutputFormat() == Settings.PossibleOutputFormats.FORMAT_MP4)
                requiresConv = false;
            tagAlbum = origin;
            suggestions = new SuggestedTagsPackage();

            Task.Run(() =>
            {
                PopulateVideoInfo();
            });

        }

        private async void PopulateVideoInfo()
        {
            try
            {
                Dictionary<string, string> info = await YTDownload.GetVideoDetails(id);
                if(report)
                    HistoryManager.AddNewEntry(new HistoryEntry(info["thumbSmall"], info["title"], info["author"], id));
                try
                {
                    suggestions = TagParser.AttemptToParseTags(info["title"], info["details"], "", info["author"]);
                }
                catch (Exception exce)
                {
                    Debug.WriteLine(exce.Message);
                }
                if (suggestions.suggestedAuthor != "")
                    tagArtist = suggestions.suggestedAuthor;
                if (suggestions.suggestedTitle != "")
                    tagTitle = suggestions.suggestedTitle;
                // Use dispatcher only to interact with the UI , putting the async method in there will block UI thread.
                // Info is obtained in the line above and populated on the UI thread.
                //Data loaded , 1/2 steps
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VideoThumb.Source = new BitmapImage(new Uri(info["thumbSmall"]));
                        VideoTitle.Text = info["title"];
                        VideoAuthor.Text = info["author"];

                        thumbUrl = info["thumbHigh"];

                        LoadingInfo.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        progressYoutubeExtraction.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    });


                await Task.Run(() =>
                    {
                        PopulateVideoDownloadInfo();
                    });                         
            }
            catch(Exception exc)
            {
                Debug.WriteLine(exc.Message);
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
                //TODO : I've id -> why bother extractor with id extracting? -> Edit Extractor code
                IEnumerable<VideoInfo> videoInfos = DownloadUrlResolver.GetDownloadUrls(link);

                VideoInfo vid = null;
                List<VideoInfo> items = new List<VideoInfo>();
                string format = "";

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
                        title = vid.Title;
                        targetedFileFormat = format;
                        btnDownload.IsEnabled = true;
                        progressYoutubeExtraction.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        if (Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_AUTO_DL))
                            QueueDownload();
                    });
                }
                

            }
            catch (Exception)
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
            YTDownload.DownloadVideo(downloadUrl, Utils.CleanFileName(title + targetedFileFormat),this);
        }

        public void ForceDownload(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            QueueManager.Instance.ForceDownload(this);
        }

        public void QueueDownload(int retries = 5)
        {
            if (downloadUrl == null && retries >= 0)
            {
                System.Threading.Tasks.Task.Delay(1000);
                QueueDownload(retries - 1);
            }
            else if (downloadUrl != null)
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
                VideoTitle.Text += "  -  Failed parsing info , is video still available?";
                ActionButtons.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ErrorButton.Visibility = Windows.UI.Xaml.Visibility.Visible;
                progressYoutubeExtraction.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                ErrorImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                //IsEnabled = false;
            });
        }

        private void SelectSuggestionTitle(object sender, SelectionChangedEventArgs e)
        {
            
            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedIndex > 0)
            {
                TextBlock btn = (TextBlock)cmb.SelectedItem;
                TagTitle.Text = btn.Text;
                ComboTitles.SelectedIndex = 0;
                ComboTitles.IsDropDownOpen = false;
            }

        }

        private void SelectSuggestionAuthor(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cmb = (ComboBox)sender;
            if (cmb.SelectedIndex > 0)
            {
                TextBlock btn = (TextBlock)cmb.SelectedItem;
                TagArtist.Text = btn.Text;
                ComboArtist.SelectedIndex = 0;
                ComboArtist.IsDropDownOpen = false;
            }
        }

        private async void OpenInBrowswer(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.youtube.com/watch?v=" + id));
        }

        private void SuggestionClosed(object sender, object e)
        {
            var cmb = sender as ComboBox;
            cmb.SelectedIndex = 0;
        }
    }
}
