using Playnite;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class CompletionStatusToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((CompletionStatus)value)
            {
                case CompletionStatus.NotPlayed:
                    return ResourceProvider.GetString("LOCNotPlayed");
                case CompletionStatus.Played:
                    return ResourceProvider.GetString("LOCPlayed");
                case CompletionStatus.Beaten:
                    return ResourceProvider.GetString("LOCBeaten");
                case CompletionStatus.Completed:
                    return ResourceProvider.GetString("LOCCompleted");
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
