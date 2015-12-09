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
    }
}
