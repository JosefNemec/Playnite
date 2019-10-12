using Playnite.Controls;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class RandomGameSelectWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new RandomGameSelectWindow();
        }
    }

    /// <summary>
    /// Interaction logic for RandomGameSelectWindow.xaml
    /// </summary>
    public partial class RandomGameSelectWindow : WindowBase
    {
        public RandomGameSelectWindow() : base()
        {
            InitializeComponent();
        }
    }
}
