using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.DesktopApp.ViewModels;
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
        internal readonly ViewType viewType;
        internal readonly DesktopAppViewModel mainModel;

        internal Control ControlGameView;
        internal ExtendedListBox ListGames;

        public BaseGamesView(ViewType viewType) : this(viewType, DesktopApplication.Current?.MainModel)
        {
        }

        public BaseGamesView(ViewType viewType, DesktopAppViewModel mainModel)
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
                if (ListGames != null)
                {
                    SetListGamesBinding();
                }
            }
        }

        private void SetListGamesBinding()
        {
            if (mainModel.AppSettings.ViewSettings.GamesViewType == viewType ||
                DesignerProperties.GetIsInDesignMode(this))
            {
                BindingTools.SetBinding(ListGames,
                    ExtendedListBox.ItemsSourceProperty,
                    mainModel,
                    $"{nameof(mainModel.GamesView)}.{nameof(DesktopCollectionView.CollectionView)}");
            }
            else
            {
                ListGames.ItemsSource = null;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ControlGameView = Template.FindName("PART_ControlGameView", this) as Control;
            if (ControlGameView != null)
            {
                BindingTools.SetBinding(ControlGameView,
                    Control.DataContextProperty,
                    mainModel,
                    nameof(DesktopAppViewModel.SelectedGameDetails));
            }

            ListGames = Template.FindName("PART_ListGames", this) as ExtendedListBox;
            if (ListGames != null)
            {
                SetListGamesBinding();
                BindingTools.SetBinding(ListGames,
                    ExtendedListBox.SelectedItemProperty,
                    mainModel,
                    nameof(DesktopAppViewModel.SelectedGame),
                    BindingMode.TwoWay);
                BindingTools.SetBinding(ListGames,
                    ExtendedListBox.SelectedItemsListProperty,
                    mainModel,
                    nameof(DesktopAppViewModel.SelectedGamesBinder),
                    BindingMode.TwoWay);

                ScrollToSelectedBehavior.SetEnabled(ListGames, true);

                ListGames.InputBindings.Add(new KeyBinding(mainModel.EditSelectedGamesCommand, mainModel.EditSelectedGamesCommand.Gesture));
                ListGames.InputBindings.Add(new KeyBinding(mainModel.RemoveSelectedGamesCommand, mainModel.RemoveSelectedGamesCommand.Gesture));
                ListGames.InputBindings.Add(new KeyBinding(mainModel.StartSelectedGameCommand, mainModel.StartSelectedGameCommand.Gesture));
            }
        }
    }
}