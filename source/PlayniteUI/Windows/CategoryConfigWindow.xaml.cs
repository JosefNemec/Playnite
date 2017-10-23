using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Controls;
using System.ComponentModel;
using Playnite;

namespace PlayniteUI.Windows
{
    public class CategoryConfigWindowFactory : WindowFactory
    {
        public static CategoryConfigWindowFactory Instance
        {
            get => new CategoryConfigWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new CategoryConfigWindow();
        }
    }

    /// <summary>
    /// Interaction logic for CategoryConfigWindow.xaml
    /// </summary>
    public partial class CategoryConfigWindow : WindowBase
    {
        public CategoryConfigWindow()
        {
            InitializeComponent();
        }
    }
}
