using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls
{
    public class TopPanelItem : Button
    {
        public bool IsToggled
        {
            get => (bool)GetValue(IsToggledProperty);
            set => SetValue(IsToggledProperty, value);
        }

        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register(
            nameof(IsToggled),
            typeof(bool),
            typeof(TopPanelItem));

        static TopPanelItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TopPanelItem), new FrameworkPropertyMetadata(typeof(TopPanelItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BindingTools.SetBinding(this,
                Button.CommandProperty,
                nameof(TopPanelWrapperItem.Command));
            BindingTools.SetBinding(this,
                ContentPresenter.ContentProperty,
                nameof(TopPanelWrapperItem.IconObject));
            BindingTools.SetBinding(this,
                ContentPresenter.VisibilityProperty,
                nameof(TopPanelWrapperItem.Visible),
                converter: new BooleanToVisibilityConverter());
            BindingTools.SetBinding(this,
                ContentPresenter.ToolTipProperty,
                nameof(TopPanelWrapperItem.Title));
        }
    }
}
