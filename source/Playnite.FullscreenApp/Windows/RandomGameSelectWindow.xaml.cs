using Playnite.Controls;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Playnite.FullscreenApp.Windows
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
        public RandomGameSelectWindow()
        {
            InitializeComponent();
            this.Loaded += RandomGameSelectWindow_Loaded;

            var model = FullscreenApplication.Current?.MainModel;
            if (model != null)
            {
                Width = model.WindowWidth;
                Height = model.WindowHeight;
            }
        }

        private void RandomGameSelectWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ButtonPlay.Focus();
        }
    }
}
