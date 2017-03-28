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
        Activity
    }

    public enum GroupOrder
    {
        None,
        Store,
        Category
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
