using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class WindowPosition
    {
        public class Point
        {
            public double X
            {
                get; set;
            }

            public double Y
            {
                get; set;
            }
        }

        public Point Position
        {
            get; set;
        }

        public Point Size
        {
            get; set;
        }

        public System.Windows.WindowState State
        {
            get; set;
        } = System.Windows.WindowState.Normal;
    }

    public class WindowPositions
    {
        public Dictionary<string, WindowPosition> Positions
        {
            get; set;
        } = new Dictionary<string, WindowPosition>();

        public WindowPositions()
        {
        }
    }
}
