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
        string thumb;
        string title;
        string author;
        string url;
        int vidCount;

        HistoryEntry(string thumb,string title,string author,string url,int vidCount)
        {
            this.thumb = thumb;
            this.title = title;
            this.author = author;
            this.url = url;
            this.vidCount = vidCount;
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
    }
}
