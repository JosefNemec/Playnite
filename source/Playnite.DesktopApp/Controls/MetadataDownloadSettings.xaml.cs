using Playnite.Metadata;
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

namespace Playnite.DesktopApp.Controls
{
    /// <summary>
    /// Interaction logic for MetadataDownloadSettings.xaml
    /// </summary>
    public partial class MetadataDownloadSettings : UserControl
    {
        public MetadataDownloaderSettings Settings
        {
            get
            {
                return (MetadataDownloaderSettings)GetValue(SettingsProperty);
            }

            set
            {
                SetValue(SettingsProperty, value);
            }
        }

        public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register(
            nameof(Settings),
            typeof(MetadataDownloaderSettings),
            typeof(MetadataDownloadSettings),
            new PropertyMetadata(new MetadataDownloaderSettings(), SettingsPropertyChangedCallback));

        private static void SettingsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        public MetadataDownloadSettings()
        {
            InitializeComponent();
        }
    }
}
