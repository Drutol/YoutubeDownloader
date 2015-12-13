using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.UI.Core;
using TagLib;

namespace YoutubeDownloader
{

    struct TagsPackage
    {
        public string artist;
        public string album;
        public string title;
        public string ThumbPath;

        public TagsPackage(string artist,string album,string title,string thumb = null)
        {
            this.artist = artist;
            this.album = album;
            this.title = title;
            ThumbPath = thumb;
        }
    }

    internal static class TagProcessing
    {
        public static async void SetTags(TagsPackage tagsPck , StorageFile file, int nRetries = 5)
        {
            try
            {
                MusicProperties tags = await file.Properties.GetMusicPropertiesAsync();
                tags.Artist = tagsPck.artist;
                tags.Title = tagsPck.title;
                tags.Album = tagsPck.album;
                await tags.SavePropertiesAsync();
            }
            catch (Exception exc)
            {
                Debug.WriteLine("TagsProcessing : " + exc.Message);
                await Task.Delay(TimeSpan.FromSeconds(5));
                if (nRetries >= 0)
                    SetTags(tagsPck, file, nRetries - 1);            
            }
        }

        public static async void SetTagsSharp(TagsPackage tagsPck, StorageFile file, int nRetries = 5) //Using TagLibSharp
        {
            try
            {
                var fileStream = await file.OpenStreamForWriteAsync();

                var tagFile = TagLib.File.Create(new StreamFileAbstraction(file.Name, fileStream, fileStream));

                tagFile.Tag.Title = tagsPck.title;
                tagFile.Tag.Performers = new[] {tagsPck.artist};
                tagFile.Tag.Album = tagsPck.album;
                if (tagsPck.ThumbPath != null)
                {
                    StorageFile thumb = await StorageFile.GetFileFromPathAsync(tagsPck.ThumbPath);
                    var thumbStream = await thumb.OpenStreamForReadAsync();
                    TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame
                    {
                        TextEncoding = TagLib.StringType.Latin1,
                        MimeType = thumb.Name.Contains(".png") ? "image/png" : "image/jpeg",
                        Type = TagLib.PictureType.FrontCover,
                        Data = TagLib.ByteVector.FromFile(new StreamFileAbstraction("Cover", thumbStream, thumbStream))
                    };
                    tagFile.Tag.Pictures = new IPicture[1] {pic};
                    thumbStream.Dispose();
                }

                tagFile.Save();
                fileStream.Dispose();
                            
            }
            catch (Exception exc)
            {
                Debug.WriteLine("TagsProcessing : " + exc.Message);
                await Task.Delay(TimeSpan.FromSeconds(5));
                if (nRetries >= 0)
                    SetTagsSharp(tagsPck, file, nRetries - 1);
            }
        }
    }
}
