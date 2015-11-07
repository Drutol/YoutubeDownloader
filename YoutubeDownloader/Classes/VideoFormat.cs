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
        public static async void VideoConvert(StorageFile file,MediaEncodingProfile mediaProfile,string id,VideoItem caller)
        {
            try
            {
                var outFolder = await Settings.GetOutputFolder();
                var audioFile = await outFolder.CreateFileAsync(file.Name.Replace("mp4","mp3"), CreationCollisionOption.ReplaceExisting);

                MediaTranscoder transcoder = new MediaTranscoder();

                var result = await transcoder.PrepareFileTranscodeAsync(file, audioFile, mediaProfile);
                
                if(result.CanTranscode)
                {
                    var transcodeOp = result.TranscodeAsync();
                    transcodeOp.Progress +=
                        new AsyncActionProgressHandler<double>((IAsyncActionWithProgress<double> asyncInfo, double percent) =>
                        {
                            PopulateUI.UpdateVideoManipulationProgress(id, (int)percent, PopulateUI.ProgressType.PROGRESS_CONV);
                        });
                    transcodeOp.Completed +=
                        new AsyncActionWithProgressCompletedHandler<double>( (IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status) =>
                        {
                            QueueManager.Instance.ConvCompleted(id);                     
                            Utils.TryToRemoveFile(5, file);
                        });
                }

                TagProcessing.SetTags(new TagsPackage(caller.tagArtist, caller.tagAlbum, caller.tagTitle), audioFile);
                //if (Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_AUTO_RENAME))
                //    await outFile.RenameAsync(caller.tagTitle);

            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Conversion : " + exc.Message);
                throw;
            }
        }
        public static async void VideoConvert(string fileName,MediaEncodingProfile mediaProfile,string id,VideoItem caller)
        {
            var outFolder = await Settings.GetOutputFolder();
            StorageFile file = await outFolder.GetFileAsync(fileName);
            VideoConvert(file, mediaProfile, id,caller);
        }

    }
}
