using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using Windows.UI.Popups;

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



    }
}