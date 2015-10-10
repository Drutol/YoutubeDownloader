using System;
using System.Collections.Generic;
using System.IO;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Id3;
using TagLib;
using Windows.Media.Transcoding;
using Windows.Media.MediaProperties;
using Windows.Media.Core;

//using TagLib;

namespace YoutubeDownloader
{
    class TagProcessing
    {
        public static async void Convert(string filename)
        {
            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(filename);
                
                var audioFile = await ApplicationData.Current.LocalFolder.CreateFileAsync("yup.mp3", CreationCollisionOption.ReplaceExisting);

                MediaTranscoder transcoder = new MediaTranscoder();

                var result = await transcoder.PrepareFileTranscodeAsync(file, audioFile, MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High));
                await result.TranscodeAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Conversion:     " + exc.Message);
            }
        }


        public static async void SetAlbumTag(StorageFile file, string album)
        {
            try
            {
                var properties = new List<KeyValuePair<string, object>>();
                properties.Add(new KeyValuePair<string, object>("System.Music.AlbumTitle", "Updated Album Title 1"));
                await file.Properties.SavePropertiesAsync(properties);
                //MusicProperties tags = await file.Properties.GetMusicPropertiesAsync();
               // tags.Album = album;
                //await tags.SavePropertiesAsync();
            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Tag album " + exc.Message);
            }
        }
        public static async void SetAlbumTag(string fileName, string album)
        {

            try
            {
                StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

              // var properties = new List<KeyValuePair<string, object>>();
              //  properties.Add(new KeyValuePair<string, object>("System.Music.AlbumTitle", "Updated Album Title 1"));
               // await file.Properties.SavePropertiesAsync(properties);


                MusicProperties tags = await file.Properties.GetMusicPropertiesAsync();
                tags.Album = album;
                await tags.SavePropertiesAsync();

                // await System.Threading.Tasks.Task.Run(async () =>
                // {
                //StorageFile file = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);

                // DocumentProperties tags = await file.Properties.GetDocumentPropertiesAsync();
                //tags.Album = album;
                //tags.Album = "d";
                // tags.AlbumArtist = "d";
                //tags.Artist = "d";
                //tags.Publisher = "d";
                //tags.Rating = 5;
                //tags.Subtitle = "d";
                // tags.Title = "d";
                // tags.TrackNumber = 2;
                // tags.Year = 2000;

                // tags.Title = "lol";
                // await tags.SavePropertiesAsync();
                //List<KeyValuePair<string, object>> saveTags = new List<KeyValuePair<string, object>>();
                //saveTags.Add(new KeyValuePair<string, object>("System.Music.AlbumTitle", album));
                //SystemMusicProperties systag = new SystemMusicProperties
                // });


                //System.Diagnostics.Debug.WriteLine(file.Path);
                //System.Diagnostics.Debug.WriteLine(tags.Artist);
                //System.Diagnostics.Debug.WriteLine(tags.Album);
                //var lol = await tags.RetrievePropertiesAsync(new List<string>() { "Album" });
                //foreach (var sth in lol)
                //{
                //    System.Diagnostics.Debug.WriteLine(sth.Key + " " + sth.Value);
                //}

                //Dictionary<string, object> lol1 = new Dictionary<string, object>();
                //lol1.Add("System.Music.AlbumTitle", album);



                //await System.Threading.Tasks.Task.Run( () =>
                //{

                //    //var fs = await file.OpenAsync(FileAccessMode.ReadWrite);
                //    //Mp3Stream lol = new Mp3Stream(fs.AsStream(),Mp3Permissions.ReadWrite);

                //    //Id3Tag newTag = lol.GetTag(Id3TagFamily.Version2x);

                //    //newTag.Album.Value = album;
                //    //newTag.Title.Value = "lol";

                //    //lol.WriteTag(newTag,WriteConflictAction.Replace);
                //});

                //System.Diagnostics.Debug.WriteLine(tag.Artists);



                //var fs = await file.OpenStreamForWriteAsync();

                // Create the file if it does not exist.


                //System.IO.FileAttributes attributes = File.SetAttributes(file.Path, FileAttributes.)

                //await System.Threading.Tasks.Task.Run(async () =>
                // {
                //     var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite);
                //     //var saveFileStream = await file.OpenStreamForWriteAsync();
                //     var stream = fileStream.AsStream();
                //     TagLib.File tagFile = TagLib.Mpeg4.FileParser.Create(new StreamFileAbstraction(file.Name, stream, stream));

                //     tagFile.Tag.Album = album;                 
                //     tagFile.Save();
                // });

            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("Tag album " + exc.Message);
            }
        }
    }
}
