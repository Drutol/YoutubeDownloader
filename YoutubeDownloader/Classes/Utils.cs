using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;

namespace YoutubeDownloader
{
    public static class Utils
    {
        ///<summary>
        ///Removes all illegal chacters from filename so it can be saved.
        ///</summary>
        public static string CleanFileName(string fileName)
        {
            return Path.GetInvalidFileNameChars().Aggregate(fileName, (current, c) => current.Replace(c.ToString(), string.Empty));
        }
        /// <summary>
        /// Tries to delete file n times , each try is postponed by 5 seconds.
        /// </summary>
        /// <param name="nRetries"></param>
        /// <param name="file"></param>
        public static void TryToRemoveFile(int nRetries,StorageFile file)
        {
            Task.Run( async () =>
            {
                try
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                catch (Exception)
                {
                    if (nRetries > 0)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5));
                        TryToRemoveFile(nRetries - 1,file);
                    }            
                }
            });
        }

        //Snippet from https://github.com/flagbug/YoutubeExtractor
        public static string TryNormalizeYoutubeUrl(string url)
        {
            url = url.Trim();

            url = url.Replace("youtu.be/", "youtube.com/watch?v=");
            url = url.Replace("www.youtube", "youtube");
            url = url.Replace("youtube.com/embed/", "youtube.com/watch?v=");

            if (url.Contains("/v/"))
            {
                url = "http://youtube.com" + new Uri(url).AbsolutePath.Replace("/v/", "/watch?v=");
            }

            url = url.Replace("/watch#", "/watch?");


            if (url.Contains("?"))
            {
                url = url.Substring(url.IndexOf('?') + 1);
            }

            var dictionary = new Dictionary<string, string>();

            foreach (string vp in Regex.Split(url, "&"))
            {
                string[] strings = Regex.Split(vp, "=");
                dictionary.Add(strings[0], strings.Length == 2 ? System.Net.WebUtility.UrlDecode(strings[1]) : string.Empty);
            }

            if (dictionary.ContainsKey("list"))
                return dictionary["list"];
            else if (dictionary.ContainsKey("v"))
                return dictionary["v"];

            return "";
        }

    }
}
