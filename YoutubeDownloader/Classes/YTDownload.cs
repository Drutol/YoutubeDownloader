using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Newtonsoft.Json;
using Buffer = Windows.Storage.Streams.Buffer;

namespace YoutubeDownloader
{
    #region Enums
    enum RequestTypes
    {
        REQUEST_PLAYLIST_ITEMS,
        REQUEST_VIDEO,
        REQUEST_PLAYLIST,
        REQUEST_SEARCH
    }

    public enum IdType
    {
        TYPE_PLAYLIST,
        TYPE_VIDEO,
        INVALID
    }
    #endregion

    public struct SearchPackage
    {
        public enum UploadTimeFrame
        {
            Today,
            LastWeek,
            LastMonth,
            AllTime
        }
        public string Related;
        public string Query;
        public string Order;
        public UploadTimeFrame Date;
        public string Token;
        public string Type;

        public SearchPackage(string related, string query, string order, string date, string token, string type)
        {
            Related = related;
            Query = query;
            Order = order;
            switch (date)
            {
                case "Today":
                    Date = UploadTimeFrame.Today;
                    break;
                case "Last week":
                    Date = UploadTimeFrame.LastWeek;
                    break;
                case "Last month":
                    Date = UploadTimeFrame.LastMonth;
                    break;
                default:
                    Date = UploadTimeFrame.AllTime;
                    break;
            }
            Token = token;
            Type = type;
        }
    }


    static class YTDownload
    {
        private const string API_KEY = "AIzaSyCC1bN7iQNMc60AoocV7V0ub1VKPiib0zA"; //don't hate me :(
        public static async Task<Tuple<List<string>,string,string>> GetVideosInPlaylist(string playlistId,string pageToken = "")
        {
            List<string> videos = new List<string>();
            string nextPage = "", prevPage="";
            try
            {
                WebRequest request = GetWebRequest(RequestTypes.REQUEST_PLAYLIST_ITEMS, playlistId,pageToken);
                
                dynamic objResponse = await GetRequestResponse(request);
                try
                {
                    nextPage = objResponse.nextPageToken;
                    prevPage = objResponse.prevPageToken;
                }
                catch (Exception)
                {
                    // ignored
                }
                foreach (var item in objResponse.items)
                {
                    try
                    {
                        videos.Add((string)item.contentDetails.videoId);
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine("Error adding video <GetVideosInPlaylist>");
                    }
                }

            }
            catch(Exception e)
            {
                MessageDialog dialog = new MessageDialog(e.Message);
                await dialog.ShowAsync();
            }

            return new Tuple<List<string>, string, string>(videos,nextPage,prevPage);
        }

        #region Helpers

        private static WebRequest GetWebRequest(SearchPackage pcSearchPackage)
        {
            string uri = "";
            string dateString = "&publishedAfter=";
            DateTime date = DateTime.Now;
            switch (pcSearchPackage.Date)
            {
                case SearchPackage.UploadTimeFrame.Today:
                    date = date.AddDays(-1);
                    break;
                case SearchPackage.UploadTimeFrame.LastWeek:
                    date = date.AddDays(-7);
                    break;
                case SearchPackage.UploadTimeFrame.LastMonth:
                    date = date.AddDays(-30);
                    break;
            }
            if (pcSearchPackage.Date == SearchPackage.UploadTimeFrame.AllTime)
                dateString = "";
            else
            {
                dateString += date.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss") + "Z";
            }

            if (pcSearchPackage.Related == "")
                uri = $"https://www.googleapis.com/youtube/v3/search?part=snippet&key={API_KEY}&q={pcSearchPackage.Query}&type={pcSearchPackage.Type}&maxResults={Settings.GetValueForSetting(Settings.PossibleValueSettings.SETTING_PER_PAGE)}&order={pcSearchPackage.Order}{dateString}";
            else
                uri = $"https://www.googleapis.com/youtube/v3/search?part=snippet&key={API_KEY}&relatedToVideoId={pcSearchPackage.Related}&maxResults={Settings.GetValueForSetting(Settings.PossibleValueSettings.SETTING_PER_PAGE)}&type=video&order={pcSearchPackage.Order}{dateString}";

            if (pcSearchPackage.Token != "")
                uri += "&pageToken=" + pcSearchPackage.Token;
            WebRequest request = WebRequest.Create(Uri.EscapeUriString(uri));
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";

            return request;
        }

        private static WebRequest GetWebRequest(RequestTypes RequestType, string id, string pageToken = "", string queryType = "", string relatedId = "")
        {
            string uri;

            switch (RequestType)
            {
                case RequestTypes.REQUEST_PLAYLIST_ITEMS:
                    uri = $"https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&maxResults={Settings.GetValueForSetting(Settings.PossibleValueSettings.SETTING_PER_PAGE)}&playlistId={id}&key={API_KEY}";
                    break;
                case RequestTypes.REQUEST_VIDEO:
                    uri = "https://www.googleapis.com/youtube/v3/videos?id=" + id + "&part=snippet&key=" + API_KEY;
                    break;
                case RequestTypes.REQUEST_PLAYLIST:
                    uri = "https://www.googleapis.com/youtube/v3/playlists?part=snippet&maxResults=50&id=" + id + "&key=" + API_KEY;
                    break;
                default:
                    throw new Exception("Invalid Request");
            }
            if (pageToken != "")
                uri += "&pageToken=" + pageToken;
            WebRequest request = WebRequest.Create(Uri.EscapeUriString(uri));
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";

            return request;
        }

        static async private Task<dynamic> GetRequestResponse(WebRequest request)
        {
            var response = await request.GetResponseAsync();

            string responseString = "";
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                responseString = reader.ReadToEnd();
            }

            dynamic objResponse = JsonConvert.DeserializeObject(responseString);

            return objResponse;
        }

        #endregion

        public static async Task<Tuple<string, string, string>> GetPlaylistDetails(string id) // GetPlaylistName auth and thumb in short.
        {
            WebRequest request = GetWebRequest(RequestTypes.REQUEST_PLAYLIST, id);
            dynamic objResponse = await GetRequestResponse(request);

            string name = "";
            string auth = "";
            string thumb = "";

            foreach (var item in objResponse.items)
            {
                thumb = item.snippet.thumbnails.medium.url;
                auth = item.snippet.channelTitle;
                name = item.snippet.title;
            }

            return new Tuple<string, string, string>(name, auth, thumb);
        }

        public static async Task<Dictionary<string, string>> GetVideoDetails(string videoId)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();

            WebRequest request = GetWebRequest(RequestTypes.REQUEST_VIDEO, videoId);

            dynamic objResponse = await GetRequestResponse(request);
            try
            {
                foreach (var item in objResponse.items)
                {
                    info.Add("title", (string) (item.snippet.title));
                    info.Add("thumbSmall", (string) (item.snippet.thumbnails.medium.url));
                    info.Add("thumbHigh", (string) (item.snippet.thumbnails.high.url));
                    info.Add("author", (string) (item.snippet.channelTitle));
                    info.Add("details", (string) (item.snippet.description));
                }
            }
            catch (Exception e)
            {
                MessageDialog dialog = new MessageDialog(e.Message);
                await dialog.ShowAsync();
            }

            return info;
        }

        /// <summary>
        /// Checks if video or playlist id can be extracted from provided string
        /// <returns>
        /// Tuple consisting of enum and new id.
        /// </returns>
        /// </summary>
        public static async Task<Tuple<IdType, string>> IsIdValid(string id)
        {
            string finalId;
            if (id.Length != 11 && id.Length != 34)
            {
                id = await Utils.TryNormalizeYoutubeUrl(id);
                if (id.Length != 11 && id.Length != 34)
                {
                    finalId = "";
                    return new Tuple<IdType, string>(IdType.INVALID, finalId);
                }
            }
            finalId = id;
            return id.Length == 11 ? new Tuple<IdType, string>(IdType.TYPE_VIDEO, finalId) : new Tuple<IdType, string>(IdType.TYPE_PLAYLIST, finalId);
        }

        public static async Task<Dictionary<string, Dictionary<string, string>>> GetSearchResults(string query,string queryType, string relatedId = "", string token = "", string order = "", string date = "")
        {
            if (relatedId != "")
                query = "";
            var request = GetWebRequest(new SearchPackage(relatedId,query,order,date,token,queryType));
            string nextPage = "", prevPage = "";
            dynamic response = await GetRequestResponse(request);
            try
            {
                nextPage = response.nextPageToken;
                prevPage = response.prevPageToken;
            }
            catch (Exception)
            {
            }
            ;
            Dictionary<string, Dictionary<string, string>> result = new Dictionary<string, Dictionary<string, string>>();

            foreach (var item in response.items)
            {
                if (item.id.videoId == null)
                    continue;
                Dictionary<string, string> info = new Dictionary<string, string>
                {
                    {"title", (string) (item.snippet.title)},
                    { "thumbSmall", (string) (item.snippet.thumbnails.medium.url)},
                    { "thumbHigh", (string) (item.snippet.thumbnails.high.url)},
                    { "author", (string) (item.snippet.channelTitle)},
                    { "details", (string) (item.snippet.description)}
                };
                result.Add((string) item.id.videoId, info);
            }
            Dictionary<string, string> tokens = new Dictionary<string, string> {{"prev", prevPage}, {"next", nextPage}};
            result.Add("tokens", tokens);
            return result;
        }

        public static async void DownloadVideo(string url, string filename, VideoItem caller)
        {
            try
            {
                HttpClientHandler aHandler = new HttpClientHandler();
                aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                HttpClient aClient = new HttpClient(aHandler);
                aClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpResponseMessage response = await aClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead); // Important! ResponseHeadersRead.

                var outputFolder = await Settings.GetOutputFolder();
                var audioFile = await outputFolder.CreateFileAsync(filename + caller.sourceFileFormat, CreationCollisionOption.ReplaceExisting);
                var fs = await audioFile.OpenAsync(FileAccessMode.ReadWrite);

                Stream stream = await response.Content.ReadAsStreamAsync();
                if (response.Content.Headers.ContentLength != null)
                {
                    ulong totalBytes = (ulong) response.Content.Headers.ContentLength;
                    IInputStream inputStream = stream.AsInputStream();
                    ulong totalBytesRead = 0;
                    while (true)
                    {
                        // Read from the web.
                        IBuffer buffer = new Buffer(1024);
                        buffer = await inputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

                        if (buffer.Length == 0)
                            break; //we're done nothing left to read... cya!

                        // Report progress.
                        totalBytesRead += buffer.Length;
                        var read = totalBytesRead;
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => { caller.SetProgress((int) (100*read/totalBytes)); });

                        // Write to file.
                        await fs.WriteAsync(buffer);
                    }
                    inputStream.Dispose();
                }
                fs.Dispose();

                //Once we're done we are calling manager with info that item has been donwloaded.
                QueueManager.Instance.DownloadCompleted(caller.id);
                //And we have to queue it's conversion to different format.
                QueueManager.Instance.QueueNewItemConv(caller);

                //TODO Handle failure
            }
            catch (Exception exc)
            {
                Debug.WriteLine("DownloadVideo  " + url + "   " + exc.Message);
            }
        }

        public static async void DownloadThumbnails(Dictionary<string, string> urls)
        {
            var folder = await Settings.GetOutputFolder();
            foreach (KeyValuePair<string, string> url in urls)
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var thumb = await folder.CreateFileAsync(Utils.CleanFileName(url.Key + ".png"), CreationCollisionOption.ReplaceExisting);

                            HttpClient http = new HttpClient();
                            byte[] response = await http.GetByteArrayAsync(url.Value); //get bytes

                            var fs = await thumb.OpenStreamForWriteAsync(); //get stream

                            using (DataWriter writer = new DataWriter(fs.AsOutputStream()))
                            {
                                writer.WriteBytes(response); //write
                                await writer.StoreAsync();
                                await writer.FlushAsync();
                            }
                        }
                        catch (Exception exce)
                        {
                            Debug.WriteLine(exce.Message);
                        }
                    });
                }
                catch (Exception exc)
                {
                    Debug.WriteLine(exc.Message);
                }
            }
        }
    }
}