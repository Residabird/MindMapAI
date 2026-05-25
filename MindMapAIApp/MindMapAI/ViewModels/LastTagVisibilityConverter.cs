using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MindMapAI.ViewModels
{
         public class LastTagVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Если элемент последний в коллекции — скрываем разделитель "/"
            if (value == null)
                return Visibility.Collapsed;

            // Этот конвертер получает PreviousData, поэтому если оно null — значит элемент первый
            return value == null ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
