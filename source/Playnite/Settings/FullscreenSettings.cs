using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class FullscreenSettings : ObservableObject
    {
        private int columnCount = 5;
        public int ColumnCount
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

        private int rowCount = 2;
        public int RowCount
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

        private int detailsRowCount = 3;
        public int DetailsRowCount
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
