using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace YoutubeDownloader
{
    class PopulateUI
    {
        public enum ProgressType
        {
            PROGRESS_DL,
            PROGRESS_CONV,
        }
        public static async void UpdateVideoManipulationProgress(string itemId,int progress,ProgressType type)
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    var frame = (Frame)Window.Current.Content;
                    var page = (MainPage)frame.Content;
                    foreach (VideoItem vidItem in page.vidListItems)
                    {
                        if (vidItem.id == itemId)
                        {
                            if (type == ProgressType.PROGRESS_DL)
                                vidItem.SetProgress(progress);
                            else if (type == ProgressType.PROGRESS_CONV)
                                vidItem.SetConvProgress(progress);
                            return;
                        }
                    }
                });
                


            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc.Message);

            }
        }
    }
}
