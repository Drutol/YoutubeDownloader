using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace YoutubeDownloader
{
    public class PageResultsFormatter : IValueConverter
    {
        public object Convert(object value, Type taretType, object parameter, string language)
        {
            if (value != null)
            {
                return System.Convert.ToInt32(value) == 31 ? "All" : value;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class ActionButonsVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool param = System.Convert.ToBoolean(parameter);
            bool val = System.Convert.ToBoolean(value);
            if (param && val )
                return "Visible";
            return "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class InvertVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var param = (Visibility)value;
            return param == Visibility.Visible ? "Collapsed" : "Visible";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
