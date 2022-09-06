using Playnite.Common;
using Playnite.Database;
using Playnite.Extensions.Markup;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.ViewModels
{
    public class SidebarWrapperItem : ObservableObject
    {
        public enum SourceType
        {
            Builtin,
            Extension,
            SoftwareTool
        }

        private static ILogger logger = LogManager.GetLogger();
        private DesktopAppViewModel model;
        public RelayCommand<object> Command { get; set; }

        public SidebarItem SideItem { get; }

        public SiderbarItemType Type
        {
            get => SideItem.Type;
        }

        public string Title
        {
            get => SideItem.Title;
        }

        public Thickness IconPadding
        {
            get => SideItem.IconPadding;
            set { }
        }

        public double ProgressValue
        {
            get => SideItem.ProgressValue;
            set { }
        }

        public double ProgressMaximum
        {
            get => SideItem.ProgressMaximum;
            set { }
        }

        public bool Visible
        {
            get => SideItem.Visible;
            set { }
        }

        public object IconObject => SdkHelpers.ResolveUiItemIcon(SideItem.Icon);

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                if (SideItem.Type == SiderbarItemType.Button)
                {
                    return;
                }

                if (selected != value && value == false)
                {
                    if (SideItem.Closed != null)
                    {
                        SideItem.Closed();
                    }
                }

                selected = value;
                OnPropertyChanged();
            }
        }

        public SidebarWrapperItem(SidebarItem item, DesktopAppViewModel model)
        {
            this.model = model;
            SideItem = item;
            Command = new RelayCommand<object>(Activation);
            item.PropertyChanged += Item_PropertyChanged;
        }

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        private void Activation(object arg)
        {
            if (Selected)
            {
                return;
            }

            if (SideItem.Type == SiderbarItemType.Button)
            {
                try
                {
                    SideItem.Activated();
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to activate sidebar item.");
                }
            }
            else
            {
                Control view = null;
                if (SideItem.Opened != null)
                {
                    view = SideItem.Opened();
                    if (view == null)
                    {
                        return;
                    }
                }

                model.SidebarItems.ForEach(a =>
                {
                    if (a.Selected)
                    {
                        a.Selected = false;
                    }
                });

                Selected = true;
                model.ActiveView = view;
            }
        }
    }

    public class MainSidebarViewItem : SidebarItem
    {
        private Control view;
        private DesktopAppViewModel model;

        public ApplicationView AppView { get; }

        public MainSidebarViewItem(Control view, DesktopAppViewModel model, ApplicationView appView)
        {
            this.view = view;
            this.AppView = appView;
            this.model = model;
            Type = SiderbarItemType.View;
            Opened = () =>
            {
                if (AppView == ApplicationView.Statistics)
                {
                    model.LibraryStats.Calculate();
                }

                return view;
            };
        }
    }

    public class SoftwareToolSidebarItem : SidebarItem
    {
        private AppSoftware app;
        private DesktopAppViewModel model;

        public SoftwareToolSidebarItem(AppSoftware app, DesktopAppViewModel model)
        {
            this.app = app;
            this.model = model;
            Type = SiderbarItemType.Button;
            Icon = app.Icon;
            Title = app.Name;
            Activated = () => model.StartSoftwareToolCommand.Execute(app);
        }
    }

    public partial class DesktopAppViewModel
    {
        private ObservableCollection<SidebarWrapperItem> sidebarItems = new ObservableCollection<SidebarWrapperItem>();
        public ObservableCollection<SidebarWrapperItem> SidebarItems
        {
            get => sidebarItems;
            set
            {
                sidebarItems = value;
                OnPropertyChanged();
            }
        }

        public void LoadSideBarItems()
        {
            libraryView = new Controls.Views.Library(this);
            statsView = new Controls.LibraryStatistics(LibraryStats);

            var libraryItem = new MainSidebarViewItem(libraryView, this, ApplicationView.Library)
            {
                Icon = "SidebarLibraryIcon",
                Title = Resources.GetString(LOC.Library)
            };

            var statsItem = new MainSidebarViewItem(statsView, this, ApplicationView.Statistics)
            {
                Icon = "SidebarStatisticsIcon",
                Title = Resources.GetString(LOC.Statistics)
            };

            var sideItems = new List<SidebarWrapperItem>();
            foreach (var plugin in Extensions.Plugins)
            {
                try
                {
                    var items = plugin.Value.Plugin.GetSidebarItems().ToList();
                    if (items.HasItems())
                    {
                        items.ForEach(a => sideItems.Add(new SidebarWrapperItem(a, this)));
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    Logger.Error(e, $"Failed to GetSidebarItems, from {plugin.Value.Description.Name}");
                }
            }

            sideItems = sideItems.OrderByDescending(a => a.SideItem.Type).ThenBy(a => a.SideItem.Title).ToList();
            sideItems.Insert(0, new SidebarWrapperItem(statsItem, this));
            sideItems.Insert(0, new SidebarWrapperItem(libraryItem, this));
            sideItems[0].Command.Execute(null);
            SidebarItems.AddRange(sideItems);
        }

        public void LoadSoftwareToolsSidebarItems()
        {
            if (!Database.IsOpen)
            {
                return;
            }

            Database.SoftwareApps.ItemUpdated += (_, __) => RefreshSoftwareToolsSidebarItems();
            Database.SoftwareApps.ItemCollectionChanged += (_, __) => RefreshSoftwareToolsSidebarItems();
            foreach (var tool in Database.SoftwareApps.OrderBy(a => a.Name))
            {
                if (tool.ShowOnSidebar)
                {
                    SidebarItems.Add(new SidebarWrapperItem(new SoftwareToolSidebarItem(tool, this), this));
                }
            }
        }

        public void RefreshSoftwareToolsSidebarItems()
        {
            SidebarItems.Where(a => a.SideItem is SoftwareToolSidebarItem).ToList().ForEach(a => SidebarItems.Remove(a));
            foreach (var tool in Database.SoftwareApps.OrderBy(a => a.Name))
            {
                if (tool.ShowOnSidebar)
                {
                    SidebarItems.Add(new SidebarWrapperItem(new SoftwareToolSidebarItem(tool, this), this));
                }
            }
        }
    }
}
