using Playnite.Controls;
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
    /// <summary>
    /// Interaction logic for CrashWindow.xaml
    /// </summary>
    public partial class CrashWindow : WindowBase
    {
        public CrashWindow() : base()
        {
            InitializeComponent();

            var model = FullscreenApplication.Current?.MainModel;
            if (model != null)
            {
                Width = model.WindowWidth;
                Height = model.WindowHeight;
            }

            Loaded += CrashWindow_Loaded;
        }

        private void CrashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ButtonGeneratePackage.Focus();
        }
    }
}
