using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite
{
    public enum SortOrder
    {
        Name,
        LastActivity,
        Provider,
        Categories,
        Genres,
        ReleaseDate,
        Developers,
        Publishers,
        IsInstalled,
        Hidden,
        Favorite,
        InstallDirectory,
        Icon,
        Platform,
        Tags
    }

    public enum SortOrderDirection
    {
        Ascending,
        Descending
    }

    public enum GroupOrder
    {
        None,
        Provider,
        Category,
        Platform
    }

    public enum GameImageSize
    {
        Icon,
        Image
    }

    public enum ViewType : int
    {
        List = 0,
        Images = 1,
        Grid = 2
    }
}
