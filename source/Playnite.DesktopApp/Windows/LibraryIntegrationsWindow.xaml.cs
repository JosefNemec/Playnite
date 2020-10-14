using Playnite.Controls;
using Playnite.ViewModels;
using Playnite.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Windows
{
    public class LibraryIntegrationsWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new LibraryIntegrationsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class LibraryIntegrationsWindow : WindowBase
    {
        public LibraryIntegrationsWindow() : base()
        {
            InitializeComponent();
        }
    }
}
