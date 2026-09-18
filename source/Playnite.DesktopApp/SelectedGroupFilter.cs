using System;
using System.Collections.Generic;
using Playnite.SDK.Models;

namespace Playnite.DesktopApp
{
    internal static class SelectedGroupFilter
    {
        public static bool IsEntryVisible(FilterSettings settings, GroupableField grouping, Guid? entryId)
        {
            if (!settings.ShowSelectedGroupsOnly)
            {
                return true;
            }

            var selectedIds = GetSelectedIds(settings, grouping);
            return selectedIds == null || !entryId.HasValue || selectedIds.Contains(entryId.Value);
        }

        private static List<Guid> GetSelectedIds(FilterSettings settings, GroupableField grouping)
        {
            IdItemFilterItemProperties filter;
            switch (grouping)
            {
                case GroupableField.Category:
                    filter = settings.Category;
                    break;
                case GroupableField.Genre:
                    filter = settings.Genre;
                    break;
                case GroupableField.Developer:
                    filter = settings.Developer;
                    break;
                case GroupableField.Publisher:
                    filter = settings.Publisher;
                    break;
                case GroupableField.Tag:
                    filter = settings.Tag;
                    break;
                case GroupableField.Feature:
                    filter = settings.Feature;
                    break;
                case GroupableField.Platform:
                    filter = settings.Platform;
                    break;
                case GroupableField.Series:
                    filter = settings.Series;
                    break;
                case GroupableField.AgeRating:
                    filter = settings.AgeRating;
                    break;
                case GroupableField.Region:
                    filter = settings.Region;
                    break;
                default:
                    return null;
            }

            return filter?.Text.IsNullOrEmpty() == true && filter.Ids.HasItems() ? filter.Ids : null;
        }
    }
}
