using Playnite.Behaviors;
using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ListSideBarItems", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ElemMainMenu", Type = typeof(FrameworkElement))]
    public class Sidebar : Control
    {
        private readonly DesktopAppViewModel mainModel;

        private ItemsControl ListSideBarItems;
        private FrameworkElement ElemMainMenu;

        static Sidebar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Sidebar), new FrameworkPropertyMetadata(typeof(Sidebar)));
        }

        public Sidebar() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public Sidebar(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            DataContext = this.mainModel;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ListSideBarItems = Template.FindName("PART_ListSideBarItems", this) as ItemsControl;
            if (ListSideBarItems != null)
            {
                BindingTools.SetBinding(ListSideBarItems,
                    ItemsControl.ItemsSourceProperty,
                    mainModel,
                    nameof(DesktopAppViewModel.AppViewItems));
            }

            ElemMainMenu = Template.FindName("PART_ElemMainMenu", this) as FrameworkElement;
            if (ElemMainMenu != null)
            {
                LeftClickContextMenuBehavior.SetEnabled(ElemMainMenu, true);
                ElemMainMenu.ContextMenu = new MainMenu(mainModel)
                {
                    StaysOpen = false,
                    Placement = PlacementMode.Bottom
                };
            }
        }
    }
}
