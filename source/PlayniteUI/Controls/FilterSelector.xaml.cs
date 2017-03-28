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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Playnite;

namespace PlayniteUI.Controls
{
    public class FilterSelectorConfig
    {
        public GamesStats Stats
        {
            get; set;
        }

        public FilterSettings Settings
        {
            get; set;
        }

        public FilterSelectorConfig(GamesStats stats, FilterSettings settings)
        {
            Stats = stats;
            Settings = settings;
        }
    }

    /// <summary>
    /// Interaction logic for FilterSelector.xaml
    /// </summary>
    public partial class FilterSelector : UserControl
    {
        public FilterSelector()
        {
            InitializeComponent();
        }
    }
}
