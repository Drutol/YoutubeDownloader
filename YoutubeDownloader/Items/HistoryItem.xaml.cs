using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace YoutubeDownloader
{
    public sealed partial class HistoryItem : UserControl
    {
        HistoryEntry myInfo;

        public HistoryItem(HistoryEntry info)
        {
            myInfo = info;
            InitializeComponent();
            Task.Run(() =>
            {
                PopulateInfo();
            });
            
        }

        private async void LoadEntry(object sender, RoutedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
               // var frame = (Frame)Window.Current.Content;
               // var page = (Pages.DownloadPage)frame.Content;
                //page.BeginWork(myInfo.id);
            });
        }

        private async void OpenURL(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(myInfo.playlist ? "https://www.youtube.com/playlist?list=" + myInfo.id : "https://www.youtube.com/watch?v=" + myInfo.id));
        }

        private async void PopulateInfo()
        {
            try
            {
                await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                     {
                         Thumb.Source = new BitmapImage(new Uri(myInfo.thumb));
                         Title.Text = myInfo.title;
                         Author.Text = myInfo.author;
                         if (!myInfo.playlist)
                             PlaylistMark.Visibility = Visibility.Collapsed;
                     });
            }
            catch (Exception exc)
            {
                Debug.WriteLine(exc.Message);
            }
        }

        private void ShowFlyout(object sender, TappedRoutedEventArgs e)
        {
            Flyout.ShowAt(this);
        }
    }


}
