using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using TagLib;

namespace YoutubeDownloader
{

    struct TagsPackage
    {
        public readonly string artist;
        public readonly string album;
        public readonly string title;
        public readonly  string ThumbSource;

        public TagsPackage(string artist,string album,string title, string thumb = null)
        {
            this.artist = artist;
            this.album = album;
            this.title = title;
            ThumbSource = thumb;
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
                if (tagsPck.ThumbSource != null)
                {

                    TagLib.Id3v2.AttachedPictureFrame pic = new TagLib.Id3v2.AttachedPictureFrame
                    {
                        TextEncoding = StringType.Latin1,
                        Type = PictureType.FrontCover
                    };
                    var uri = new Uri(tagsPck.ThumbSource);         
                    if (!uri.IsFile)
                    {
                        var rass = RandomAccessStreamReference.CreateFromUri(uri);
                        IRandomAccessStream stream = await rass.OpenReadAsync();
                        pic.Data = ByteVector.FromStream(stream.AsStream());
                        pic.MimeType = "image/jpeg";
                        stream.Dispose();
                    }
                    else
                    {
                        StorageFile thumb = await StorageFile.GetFileFromPathAsync(tagsPck.ThumbSource);
                        var thumbStream = await thumb.OpenStreamForReadAsync();
                        pic.Data = ByteVector.FromFile(new StreamFileAbstraction("Cover", thumbStream, thumbStream));
                        pic.MimeType = thumb.Name.Contains(".png") ? "image/png" : "image/jpeg";
                        thumbStream.Dispose();
                    }

                    tagFile.Tag.Pictures = new IPicture[1] {pic};
                    
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
