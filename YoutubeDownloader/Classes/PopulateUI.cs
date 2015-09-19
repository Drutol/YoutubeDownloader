using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;

namespace YoutubeDownloader
{
    class PopulateUI
    {
        public async void HandleException(string msg)
        {
            MessageDialog dialog = new MessageDialog("LOL");
            await dialog.ShowAsync();
        }

        public void CreateVideoEntry()
        {
            
        }
    }
}
