using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Foundation;

namespace YoutubeDownloader
{
    public static class VideoFormat
    {
        public static async void VideoConvert(StorageFile file,MediaEncodingProfile mediaProfile,string id)
        {
            try
            {                                             
                var audioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(file.Name.Replace("mp4","mp3"), CreationCollisionOption.ReplaceExisting);

                MediaTranscoder transcoder = new MediaTranscoder();

                var result = await transcoder.PrepareFileTranscodeAsync(file, audioFile, mediaProfile);
                
                if(result.CanTranscode)
                {
                    var transcodeOp = result.TranscodeAsync();
                    transcodeOp.Progress +=
                        new AsyncActionProgressHandler<double>((IAsyncActionWithProgress<double> asyncInfo, double percent) =>                 
                        {
                            PopulateUI.UpdateVideoDownloadProgress(id,(int)percent);
                        });
                }             
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Conversion(string)" + exc.Message);
            }

            
        }
        public static async void VideoConvert(string fileName,MediaEncodingProfile mediaProfile,string id)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            VideoConvert(file, mediaProfile,id);
        }

    }
}
