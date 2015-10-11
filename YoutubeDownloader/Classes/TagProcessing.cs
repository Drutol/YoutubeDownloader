using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Media.Transcoding;
using Windows.Media.MediaProperties;
using Windows.Media.Core;

namespace YoutubeDownloader
{
    class TagProcessing
    {
        public enum EditableTags
        {
            ALBUM,
            TITLE,
            ARTIST,
        }

        public static async void SetTag(StorageFile file, string value, EditableTags tagToEdit)
        {
            try
            {
                MusicProperties tags = await file.Properties.GetMusicPropertiesAsync();
                switch (tagToEdit)
                {
                    case EditableTags.ALBUM:
                        tags.Album = value;
                        break;
                    case EditableTags.TITLE:
                        tags.Title = value;
                        break;
                    case EditableTags.ARTIST:
                        tags.Artist = value;
                        break;
                    default:
                        break;
                }
                await tags.SavePropertiesAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("TagProcessingAlbum(StorageFile) " + exc.Message);
            }
        }
        public static async void SetTag(string fileName, string value, EditableTags tagToEdit)
        {
            StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
            SetTag(file, value, tagToEdit);
        }
    }
}
