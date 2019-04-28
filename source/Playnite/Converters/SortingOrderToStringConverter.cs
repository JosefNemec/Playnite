using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class SortingOrderToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var order = (SortOrder)value;
            switch (order)
            {
                case SortOrder.Name:
                    return ResourceProvider.GetString("LOCNameLabel");
                case SortOrder.LastActivity:
                    return ResourceProvider.GetString("LOCLastPlayedLabel");
                case SortOrder.Library:
                case SortOrder.Categories:
                case SortOrder.Genres:
                case SortOrder.ReleaseDate:
                case SortOrder.Developers:
                case SortOrder.Publishers:
                case SortOrder.IsInstalled:
                case SortOrder.Hidden:
                case SortOrder.Favorite:
                case SortOrder.InstallDirectory:
                case SortOrder.Platform:
                case SortOrder.Tags:
                case SortOrder.Playtime:
                    return ResourceProvider.GetString("LOCMostPlayedLabel");
                case SortOrder.Added:
                case SortOrder.Modified:
                case SortOrder.PlayCount:
                case SortOrder.Series:
                case SortOrder.Version:
                case SortOrder.AgeRating:
                case SortOrder.Region:
                case SortOrder.Source:
                case SortOrder.CompletionStatus:
                case SortOrder.UserScore:
                case SortOrder.CriticScore:
                case SortOrder.CommunityScore:
                default:
                    return order.ToString();
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
