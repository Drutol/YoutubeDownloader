using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.MediaProperties;
using Windows.Media.Transcoding;
using Windows.Storage;
using Windows.Foundation;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace YoutubeDownloader
{
    public static class VideoFormat
    {
        public static async void VideoConvert(StorageFile file,MediaEncodingProfile mediaProfile,VideoItem caller)
        {
            try
            {
                var outFolder = await Settings.GetOutputFolder();
                StorageFile audioFile;
                if (Settings.GetBoolSettingValueForKey(Settings.PossibleSettingsBool.SETTING_AUTO_RENAME) && caller.tagTitle != "")
                    audioFile = await outFolder.CreateFileAsync(caller.tagTitle+"."+mediaProfile.Container.Subtype.ToLower(), CreationCollisionOption.ReplaceExisting);
                else
                    audioFile = await outFolder.CreateFileAsync(file.Name.Replace("mp4", mediaProfile.Container.Subtype.ToLower()), CreationCollisionOption.ReplaceExisting);

                MediaTranscoder transcoder = new MediaTranscoder();

                if (caller.trimEnd != null)
                    transcoder.TrimStartTime = new TimeSpan(0, 0, 0, (int)caller.trimStart, 0);
                if (caller.trimEnd != null)
                    transcoder.TrimStopTime = new TimeSpan(0, 0, 0, (int)caller.trimEnd, 0);

                var result = await transcoder.PrepareFileTranscodeAsync(file, audioFile, mediaProfile);
               
                if(result.CanTranscode)
                {
                    var transcodeOp = result.TranscodeAsync();
                    transcodeOp.Progress +=
                        new AsyncActionProgressHandler<double>(async (IAsyncActionWithProgress<double> asyncInfo, double percent) =>
                        {
                            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                            {
                                caller.SetConvProgress((int)percent);
                            });
                        });
                    transcodeOp.Completed +=
                        new AsyncActionWithProgressCompletedHandler<double>( (IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status) =>
                        {
                            QueueManager.Instance.ConvCompleted(caller.id);                     
                            Utils.TryToRemoveFile(5, file);
                            TagProcessing.SetTags(new TagsPackage(caller.tagArtist, caller.tagAlbum, caller.tagTitle), audioFile);
                        });
                }            
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Conversion : " + exc.Message);
            }
        }
        public static async void VideoConvert(string fileName,MediaEncodingProfile mediaProfile,VideoItem caller)
        {
            var outFolder = await Settings.GetOutputFolder();
            StorageFile file = await outFolder.GetFileAsync(fileName);
            VideoConvert(file, mediaProfile,caller);
        }

    }
}
