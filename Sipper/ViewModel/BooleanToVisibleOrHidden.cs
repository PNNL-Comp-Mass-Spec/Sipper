using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Sipper.ViewModel
{
    /// <summary>
    /// Converter class to convert from True/False to Visible/Hidden
    /// Set "Collapse" to True to instead convert to Visible/Collapsed
    /// </summary>
    /// <remarks>From "http://www.rhyous.com/2011/02/22/binding-visibility-to-a-bool-value-in-wpf/"</remarks>
    public class BoolToVisibleOrHidden : IValueConverter
    {

        #region Properties

        public bool Collapse { get; set; }
        public bool Reverse { get; set; }

        #endregion

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bValue = (bool)value;

            if (bValue != Reverse)
            {
                return Visibility.Visible;
            }

            if (Collapse)
                return Visibility.Collapsed;
            return Visibility.Hidden;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibility = (Visibility)value;

            if (visibility == Visibility.Visible)
                return !Reverse;
            return Reverse;
        }
    }
}
