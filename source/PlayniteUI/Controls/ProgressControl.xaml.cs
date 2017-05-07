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

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        public string Text
        {
            get
            {
                return TextProgressDescription.Dispatcher.Invoke(() => TextProgressDescription.Text);
            }

            set
            {
                TextProgressDescription.Dispatcher.Invoke(() => TextProgressDescription.Text = value);
            }
        }

        public double ProgressMin
        {
            get
            {
                return ProgressBarBar.Dispatcher.Invoke(() => ProgressBarBar.Minimum);
            }

            set
            {
                ProgressBarBar.Dispatcher.Invoke(() => ProgressBarBar.Minimum = value);
            }
        }

        public double ProgressMax
        {
            get
            {
                return ProgressBarBar.Dispatcher.Invoke(() => ProgressBarBar.Maximum);

            }

            set
            {
                ProgressBarBar.Dispatcher.Invoke(() => ProgressBarBar.Maximum = value);
            }
        }

        public double ProgressValue
        {
            get
            {
                return ProgressBarBar.Dispatcher.Invoke(() => ProgressBarBar.Value);
            }

            set
            {
                ProgressBarBar.Dispatcher.Invoke(() => ProgressBarBar.Value = value);
            }
        }

        public Visibility Visible
        {
            get
            {
                return Dispatcher.Invoke(() => Visibility);
            }

            set
            {
                Dispatcher.Invoke(() => Visibility = value);
            }
        }

        public ProgressControl()
        {
            InitializeComponent();
        }
    }
}
