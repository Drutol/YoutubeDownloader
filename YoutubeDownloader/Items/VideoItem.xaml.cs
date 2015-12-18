using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using YoutubeExtractor;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace YoutubeDownloader
{
    public partial class VideoItem : UserControl
    {
        public string id;
        public string origin;
        public string downloadUrl;
        public string thumbUrl;
        public string title;
        public string desc;
        public string targetedFileFormat;
        public string sourceFileFormat = ".mp4";
        public string fileName;
        public bool requiresConv = true;

        public string tagAlbum = "";
        public string tagTitle = "";
        public string tagArtist = "";

        private int? _trimStart;
        private int? _trimEnd;
        private Settings.PossibleOutputFormats _outputFormat;

        private bool isOk = true; //Yt Extraction
        bool report; //history
        public SuggestedTagsPackage suggestions;


        public Settings.PossibleOutputFormats outputFormat
        {
            get
            {
                return _outputFormat;
            }
            set
            {
                if (value != Settings.PossibleOutputFormats.FORMAT_MP3)
                    BtnEditTags.Visibility = Visibility.Collapsed;
                else
                    BtnEditTags.Visibility = Visibility.Visible;

                if (_outputFormat == Settings.PossibleOutputFormats.FORMAT_MP4)
                    requiresConv = false;

                _outputFormat = value;
            }
        }

        public int? trimStart
        {
            get
            {
                return _trimStart;
            }
            set
            {
                _trimStart = value;

                if (_trimStart == null)
                    TrimStart.Text = "";
                else
                    TrimStart.Text = $"Trim start : {String.Format("{0:mm\\:ss}", TimeSpan.FromSeconds((double)value))}";

                if (_trimStart > _trimEnd)
                {
                    RemoveTrimEnd(null, null);
                }

                CheckTrimRemovalButtons();
            }
        }

        public int? trimEnd
        {
            get
            {
                return _trimEnd;
            }
            set
            {
                _trimEnd = value;
                if (_trimEnd == null)
                    TrimEnd.Text = "";
                else
                    TrimEnd.Text = $"Trim end  : {String.Format("{0:mm\\:ss}", TimeSpan.FromSeconds((double)value))}";

                if (_trimEnd < _trimStart)
                {
                    RemoveTrimStart(null,null);
                }

                CheckTrimRemovalButtons();          
            }
        }

        public string AlbumCoverPath = null;

        private void CheckTrimRemovalButtons()
        {
            if (_trimEnd != null)
                BtnTrimRemoveEnd.Visibility = Visibility.Visible;
            else
                BtnTrimRemoveEnd.Visibility = Visibility.Collapsed;

            if (_trimStart != null)
                BtnTrimRemoveStart.Visibility = Visibility.Visible;
            else
                BtnTrimRemoveStart.Visibility = Visibility.Collapsed;

        }



        public VideoItem(string id,string origin = "",bool report = false) //origin as for playlist title
        {
            InitializeComponent();

            this.id = id;
            this.origin = origin;
            this.report = report;
            outputFormat = Settings.GetPrefferedOutputFormat();
            tagAlbum = origin;
            suggestions = new SuggestedTagsPackage();
            CheckTrimRemovalButtons();
            Task.Run(() =>
            {
                PopulateVideoInfo();
            });

        }

        public VideoItem(SearchItem item) // from search
        {
            InitializeComponent();

            id = item.id;
            origin = "";
            tagAlbum = "";
            report = false;
            downloadUrl = item.downloadUrl;
            outputFormat = Settings.GetPrefferedOutputFormat();

            title = item.title;
            desc = item.desc;

            ProgressYoutubeExtraction.Visibility = Visibility.Visible;

            suggestions = new SuggestedTagsPackage();
            CheckTrimRemovalButtons();
            Task.Run(async () =>
            {
                if(downloadUrl == null)
                    PopulateVideoDownloadInfo();
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    VideoThumb.Source = new BitmapImage(new Uri(item.thumbUrl));
                    VideoTitle.Text = title;
                    VideoAuthor.Text = item.author;

                    thumbUrl = item.thumbDownloadUrl;
                    if (Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_AUTO_COVER))
                        AlbumCoverPath = thumbUrl;
                    LoadingInfo.Visibility = Visibility.Collapsed;

                });
            });
            if (_outputFormat == Settings.PossibleOutputFormats.FORMAT_MP3 && Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_PARSE_TAGS))
                try
                {
                    suggestions = TagParser.AttemptToParseTags(title, desc, "", item.author);
                    if (suggestions.suggestedAuthor != "")
                        tagArtist = suggestions.suggestedAuthor;
                    else
                        tagArtist = item.author;
                    if (suggestions.suggestedTitle != "")
                        tagTitle = suggestions.suggestedTitle;
                }
                catch (Exception exce)
                {
                    Debug.WriteLine(exce.Message);
                }


        }

        private async void PopulateVideoInfo()
        {
            try
            {
                
                Dictionary<string, string> info = await YTDownload.GetVideoDetails(id);
                if (info.Count == 0)
                {
                    SetErrorState(true);
                    return;
                }
                if(report)
                    HistoryManager.AddNewEntry(new HistoryEntry(info["thumbSmall"], info["title"], info["author"], id));
                if (_outputFormat == Settings.PossibleOutputFormats.FORMAT_MP3 && Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_PARSE_TAGS))
                    try
                    {
                        suggestions = TagParser.AttemptToParseTags(info["title"], info["details"], "", info["author"]);
                        if (suggestions.suggestedAuthor != "")
                            tagArtist = suggestions.suggestedAuthor;
                        else
                            tagArtist = info["author"];
                        if (suggestions.suggestedTitle != "")
                            tagTitle = suggestions.suggestedTitle;
                    }
                    catch (Exception exce)
                    {
                        Debug.WriteLine(exce.Message);
                    }
                else
                    suggestions = new SuggestedTagsPackage();

                // Use dispatcher only to interact with the UI , putting the async method in there will block UI thread.
                // Info is obtained in the line above and populated on the UI thread.
                //Data loaded , 1/2 steps
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        VideoThumb.Source = new BitmapImage(new Uri(info["thumbSmall"]));
                        VideoTitle.Text = info["title"];
                        VideoAuthor.Text = info["author"];
                        thumbUrl = info["thumbHigh"];
                        if (Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_AUTO_COVER))
                            AlbumCoverPath = thumbUrl;

                        LoadingInfo.Visibility = Visibility.Collapsed;
                        ProgressYoutubeExtraction.Visibility = Visibility.Visible;
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
                        BtnDownload.IsEnabled = true;
                        ProgressYoutubeExtraction.Visibility = Visibility.Collapsed;
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

        public void StartDownload(object sender, RoutedEventArgs e)
        {
            YTDownload.DownloadVideo(downloadUrl, Utils.CleanFileName(title + targetedFileFormat),this);
        }

        public void ForceDownload(object sender, RoutedEventArgs e)
        {
            QueueManager.Instance.ForceDownload(this);
        }

        public void QueueDownload(int retries = 5)
        {
            if (downloadUrl == null && retries >= 0)
            {
                Task.Delay(1000);
                QueueDownload(retries - 1);
            }
            else if (downloadUrl != null)
                QueueManager.Instance.QueueNewItem(this);
        }

        private async void SetErrorState(bool completeFail = false) //complete when there's no response from yt whatsoever
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if(!completeFail)
                    VideoTitle.Text += "  -  Failed parsing info , is video still available?";
                else
                    VideoTitle.Text = "Failed parsing info , is video still available?";
                ErrorButton.Visibility = Visibility.Visible;
                ProgressYoutubeExtraction.Visibility = Visibility.Collapsed;
                ErrorImage.Visibility = Visibility.Visible;
                BtnEditTags.IsEnabled = false;
                ActionButtons.Visibility = Visibility.Collapsed;
                CompactContainer.Visibility = Visibility.Collapsed;
                LoadingInfo.Visibility = Visibility.Collapsed;
                isOk = false;
            });
        }

        private async void OpenInBrowswer(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("https://www.youtube.com/watch?v=" + id));
        }

        private void SuggestionClosed(object sender, object e)
        {
            var cmb = sender as ComboBox;
            cmb.SelectedIndex = 0;
        }

        private void btnEditTags_Click(object sender, RoutedEventArgs e)
        {
            if (_outputFormat != Settings.PossibleOutputFormats.FORMAT_MP3) return;
            VideoTitle.IsTextSelectionEnabled = true;
            VideoAuthor.IsTextSelectionEnabled = true;
            Utils.DetailsPopulate(this);
        }

        public void DisableTextSelection()
        {
            VideoTitle.IsTextSelectionEnabled = false;
            VideoAuthor.IsTextSelectionEnabled = false;
        }

        private void OpenVideoDetails(object sender, PointerRoutedEventArgs e)
        {
            if (isOk && e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
            {
                btnEditTags_Click(null, null);
            }
        }

        private void PreviewVideo(object sender, RoutedEventArgs e)
        {
            if (downloadUrl != null)
                GetMainPageInstance().BeginVideoPreview(new Uri(downloadUrl), this,VideoThumb.Source);
        }

        private void RemoveTrimStart(object sender, RoutedEventArgs e)
        {
            trimStart = null;
            GetMainPageInstance().TrimResetStart();
        }

        private void RemoveTrimEnd(object sender, RoutedEventArgs e)
        {
            trimEnd = null;
            GetMainPageInstance().TrimResetEnd();
        }

        public override bool Equals(object obj)
        {
            VideoItem caller = obj as VideoItem;
            return caller.id == id;
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
            return new VideoItem(item);
        }

    }
}
