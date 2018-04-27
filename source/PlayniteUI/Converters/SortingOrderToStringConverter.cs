using Playnite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PlayniteUI
{
    public class SortingOrderToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var order = (SortOrder)value;
            switch (order)
            {
                case SortOrder.Name:
                    return ResourceProvider.Instance.FindString("LOCNameLabel");
                case SortOrder.LastActivity:
                    return ResourceProvider.Instance.FindString("LOCLastPlayedLabel");
                case SortOrder.Provider:
                case SortOrder.Categories:
                case SortOrder.Genres:
                case SortOrder.ReleaseDate:
                case SortOrder.Developers:
                case SortOrder.Publishers:
                case SortOrder.IsInstalled:
                case SortOrder.Hidden:
                case SortOrder.Favorite:
                case SortOrder.InstallDirectory:
                case SortOrder.Icon:
                case SortOrder.Platform:
                case SortOrder.Tags:
                case SortOrder.Playtime:
                    return ResourceProvider.Instance.FindString("LOCMostPlayedLabel");
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
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
