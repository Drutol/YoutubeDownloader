using Newtonsoft.Json;


using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Windows.UI.Popups;
using System.Net.Http;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Threading.Tasks;

namespace YoutubeDownloader
{
    #region Enums
    enum RequestTypes
    {
        REQUEST_PLAYLIST_ITEMS,
        REQUEST_VIDEO,
        REQUEST_PLAYLIST,
    }

    public enum IdType
    {
        TYPE_PLAYLIST,
        TYPE_VIDEO,
        INVALID,
    }
    #endregion
    static class YTDownload
    {
        const string API_KEY = "AIzaSyCC1bN7iQNMc60AoocV7V0ub1VKPiib0zA"; //don't hate me :(
        static public async System.Threading.Tasks.Task<List<string>> GetVideosInPlaylist(string playlistID)
        {
            List<string> videos = new List<string>();

            try
            {
                WebRequest request = GetWebRequest(RequestTypes.REQUEST_PLAYLIST_ITEMS, playlistID);

                dynamic objResponse = await GetRequestResponse(request);

                foreach (var item in objResponse.items)
                {
                    try
                    {
                        videos.Add((string)item.contentDetails.videoId);
                    }
                    catch (Exception e)
                    {
                        MessageDialog dialog = new MessageDialog(e.Message, item.contentDetails.videoId.ToString());
                        await dialog.ShowAsync();
                    }
                }

            }
            catch(Exception e)
            {
                MessageDialog dialog = new MessageDialog(e.Message);
                await dialog.ShowAsync();
            }

            return videos;
        }

        #region Helpers
        static private WebRequest GetWebRequest(RequestTypes RequestType, string id)
        {
            string uri;

            switch (RequestType)
            {
                case RequestTypes.REQUEST_PLAYLIST_ITEMS:
                    uri = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&maxResults=50&playlistId=" + id + "&key=" + API_KEY;
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
            WebRequest request = WebRequest.Create(Uri.EscapeUriString(uri));
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "GET";

            return request;
        }

        static async private System.Threading.Tasks.Task<dynamic> GetRequestResponse(WebRequest request)
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

        public static async Task<string> GetPlaylistDetails(string id) // GetPlaylistName in short.
        {
            WebRequest request = GetWebRequest(RequestTypes.REQUEST_PLAYLIST, id);
            dynamic objResponse = await GetRequestResponse(request);

            foreach (var item in objResponse.items)
            {
                return item.snippet.title;
            }

            return "";
        }

        static public async System.Threading.Tasks.Task<Dictionary<string,string>> GetVideoDetails(string videoId)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();

            WebRequest request = GetWebRequest(RequestTypes.REQUEST_VIDEO, videoId);

            dynamic objResponse = await GetRequestResponse(request);
            try
            {
                foreach (var item in objResponse.items)
                {
                    info.Add("title", (string)(item.snippet.title));
                    info.Add("thumbSmall", (string)(item.snippet.thumbnails.medium.url));
                    info.Add("thumbHigh", (string)(item.snippet.thumbnails.high.url));
                    info.Add("author", (string)(item.snippet.channelTitle));
                    info.Add("details", (string)(item.snippet.description));
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
        public static async Task<Tuple<IdType,string>> IsIdValid(string id)
        {
            string finalId;
            if (id.Length != 11 && id.Length != 34)
            {
                id = await Utils.TryNormalizeYoutubeUrl(id);
                if (id.Length != 11 && id.Length != 34)
                {
                    finalId = "";
                    return new Tuple<IdType, string>(IdType.INVALID,finalId);
                }
            }
            finalId = id;
            if (id.Length == 11) return new Tuple<IdType, string>(IdType.TYPE_VIDEO, finalId);
            else return new Tuple<IdType, string>(IdType.TYPE_PLAYLIST, finalId); ;
        }

        static async public void DownloadVideo(string url,string filename,VideoItem caller)
        {
            try
            {
                HttpClientHandler aHandler = new HttpClientHandler();
                aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                HttpClient aClient = new HttpClient(aHandler);
                aClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpResponseMessage response = await aClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead); // Important! ResponseHeadersRead.

                var outputFolder = await Settings.GetOutputFolder();
                var audioFile = await outputFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                var fs = await audioFile.OpenAsync(FileAccessMode.ReadWrite);

                Stream stream = await response.Content.ReadAsStreamAsync();
                ulong totalBytes = (ulong)response.Content.Headers.ContentLength;
                IInputStream inputStream = stream.AsInputStream();
                ulong totalBytesRead = 0;
                while (true)
                {
                    // Read from the web.
                    IBuffer buffer = new Windows.Storage.Streams.Buffer(1024);
                    buffer = await inputStream.ReadAsync(buffer, buffer.Capacity, InputStreamOptions.None);

                    if (buffer.Length == 0)
                        break; //we're done nothing left to read... cya!

                    // Report progress.
                    totalBytesRead += buffer.Length;
                    PopulateUI.UpdateVideoManipulationProgress(caller.id, (int)((100 * totalBytesRead) / totalBytes),PopulateUI.ProgressType.PROGRESS_DL); 

                    // Write to file.
                    await fs.WriteAsync(buffer);
                }
                inputStream.Dispose();
                fs.Dispose();

                //Once we're done we are calling manager with info that item has been donwloaded.
                QueueManager.Instance.DownloadCompleted(caller.id);
                //And we have to queue it's conversion to different format.
                QueueManager.Instance.QueueNewItemConv(caller);
         
                //TODO Handle failure
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("DownloadVideo  "+ url + "   " + exc.Message);
            }

        }

        public static async void DownloadThumbnails(Dictionary<string,string> urls)
        {
            var folder = await Settings.GetOutputFolder();
            foreach (KeyValuePair<string,string> url in urls)
            {
                try
                {
                    await Task.Run(async () =>
                    {
                        try
                        {
                            var thumb = await folder.CreateFileAsync(Utils.CleanFileName(url.Key + ".png"), CreationCollisionOption.ReplaceExisting);

                            HttpClient http = new System.Net.Http.HttpClient();
                            byte[] response = await http.GetByteArrayAsync(url.Value);

                            var fs = await thumb.OpenStreamForWriteAsync();

                            var writer = new DataWriter(fs.AsOutputStream());

                            writer.WriteBytes(response);
                            await writer.StoreAsync();
                            //TODO dispose writer?
                        }
                        catch (Exception exce)
                        {
                            System.Diagnostics.Debug.WriteLine(exce.Message);
                        }
                    });

                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.Message);
                }
            }
        }
    }
}