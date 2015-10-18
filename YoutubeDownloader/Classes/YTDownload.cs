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
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Threading.Tasks;

namespace YoutubeDownloader
{

    enum RequestTypes
    {
        REQUEST_PLAYLIST,
        REQUEST_VIDEO,
    }

    public enum IdType
    {
        TYPE_PLAYLIST,
        TYPE_VIDEO,
        INVALID,
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

        public static IdType IsIdValid(string id)
        {
            if(id.Length != 11 && id.Length != 34) return IdType.INVALID;

            if (id.Length == 11) return IdType.TYPE_VIDEO;
            else return IdType.TYPE_PLAYLIST;
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

        static async public void DownloadVideo(string url,string filename,string id,VideoItem caller = null)
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
                        break;

                    // Report progress.
                    totalBytesRead += buffer.Length;
                    PopulateUI.UpdateVideoManipulationProgress(id, (int)((100 * totalBytesRead) / totalBytes),PopulateUI.ProgressType.PROGRESS_DL); 

                    // Write to file.
                    await fs.WriteAsync(buffer);
                }
                inputStream.Dispose();
                fs.Dispose();

                StorageFile outFile = await VideoFormat.VideoConvert(audioFile, Settings.GetPrefferedEncodingProfile() ,id);
                TagProcessing.SetTags(new TagsPackage(caller.tagArtist, caller.tagAlbum, caller.tagTitle),outFile);
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);
            }

        }

        public static async void DownloadThumbnails(Dictionary<string,string> urls)
        {
            foreach (KeyValuePair<string,string> url in urls)
            {
                try
                {
                    var folder = await Settings.GetOutputFolder();
                    var thumb = await folder.CreateFileAsync(url.Key+".bmp", CreationCollisionOption.ReplaceExisting);


                    var img = await GetBytesFromImage(url.Value);

                    await FileIO.WriteBytesAsync(thumb, img);
                }
                catch (Exception exc)
                {
                    System.Diagnostics.Debug.WriteLine(exc.Message);
                }
            }
        }

        private static async Task<byte[]> GetBytesFromImage(string URL)
        {
            byte[] srcPixels;
            var uri = new Uri(URL);
            var streamRef = RandomAccessStreamReference.CreateFromUri(uri);

            using (IRandomAccessStreamWithContentType fileStream = await streamRef.OpenReadAsync())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(fileStream);
                PixelDataProvider pixelProvider = await decoder.GetPixelDataAsync();
                srcPixels = pixelProvider.DetachPixelData();
            }

            return srcPixels;
        }


    }
}