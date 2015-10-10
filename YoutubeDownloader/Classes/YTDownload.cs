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
using Windows.UI.Xaml.Controls;

namespace YoutubeDownloader
{

    enum RequestTypes
    {
        REQUEST_PLAYLIST,
        REQUEST_VIDEO,
    }

    static class YTDownload
    {
        const string API_KEY = "AIzaSyCC1bN7iQNMc60AoocV7V0ub1VKPiib0zA";
        static public async System.Threading.Tasks.Task<List<string>> GetVideosInPlaylist(string playlistID)
        {
            List<string> videos = new List<string>();

            try
            {
                WebRequest request = GetWebRequest(RequestTypes.REQUEST_PLAYLIST, playlistID);

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
                    info.Add("author", (string)(item.snippet.channelTitle));
                }
                
            }
            catch (Exception e)
            {
                MessageDialog dialog = new MessageDialog(e.Message);
                await dialog.ShowAsync();
            }
            

            return info;
        }

        public static bool IsIdValid(string id)
        {
            return id.Length == 11 || id.Length == 34;
        }

        static private WebRequest GetWebRequest(RequestTypes RequestType,string id)
        {
            string uri;

            switch (RequestType)
            {
                case RequestTypes.REQUEST_PLAYLIST:
                    uri = "https://www.googleapis.com/youtube/v3/playlistItems?part=contentDetails&maxResults=50&playlistId=" + id +"&key=" + API_KEY;
                    break;
                case RequestTypes.REQUEST_VIDEO:
                    uri = "https://www.googleapis.com/youtube/v3/videos?id=" + id + "&part=snippet&key=" + API_KEY;
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

        static async public void DownloadVideo(string url,string filename,string id)
        {
            try
            {
                HttpClientHandler aHandler = new HttpClientHandler();
                aHandler.ClientCertificateOptions = ClientCertificateOption.Automatic;
                HttpClient aClient = new HttpClient(aHandler);
                aClient.DefaultRequestHeaders.ExpectContinue = false;
                HttpResponseMessage response = await aClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead); // Important! ResponseHeadersRead.

                var audioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
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
                        break;

                    // Report progress.
                    totalBytesRead += buffer.Length;
                    PopulateUI.UpdateVideoDownloadProgress(id, (int)((100 * totalBytesRead) / totalBytes)); 

                    // Write to file.
                    await fs.WriteAsync(buffer);
                }
                inputStream.Dispose();
                fs.Dispose();

                //TagProcessing.SetAlbumTag(filename, "lolme");
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
            }

        }



    }
}