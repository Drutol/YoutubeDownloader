using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeDownloader
{
    /// <summary>
    /// Singleton class.
    /// </summary>
    public class QueueManager
    {
        const int maxPararellDownloads = 4;

        private static QueueManager instance;

        private QueueManager() { }

        private List<VideoItem> queuedItems = new List<VideoItem>();
        private List<VideoItem> downloadingItems = new List<VideoItem>();

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
            while(queuedItems.Count > 0 && downloadingItems.Count <= maxPararellDownloads)
            {
                downloadingItems.Add(queuedItems.First());
                queuedItems.First().StartDownload(null,null);
                queuedItems.RemoveAt(0);
            }
        }

        public void DownloadCompleted(string id)
        {
            foreach (VideoItem item in downloadingItems)
            {
                if (item.id == id)
                {
                    downloadingItems.Remove(item);
                    break;
                }
            }
            CheckQueue();
        }


    }
}
