using Playnite;
using Playnite.MetaProviders;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace PlayniteUI
{
    public class CompletionStatusToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (CompletionStatus)value;
            var resources = new ResourceProvider();
            switch (source)
            {
                case CompletionStatus.NotPlayed:
                    return resources.FindString("LOCNotPlayed");
                case CompletionStatus.Played:
                    return resources.FindString("LOCPlayed");
                case CompletionStatus.Beaten:
                    return resources.FindString("LOCBeaten");
                case CompletionStatus.Completed:
                    return resources.FindString("LOCCompleted");
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
