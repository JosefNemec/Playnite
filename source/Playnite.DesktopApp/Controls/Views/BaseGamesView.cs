using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ListGames", Type = typeof(ExtendedListBox))]
    [TemplatePart(Name = "PART_ControlGameView", Type = typeof(Control))]
    public abstract class BaseGamesView : Control
    {
        internal readonly DesktopView viewType;
        internal readonly DesktopAppViewModel mainModel;

        internal Control ControlGameView;
        internal ExtendedListBox ListGames;

        public BaseGamesView(DesktopView viewType) : this(viewType, DesktopApplication.Current?.MainModel)
        {
        }

        public BaseGamesView(DesktopView viewType, DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            this.viewType = viewType;
            Loaded += BaseGamesView_Loaded;
            Unloaded += BaseGamesView_Unloaded;
        }

        private void BaseGamesView_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
        }

        private void BaseGamesView_Unloaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.ViewSettings.PropertyChanged -= ViewSettings_PropertyChanged;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewSettings.GamesViewType))
            {
                SetViewActiveBindings();
            }
        }

        private void SetViewActiveBindings()
        {
            if (mainModel.AppSettings.ViewSettings.GamesViewType == viewType ||
                DesignerProperties.GetIsInDesignMode(this))
            {
                if (ListGames != null)
                {
                    BindingTools.SetBinding(ListGames,
                        ExtendedListBox.ItemsSourceProperty,
                        mainModel,
                        $"{nameof(mainModel.GamesView)}.{nameof(DesktopCollectionView.CollectionView)}");
                    BindingTools.SetBinding(ListGames,
                        ExtendedListBox.SelectedItemsListProperty,
                        mainModel,
                        nameof(DesktopAppViewModel.SelectedGamesBinder),
                        BindingMode.TwoWay);
                }

                if (ControlGameView != null)
                {
                    BindingTools.SetBinding(ControlGameView,
                        Control.DataContextProperty,
                        mainModel,
                        nameof(DesktopAppViewModel.SelectedGameDetails),
                        mode: BindingMode.OneWay);
                }
            }
            else
            {
                if (ListGames != null)
                {
                    BindingTools.ClearBinding(ListGames, ExtendedListBox.SelectedItemsListProperty);
                    ListGames.ItemsSource = null;
                }

                if (ControlGameView != null)
                {
                    ControlGameView.DataContext = null;
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ControlGameView = Template.FindName("PART_ControlGameView", this) as Control;

            ListGames = Template.FindName("PART_ListGames", this) as ExtendedListBox;
            if (ListGames != null)
            {
                ScrollToSelectedBehavior.SetEnabled(ListGames, true);

                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    ListGames.InputBindings.Add(new KeyBinding(mainModel.EditSelectedGamesCommand, mainModel.EditSelectedGamesCommand.Gesture));
                    ListGames.InputBindings.Add(new KeyBinding(mainModel.RemoveSelectedGamesCommand, mainModel.RemoveSelectedGamesCommand.Gesture));
                    ListGames.InputBindings.Add(new KeyBinding(mainModel.StartSelectedGameCommand, mainModel.StartSelectedGameCommand.Gesture));
                }

                ListGames.SelectionMode = SelectionMode.Extended;
                VirtualizingPanel.SetCacheLengthUnit(ListGames, VirtualizationCacheLengthUnit.Item);
                VirtualizingPanel.SetCacheLength(ListGames, new VirtualizationCacheLength(5));
                VirtualizingPanel.SetScrollUnit(ListGames, ScrollUnit.Pixel);
                VirtualizingPanel.SetIsVirtualizingWhenGrouping(ListGames, true);
                VirtualizingPanel.SetVirtualizationMode(ListGames, VirtualizationMode.Recycling);

                SetViewActiveBindings();
            }

            ControlTemplateTools.InitializePluginControls(
                mainModel.Extensions,
                Template,
                this,
                SDK.ApplicationMode.Desktop,
                mainModel,
                $"{nameof(DesktopAppViewModel.SelectedGameDetails)}.{nameof(GameDetailsViewModel.Game)}.{nameof(GameDetailsViewModel.Game.Game)}");
        }
    }
}