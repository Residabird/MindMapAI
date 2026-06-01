using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MindMapAI.ViewModels
{
    public class LastTagVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 3) return Visibility.Visible;

            var item = values[0];
            if (item == null) return Visibility.Collapsed;

            if (values[2] is IList list)
            {
                int index = list.IndexOf(item);
                return index == list.Count - 1 ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
