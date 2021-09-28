using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Converters;
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
    [TemplatePart(Name = "PART_ElemMainMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_PanelSideBarItems", Type = typeof(Panel))]
    public class Sidebar : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private Panel PanelSideBarItems;
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
            Loaded += Sidebar_Loaded;
            Unloaded += Sidebar_Unloaded;
        }

        private void Sidebar_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.SidebarItems.CollectionChanged += SidebarItems_CollectionChanged;
        }

        private void Sidebar_Unloaded(object sender, RoutedEventArgs e)
        {
            mainModel.SidebarItems.CollectionChanged -= SidebarItems_CollectionChanged;
        }

        private void SidebarItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            LoadSidebarItems();
        }

        public void LoadSidebarItems()
        {
            if (PanelSideBarItems == null)
            {
                return;
            }

            PanelSideBarItems.Children.Clear();
            foreach (var sideItem in mainModel.SidebarItems)
            {
                PanelSideBarItems.Children.Add(new SidebarItem { DataContext = sideItem });
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PanelSideBarItems = Template.FindName("PART_PanelSideBarItems", this) as Panel;
            if (PanelSideBarItems != null)
            {
                LoadSidebarItems();
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
                ElemMainMenu.ContextMenu.SetResourceReference(ContextMenu.StyleProperty, "TopPanelMenu");
                BindingTools.SetBinding(ElemMainMenu,
                    FrameworkElement.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.ShowMainMenuOnTopPanel),
                    converter: new InvertedBooleanToVisibilityConverter());
            }
        }
    }
}
