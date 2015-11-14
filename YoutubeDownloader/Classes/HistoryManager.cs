using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace YoutubeDownloader
{
    public struct HistoryEntry
    {

        public string thumb;
        public string title;
        public string author;
        public string id;
        public bool playlist;

        public HistoryEntry(string thumb,string title,string author,string id)
        {
            this.thumb = thumb;
            this.title = title;
            this.author = author;
            this.id = id;
            playlist = id.Length == 11 ? false : true;
        }
    };

    static public class HistoryManager
    {
        public static void AddNewEntry(HistoryEntry info)
        {
            int counter = Settings.GetValueForSetting(Settings.PossibleValueSettings.HISTORY_COUNTER);
            if(counter > 5)
            {
                counter = 0;
                Settings.ChangeSetting("HistoryCounter", 0);
            }

            Settings.ChangeSetting("HistoryEntry" + counter, Newtonsoft.Json.JsonConvert.SerializeObject(info));
        }

        public static List<HistoryEntry> GetHistoryEntries()
        {
            List<HistoryEntry> list = new List<HistoryEntry>();
            for (int i = 0; i < 6; i++)
            {
                try
                {
                    string data = (string)ApplicationData.Current.LocalSettings.Values["HistoryEntry" + i];
                    if(data != null)
                        list.Add(Newtonsoft.Json.JsonConvert.DeserializeObject<HistoryEntry>(data));
                }
                catch (Exception) // data may be empty
                {
                    continue;
                }
            }
            return list;
        }
    }
}
