using Playnite.Controls;
using Playnite.FullscreenApp.Controls;
using Playnite.FullscreenApp.ViewModels;
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
using System.Windows.Threading;

namespace Playnite.FullscreenApp.Windows
{
    public class ExtensionsMenuWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ExtensionsMenuWindow();
        }
    }

    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class ExtensionsMenuWindow : WindowBase
    {
        private object lastUpdatedSource = null;

        public ExtensionsMenuWindow() : base()
        {
            InitializeComponent();
            WindowTools.ConfigureChildWindow(this);
        }

        // This is practically the only place where we can refocus new items
        // afters virtualized panel finished making new buttons after items list change.
        private void PART_ItemsHost_LayoutUpdated(object sender, EventArgs e)
        {
            if (lastUpdatedSource == PART_ItemsHost.ItemsSource)
                return;

            lastUpdatedSource = PART_ItemsHost.ItemsSource;
            var button = ElementTreeHelper.FindVisualChildren<ButtonEx>(PART_ItemsHost).FirstOrDefault();
            if (button != null && !button.IsFocused)
                button.Focus();
        }
    }
}
