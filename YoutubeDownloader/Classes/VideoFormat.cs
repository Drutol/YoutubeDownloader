﻿using System;
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
        public static async Task<bool> VideoConvert(StorageFile file,MediaEncodingProfile mediaProfile,string id)
        {
            try
            {
                var outFolder = await Settings.GetOutputFolder();                                          
                var audioFile = await outFolder.CreateFileAsync(file.Name.Replace("mp4","mp3"), CreationCollisionOption.ReplaceExisting);

                MediaTranscoder transcoder = new MediaTranscoder();

                System.Diagnostics.Debug.WriteLine(file.Name);
                System.Diagnostics.Debug.WriteLine(audioFile.Name);

                var result = await transcoder.PrepareFileTranscodeAsync(file, audioFile, mediaProfile);
                
                if(result.CanTranscode)
                {
                    var transcodeOp = result.TranscodeAsync();
                    transcodeOp.Progress +=
                        new AsyncActionProgressHandler<double>((IAsyncActionWithProgress<double> asyncInfo, double percent) =>                 
                        {
                            PopulateUI.UpdateVideoDownloadProgress(id,(int)percent);
                        });
                    transcodeOp.Completed +=
                        new AsyncActionWithProgressCompletedHandler<double>( (IAsyncActionWithProgress<double> asyncInfo, AsyncStatus status) =>
                        {
                            Utils.TryToRemoveFile(5, file);
                        });
                }
                             
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Conversion : " + exc.Message);
                return false;
            }

            //Utils.TryToRemoveFile(5, file);
            return true;
        }
        public static async void VideoConvert(string fileName,MediaEncodingProfile mediaProfile,string id)
        {
            var outFolder = await Settings.GetOutputFolder();
            StorageFile file = await outFolder.GetFileAsync(fileName);
            await VideoConvert(file, mediaProfile, id);
        }

    }
}
