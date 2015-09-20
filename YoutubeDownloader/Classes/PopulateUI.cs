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
        //public async void HandleException(string msg)
        //{
        //    MessageDialog dialog = new MessageDialog("LOL");
        //    await dialog.ShowAsync();
        //}
        public static async void UpdateVideoDownloadProgress(string itemId,int progress)
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
                            vidItem.SetProgress(progress);
                            return;
                        }
                    }
                });
                


            }
            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine("upd" + exc.Message);

            }
        }
    }
}
