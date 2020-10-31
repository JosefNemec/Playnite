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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.ViewModels
{
    public class SidebarWrapperItem : ObservableObject
    {
        private DesktopAppViewModel model;
        private SidebarItem sideItem;
        public RelayCommand<object> Command { get; set; }

        public string Name
        {
            get => sideItem.Title;
        }

        public Thickness IconPadding
        {
            get => sideItem.IconPadding;
            set
            { }
        }

        public double ProgressValue
        {
            get => sideItem.ProgressValue;
            set
            { }
        }

        public double ProgressMaximum
        {
            get => sideItem.ProgressMaximum;
            set
            { }
        }

        public object ImageObject
        {
            get
            {
                var icon = sideItem.Icon;
                if (icon == null)
                {
                    return null;
                }

                if (icon is string stringIcon)
                {
                    var resource = ResourceProvider.GetResource(stringIcon);
                    if (resource != null)
                    {
                        if (resource is BitmapImage bitmap)
                        {
                            var image = new System.Windows.Controls.Image() { Source = bitmap };
                            RenderOptions.SetBitmapScalingMode(image, RenderOptions.GetBitmapScalingMode(bitmap));
                            return image;
                        }
                        else if (resource is TextBlock textIcon)
                        {
                            var text = new TextBlock
                            {
                                Text = textIcon.Text,
                                FontFamily = textIcon.FontFamily,
                                FontStyle = textIcon.FontStyle
                            };

                            if (textIcon.ReadLocalValue(TextBlock.ForegroundProperty) != DependencyProperty.UnsetValue)
                            {
                                text.Foreground = textIcon.Foreground;
                            }

                            return text;
                        }
                    }
                    else if (System.IO.File.Exists(stringIcon))
                    {
                        return BitmapExtensions.BitmapFromFile(stringIcon)?.ToImage();
                    }
                    else
                    {
                        var themeFile = ThemeFile.GetFilePath(stringIcon);
                        if (themeFile != null)
                        {
                            return Images.GetImageFromFile(themeFile, BitmapScalingMode.Fant, double.NaN, double.NaN);
                        }

                        var dbFile = model.Database.GetFileAsImage(stringIcon);
                        if (dbFile != null)
                        {
                            return dbFile.ToImage();
                        }
                    }
                }
                else
                {
                    return icon;
                }

                return null;
            }
        }

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                if (sideItem.Type == SiderbarItemType.Button)
                {
                    return;
                }

                if (selected != value && value == false)
                {
                    sideItem.Closed();
                }

                selected = value;
                OnPropertyChanged();
            }
        }

        public SidebarWrapperItem(SidebarItem item, DesktopAppViewModel model)
        {
            this.model = model;
            sideItem = item;
            Command = new RelayCommand<object>(Activation);
        }

        private void Activation(object arg)
        {
            if (Selected)
            {
                return;
            }

            if (sideItem.Type == SiderbarItemType.Button)
            {
                sideItem.Activated();
            }
            else
            {
                var view = sideItem.Opened();
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
        private ApplicationView appView;
        private DesktopAppViewModel model;
        public override Control Opened()
        {
            model.AppSettings.CurrentApplicationView = appView;
            if (appView == ApplicationView.Statistics)
            {
                model.LibraryStats.Calculate();
            }

            return view;
        }

        public override void Closed()
        {
        }

        public MainSidebarViewItem(Control view, DesktopAppViewModel model, ApplicationView appView)
        {
            this.view = view;
            this.appView = appView;
            this.model = model;
            Type = SiderbarItemType.View;
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
        }

        public override void Activated()
        {
            model.StartSoftwareToolCommand.Execute(app);
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

            SidebarItems.Add(new SidebarWrapperItem(libraryItem, this));
            SidebarItems.Add(new SidebarWrapperItem(statsItem, this));
            SidebarItems[0].Command.Execute(null);

            foreach (var plugin in Extensions.Plugins)
            {
                // TODO catch exceptions
                var items = plugin.Value.Plugin.GetSidebarItems();
                if (items.HasItems())
                {
                    items.ForEach(a => SidebarItems.Add(new SidebarWrapperItem(a, this)));
                }
            }
        }

        public void LoadSoftwareToolsSidebarItems()
        {
            if (!Database.IsOpen)
            {
                return;
            }

            // TODO update when lists changes
            foreach (var tool in Database.SoftwareApps)
            {
                if (tool.ShowOnSidebar)
                {
                    SidebarItems.Add(new SidebarWrapperItem(new SoftwareToolSidebarItem(tool, this), this));
                }
            }
        }
    }
}
