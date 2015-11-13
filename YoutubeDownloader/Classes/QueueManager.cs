using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeDownloader
{
    /// <summary>
    /// Singleton class that manages queueing items (DL and Conv).
    /// </summary>
    public class QueueManager
    {
        private int maxPararellDownloads = Settings.GetValueForSetting(Settings.PossibleValueSettings.SETTINGS_PARARELL_DL);
        private int maxPararellConv = Settings.GetValueForSetting(Settings.PossibleValueSettings.SETTINGS_PARARELL_CONV);

        private static QueueManager instance;

        private QueueManager() { }

        private List<VideoItem> queuedItems = new List<VideoItem>();
        private List<VideoItem> downloadingItems = new List<VideoItem>();

        private List<VideoItem> queuedItemsConv = new List<VideoItem>();
        private List<VideoItem> convertingItems = new List<VideoItem>();

        public static QueueManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new QueueManager();
                }
                return instance;
            }
        }

        #region Download

        public void QueueNewItem(VideoItem item)
        {
            queuedItems.Add(item);
            CheckQueue();
        }

        public void ForceDownload(VideoItem item)
        {
            item.StartDownload(null, null);
            downloadingItems.Add(item);
        }

        private void CheckQueue()
        {
            if (queuedItems.Count > 0 && downloadingItems.Count < maxPararellDownloads)
            {
                var item = queuedItems.First();
                downloadingItems.Add(item);
                item.StartDownload(null, null);
                queuedItems.RemoveAt(0);
            }
           
        }

        public void DownloadCompleted(string id)
        {
            foreach (VideoItem item in downloadingItems)
            {
                if (item.id == id)
                {
                    System.Diagnostics.Debug.WriteLine("Downloaded " + id);
                    downloadingItems.Remove(item);
                    break;
                }
            }
            CheckQueue();
        }

        public void MaxPararellDownloadChanged(int to)
        {
            maxPararellDownloads = to;
        }
        #endregion

        #region Conversion


        public void QueueNewItemConv(VideoItem item)
        {
            queuedItemsConv.Add(item);
            CheckQueueConv();
        }

        private void CheckQueueConv()
        {
            if (queuedItemsConv.Count > 0 && convertingItems.Count < maxPararellConv)
            {
                var item = queuedItemsConv.First();
                convertingItems.Add(item);
                VideoFormat.VideoConvert(Utils.CleanFileName(item.title + item.fileFormat), Settings.GetPrefferedEncodingProfile(),item);
                queuedItemsConv.RemoveAt(0);
            }

        }

        public void ConvCompleted(string id)
        {
            foreach (VideoItem item in convertingItems)
            {
                if (item.id == id)
                {
                    System.Diagnostics.Debug.WriteLine("Converted " + id);
                    convertingItems.Remove(item);
                    break;
                }
            }
            CheckQueueConv();
        }

        public void MaxPararellConvChanged(int to)
        {
            maxPararellConv = to;
        }
        #endregion


    }
}
