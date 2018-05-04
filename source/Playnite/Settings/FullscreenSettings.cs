using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class FullscreenSettings : ObservableObject
    {
        private long columnCount = 5;
        public long ColumnCount
        {
            get
            {
                return columnCount;
            }

            set
            {
                columnCount = value;
                OnPropertyChanged("ColumnCount");
            }
        }

        private long rowCount = 2;
        public long RowCount
        {
            get
            {
                return rowCount;
            }

            set
            {
                rowCount = value;
                OnPropertyChanged("RowCount");
            }
        }

        private long detailsRowCount = 3;
        public long DetailsRowCount
        {
            get
            {
                return detailsRowCount;
            }

            set
            {
                detailsRowCount = value;
                OnPropertyChanged("DetailsRowCount");
            }
        }
    }
}
