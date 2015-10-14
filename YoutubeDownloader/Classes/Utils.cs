using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

    }
}
