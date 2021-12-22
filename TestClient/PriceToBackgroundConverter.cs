using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TestClient
{
    public class PriceToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush brush = Brushes.White;
            try
            {
                decimal price = (decimal)value;
                if (price > 0)
                {
                    brush = Brushes.Green;
                }
                else if (price < 0)
                {
                    brush = Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
