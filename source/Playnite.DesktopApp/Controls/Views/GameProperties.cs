using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls.Views
{
    public class GameProperties : Control
    {
        static GameProperties()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameProperties), new FrameworkPropertyMetadata(typeof(GameProperties)));
        }
    }
}
