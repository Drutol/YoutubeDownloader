using System;
using Windows.Storage;
using Windows.Storage.FileProperties;
using System.Threading.Tasks;

namespace YoutubeDownloader
{

    struct TagsPackage
    {
        public string artist;
        public string album;
        public string title;

        public TagsPackage(string artist,string album,string title)
        {
            this.artist = artist;
            this.album = album;
            this.title = title;
        }
    }

    class TagProcessing
    {
        public enum EditableTags
        {
            ALBUM,
            TITLE,
            ARTIST,
        }

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
                System.Diagnostics.Debug.WriteLine("TagsProcessing : " + exc.Message);
                await Task.Delay(TimeSpan.FromSeconds(5));
                if (nRetries >= 0)
                    SetTags(tagsPck, file, nRetries - 1);
               
            }
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
