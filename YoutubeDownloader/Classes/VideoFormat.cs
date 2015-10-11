using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;

namespace YoutubeDownloader
{
    public static class VideoFormat
    {
        public static async void VideoConvert(StorageFile file,MediaEncodingProfile mediaProfile)
        {
            try
            {              
                var audioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("yup.mp3", CreationCollisionOption.ReplaceExisting);

                MediaTranscoder transcoder = new MediaTranscoder();

                var result = await transcoder.PrepareFileTranscodeAsync(file, audioFile, mediaProfile);
                await result.TranscodeAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Conversion(string)" + exc.Message);
            }
        }

        public static async void VideoConvert(string fileName,MediaEncodingProfile mediaProfile)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            VideoConvert(file, mediaProfile);
        }

    }
}
