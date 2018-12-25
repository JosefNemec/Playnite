using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using Playnite;

namespace PlayniteUI
{
    public class NullableBoolToThreeStateFilterEnumConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (ThreeStateFilterEnum)value;
            switch (state)
            {
                case ThreeStateFilterEnum.EnableInclusive:
                    return null;
                case ThreeStateFilterEnum.EnableExclusive:
                    return true;
                case ThreeStateFilterEnum.Disable:
                    return false;
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var state = (bool?)value;
            switch (state)
            {
                case null:
                    return ThreeStateFilterEnum.EnableInclusive;
                case true:
                    return ThreeStateFilterEnum.EnableExclusive;
                case false:
                    return ThreeStateFilterEnum.Disable;
                default:
                    throw new InvalidCastException();
            }
        }
    }
}