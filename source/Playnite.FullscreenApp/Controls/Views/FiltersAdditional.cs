using Playnite.Behaviors;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_MenuHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ButtonBack", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ItemsHost", Type = typeof(ItemsControl))]
    public class FiltersAdditional : Control, IDisposable
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement MenuHost;
        private ButtonBase ButtonBack;
        private ItemsControl ItemsHost;

        static FiltersAdditional()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FiltersAdditional), new FrameworkPropertyMetadata(typeof(FiltersAdditional)));
        }

        public FiltersAdditional() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public FiltersAdditional(FullscreenAppViewModel mainModel) : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public void Dispose()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                MenuHost = Template.FindName("PART_MenuHost", this) as FrameworkElement;
                if (MenuHost != null)
                {
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.CloseAdditionalFiltersCommand, new KeyGesture(Key.Back)));
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.CloseAdditionalFiltersCommand, new KeyGesture(Key.Escape)));
                    MenuHost.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleFiltersCommand, Key = Key.F });
                    MenuHost.InputBindings.Add(new XInputBinding(mainModel.CloseAdditionalFiltersCommand, XInputButton.B));
                }

                ButtonBack = Template.FindName("PART_ButtonBack", this) as ButtonBase;
                if (ButtonBack != null)
                {
                    ButtonBack.Command = mainModel.CloseAdditionalFiltersCommand;
                    BindingTools.SetBinding(ButtonBack,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.FilterAdditionalPanelVisible));
                }

                ItemsHost = Template.FindName("PART_ItemsHost", this) as ItemsControl;
                if (ItemsHost != null)
                {
                    AssignFilter("LOCGenreLabel", "PART_ButtonGenre", GameField.Genres, nameof(FilterSettings.Genre));
                    AssignFilter("LOCGameReleaseYearTitle", "PART_ButtonReleaseYear", GameField.ReleaseYear, nameof(FilterSettings.ReleaseYear));
                    AssignFilter("LOCDeveloperLabel", "PART_ButtonDeveloper", GameField.Developers, nameof(FilterSettings.Developer));
                    AssignFilter("LOCPublisherLabel", "PART_ButtonPublisher", GameField.Publishers, nameof(FilterSettings.Publisher));
                    AssignFilter("LOCFeatureLabel", "PART_ButtonFeature", GameField.Features, nameof(FilterSettings.Feature));
                    AssignFilter("LOCTagLabel", "PART_ButtonTag", GameField.Tags, nameof(FilterSettings.Tag));
                    AssignFilter("LOCTimePlayed", "PART_ButtonPlayTime", GameField.Playtime, nameof(FilterSettings.PlayTime));
                    AssignFilter("LOCCompletionStatus", "PART_ButtonCompletionStatus", GameField.CompletionStatus, nameof(FilterSettings.CompletionStatus));
                    AssignFilter("LOCSeriesLabel", "PART_ButtonSeries", GameField.Series, nameof(FilterSettings.Series));
                    AssignFilter("LOCRegionLabel", "PART_ButtonRegion", GameField.Region, nameof(FilterSettings.Region));
                    AssignFilter("LOCSourceLabel", "PART_ButtonSource", GameField.Source, nameof(FilterSettings.Source));
                    AssignFilter("LOCAgeRatingLabel", "PART_ButtonAgeRating", GameField.AgeRating, nameof(FilterSettings.AgeRating));
                    AssignFilter("LOCUserScore", "PART_ButtonUserScore", GameField.UserScore, nameof(FilterSettings.UserScore));
                    AssignFilter("LOCCommunityScore", "PART_ButtonCommunityScore", GameField.CommunityScore, nameof(FilterSettings.CommunityScore));
                    AssignFilter("LOCCriticScore", "PART_ButtonCriticScore", GameField.CriticScore, nameof(FilterSettings.CriticScore));
                    AssignFilter("LOCGameLastActivityTitle", "PART_ButtonLastActivity", GameField.LastActivity, nameof(FilterSettings.LastActivity));
                    AssignFilter("LOCAddedLabel", "PART_ButtonAdded", GameField.Added, nameof(FilterSettings.Added));
                    AssignFilter("LOCModifiedLabel", "PART_ButtonModified", GameField.Modified, nameof(FilterSettings.Modified));
                }
            }
        }

        private void AssignFilter(string title, string partId, GameField field, string bindBased)
        {
            var button = new ButtonEx();
            button.SetResourceReference(ButtonBase.ContentProperty, title);
            button.SetResourceReference(ButtonBase.MarginProperty, "ItemMargin");
            button.SetResourceReference(ButtonBase.StyleProperty, "ButtonFilterNagivation");
            button.Command = mainModel.LoadSubFilterCommand;
            button.CommandParameter = field;
            BindingTools.SetBinding(
                button,
                ButtonBase.TagProperty,
                mainModel.AppSettings.Fullscreen.FilterSettings,
                $"{bindBased}.{nameof(FilterItemProperites.IsSet)}");

            ItemsHost.Items.Add(button);
        }
    }
}
