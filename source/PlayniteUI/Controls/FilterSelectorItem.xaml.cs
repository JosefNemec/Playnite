using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace PlayniteUI.Controls
{
    public class FilterSelectorItemAutomationPeer : FrameworkElementAutomationPeer, IValueProvider, IToggleProvider
    {
        private FilterSelectorItem OwnerControl
        {
            get
            {
                return (FilterSelectorItem)Owner;
            }
        }

        public string Value
        {
            get
            {
                return OwnerControl.CountText;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public System.Windows.Automation.ToggleState ToggleState
        {
            get
            {
                if (OwnerControl.IsChecked)
                {
                    return System.Windows.Automation.ToggleState.On;
                }
                else
                {
                    return System.Windows.Automation.ToggleState.Off;
                }
            }
        }

        public FilterSelectorItemAutomationPeer(FilterSelectorItem owner) : base(owner) { }

        protected override string GetClassNameCore()
        {
            return "FilterSelectorItem";
        }
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Custom;
        }

        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
            {
                return this;
            }

            if (patternInterface == PatternInterface.Toggle)
            {
                return this;
            }

            return base.GetPattern(patternInterface);
        }

        public void SetValue(string value)
        {
            throw new NotImplementedException();
        }

        public void Toggle()
        {
            OwnerControl.IsChecked = !OwnerControl.IsChecked;
        }
    }

    /// <summary>
    /// Interaction logic for FilterSelectorItem.xaml
    /// </summary>
    public partial class FilterSelectorItem : UserControl
    {
        public string CountText
        {
            get
            {
                return (string)GetValue(CountTextProperty);
            }

            set
            {
                SetValue(CountTextProperty, value);
            }
        }

        public static readonly DependencyProperty CountTextProperty = DependencyProperty.Register(
            "CountText",
            typeof(string),
            typeof(FilterSelectorItem),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(FilterSelectorItem),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsChecked
        {
            get
            {
                return (bool)GetValue(IsCheckedProperty);
            }

            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(bool),
            typeof(FilterSelectorItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public FilterSelectorItem()
        {
            InitializeComponent();
        }
        
        private void MainControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }

        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new FilterSelectorItemAutomationPeer(this);
        }
    }
}
