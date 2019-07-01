using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK.Models;
using Playnite.ViewModels.Desktop.DesignData;
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
using System.Xml.Linq;
using BooleanToVisibilityConverter = Playnite.Converters.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ElemPlayTime", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLastPlayed", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCompletionStatus", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLibrary", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemPlatform", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemGenres", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemDevelopers", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemPublishers", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemReleaseDate", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemCategories", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemTags", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemLinks", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemDescription", Type = typeof(FrameworkElement))]

    [TemplatePart(Name = "PART_TextPlayTime", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextLastActivity", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextCompletionStatus", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ButtonLibrary", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonPlatform", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonReleaseDate", Type = typeof(Button))]

    [TemplatePart(Name = "PART_ItemsGenres", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsDevelopers", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsPublishers", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsCategories", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsTags", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ItemsLinks", Type = typeof(ItemsControl))]

    [TemplatePart(Name = "PART_ButtonPlayAction", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonContextAction", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonMoreActions", Type = typeof(Button))]
    [TemplatePart(Name = "PART_HtmlDescription", Type = typeof(HtmlTextView))]
    [TemplatePart(Name = "PART_ImageCover", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ImageIcon", Type = typeof(Image))]
    public class GameOverview : Control
    {
        private readonly DesktopAppViewModel mainModel;

        private FrameworkElement ElemPlayTime;
        private FrameworkElement ElemLastPlayed;
        private FrameworkElement ElemCompletionStatus;
        private FrameworkElement ElemLibrary;
        private FrameworkElement ElemPlatform;
        private FrameworkElement ElemGenres;
        private FrameworkElement ElemDevelopers;
        private FrameworkElement ElemPublishers;
        private FrameworkElement ElemReleaseDate;
        private FrameworkElement ElemCategories;
        private FrameworkElement ElemTags;
        private FrameworkElement ElemLinks;
        private FrameworkElement ElemDescription;

        private TextBlock TextPlayTime;
        private TextBlock TextLastActivity;
        private TextBlock TextCompletionStatus;
        private Button ButtonLibrary;
        private Button ButtonPlatform;
        private Button ButtonReleaseDate;
        private ItemsControl ItemsGenres;
        private ItemsControl ItemsDevelopers;
        private ItemsControl ItemsPublishers;
        private ItemsControl ItemsCategories;
        private ItemsControl ItemsTags;
        private ItemsControl ItemsLinks;

        private Button ButtonPlayAction;
        private Button ButtonContextAction;
        private Button ButtonMoreActions;
        private HtmlTextView HtmlDescription;
        private Image ImageCover;
        private Image ImageIcon;

        static GameOverview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameOverview), new FrameworkPropertyMetadata(typeof(GameOverview)));
        }

        public GameOverview() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public GameOverview(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = new DesignMainViewModel();
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ButtonPlayAction = Template.FindName("PART_ButtonPlayAction", this) as Button;
            if (ButtonPlayAction != null)
            {
                BindingTools.SetBinding(ButtonPlayAction,
                    Button.CommandProperty,
                    nameof(GameDetailsViewModel.PlayCommand));
                BindingTools.SetBinding(ButtonPlayAction,
                    Button.ContentProperty,
                    nameof(GameDetailsViewModel.ContextActionDescription));
                BindingTools.SetBinding(ButtonPlayAction,
                    Button.VisibilityProperty,
                    nameof(GameDetailsViewModel.IsPlayAvailable),
                    converter: new BooleanToVisibilityConverter());
            }

            ButtonContextAction = Template.FindName("PART_ButtonContextAction", this) as Button;
            if (ButtonContextAction != null)
            {
                BindingTools.SetBinding(ButtonContextAction,
                    Button.CommandProperty,
                    nameof(GameDetailsViewModel.ContextActionCommand));
                BindingTools.SetBinding(ButtonContextAction,
                    Button.ContentProperty,
                    nameof(GameDetailsViewModel.ContextActionDescription));
                BindingTools.SetBinding(ButtonContextAction,
                    Button.VisibilityProperty,
                    nameof(GameDetailsViewModel.IsContextAvailable),
                    converter: new BooleanToVisibilityConverter());
            }

            ButtonMoreActions = Template.FindName("PART_ButtonMoreActions", this) as Button;
            if (ButtonMoreActions != null)
            {
                LeftClickContextMenuBehavior.SetEnabled(ButtonMoreActions, true);
                ButtonMoreActions.ContextMenu = new GameMenu()
                {
                    ShowStartSection = false,
                    Placement = PlacementMode.Bottom
                };
                BindingTools.SetBinding(ButtonMoreActions.ContextMenu,
                    Button.DataContextProperty,
                    mainModel,
                    nameof(DesktopAppViewModel.SelectedGame));
            }

            HtmlDescription = Template.FindName("PART_HtmlDescription", this) as HtmlTextView;
            if (HtmlDescription != null)
            {
                BindingTools.SetBinding(HtmlDescription,
                    HtmlTextView.HtmlTextProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.Description)));
                BindingTools.SetBinding(HtmlDescription,
                    HtmlTextView.VisibilityProperty,
                    nameof(GameDetailsViewModel.DescriptionVisibility));
            }

            ImageCover = Template.FindName("PART_ImageCover", this) as Image;
            if (ImageCover != null)
            {
                BindingTools.SetBinding(ImageCover,
                    Image.SourceProperty,
                    GetGameBindingPath(nameof(GamesCollectionViewEntry.CoverImageObject)),
                    converter: new NullToDependencyPropertyUnsetConverter());
                BindingTools.SetBinding(ImageCover,
                    Image.VisibilityProperty,
                    nameof(GameDetailsViewModel.CoverVisibility));
            }

            ImageIcon = Template.FindName("PART_ImageIcon", this) as Image;
            if (ImageIcon != null)
            {
                var sourceBinding = new PriorityBinding();
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(GetGameBindingPath(nameof(GamesCollectionViewEntry.IconObject))),
                    Converter = new NullToDependencyPropertyUnsetConverter()
                });
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(GetGameBindingPath(nameof(GamesCollectionViewEntry.DefaultIconObject))),
                    Converter = new NullToDependencyPropertyUnsetConverter()
                });

                BindingOperations.SetBinding(ImageIcon, Image.SourceProperty, sourceBinding);
            }

            SetElemVisibility(ref ElemPlayTime, "PART_ElemPlayTime", nameof(GameDetailsViewModel.PlayTimeVisibility));
            SetElemVisibility(ref ElemLastPlayed, "PART_ElemLastPlayed", nameof(GameDetailsViewModel.LastPlayedVisibility));
            SetElemVisibility(ref ElemCompletionStatus, "PART_ElemCompletionStatus", nameof(GameDetailsViewModel.CompletionStatusVisibility));
            SetElemVisibility(ref ElemLibrary, "PART_ElemLibrary", nameof(GameDetailsViewModel.SourceLibraryVisibility));
            SetElemVisibility(ref ElemPlatform, "PART_ElemPlatform", nameof(GameDetailsViewModel.PlatformVisibility));
            SetElemVisibility(ref ElemGenres, "PART_ElemGenres", nameof(GameDetailsViewModel.GenreVisibility));
            SetElemVisibility(ref ElemDevelopers, "PART_ElemDevelopers", nameof(GameDetailsViewModel.DeveloperVisibility));
            SetElemVisibility(ref ElemPublishers, "PART_ElemPublishers", nameof(GameDetailsViewModel.PublisherVisibility));
            SetElemVisibility(ref ElemReleaseDate, "PART_ElemReleaseDate", nameof(GameDetailsViewModel.ReleaseDateVisibility));
            SetElemVisibility(ref ElemTags, "PART_ElemCategories", nameof(GameDetailsViewModel.CategoryVisibility));
            SetElemVisibility(ref ElemCategories, "PART_ElemTags", nameof(GameDetailsViewModel.TagVisibility));
            SetElemVisibility(ref ElemLinks, "PART_ElemLinks", nameof(GameDetailsViewModel.LinkVisibility));
            SetElemVisibility(ref ElemDescription, "PART_ElemDescription", nameof(GameDetailsViewModel.DescriptionVisibility));

            SetGameItemButtonBinding(ref ButtonLibrary, "PART_ButtonLibrary",
                nameof(GameDetailsViewModel.SetLibraryFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.PluginId)),
                GetGameBindingPath($"{nameof(GamesCollectionViewEntry.LibraryPlugin)}.{nameof(GamesCollectionViewEntry.LibraryPlugin.Name)}"),
                nameof(GameDetailsViewModel.SourceLibraryVisibility));

            SetGameItemButtonBinding(ref ButtonPlatform, "PART_ButtonPlatform",
                nameof(GameDetailsViewModel.SetPlatformFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.Platform)),
                GetGameBindingPath($"{nameof(GamesCollectionViewEntry.Platform)}.{nameof(GamesCollectionViewEntry.Platform.Name)}"),
                nameof(GameDetailsViewModel.PlatformVisibility));
            
            SetGameItemButtonBinding(ref ButtonReleaseDate, "PART_ButtonReleaseDate",
                nameof(GameDetailsViewModel.SetReleaseDateFilterCommand),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.ReleaseDate)),
                GetGameBindingPath(nameof(GamesCollectionViewEntry.ReleaseDate)),
                nameof(GameDetailsViewModel.ReleaseDateVisibility),
                new NullableDateToStringConverter());

            SetGameItemTextBinding(ref TextPlayTime, "PART_TextPlayTime",
                nameof(GameDetailsViewModel.Game.Playtime),
                nameof(GameDetailsViewModel.PlayTimeVisibility),
                new LongToTimePlayedConverter());

            SetGameItemTextBinding(ref TextLastActivity, "PART_TextLastActivity",
                nameof(GameDetailsViewModel.Game.LastActivity),
                nameof(GameDetailsViewModel.LastPlayedVisibility),
                new DateTimeToLastPlayedConverter());

            SetGameItemTextBinding(ref TextCompletionStatus, "PART_TextCompletionStatus",
                nameof(GameDetailsViewModel.Game.CompletionStatus),
                nameof(GameDetailsViewModel.CompletionStatusVisibility),
                new ObjectToStringConverter());

            SetItemsControlBinding(ref ItemsGenres, "PART_ItemsGenres",
                nameof(GameDetailsViewModel.SetGenreFilterCommand),
                nameof(GamesCollectionViewEntry.Genres));

            SetItemsControlBinding(ref ItemsDevelopers, "PART_ItemsDevelopers",
                nameof(GameDetailsViewModel.SetDeveloperFilterCommand),
                nameof(GamesCollectionViewEntry.Developers));

            SetItemsControlBinding(ref ItemsPublishers, "PART_ItemsPublishers",
                nameof(GameDetailsViewModel.SetPublisherFilterCommand),
                nameof(GamesCollectionViewEntry.Publishers));

            SetItemsControlBinding(ref ItemsCategories, "PART_ItemsCategories",
                nameof(GameDetailsViewModel.SetCategoryFilterCommand),
                nameof(GamesCollectionViewEntry.Categories));

            SetItemsControlBinding(ref ItemsTags, "PART_ItemsTags",
                nameof(GameDetailsViewModel.SetTagFilterCommand),
                nameof(GamesCollectionViewEntry.Tags));

            SetItemsControlBinding(ref ItemsLinks, "PART_ItemsLinks",
                nameof(GameDetailsViewModel.OpenLinkCommand),
                nameof(GamesCollectionViewEntry.Links),
                nameof(Link.Url));
        }

        private void SetItemsControlBinding(ref ItemsControl elem, string partId, string command, string listSource, string tooltip = null)
        {
            elem = Template.FindName(partId, this) as ItemsControl;
            if (elem != null)
            {
                elem.ItemTemplate = GetFieldItemTemplate(command, tooltip);
                BindingTools.SetBinding(elem,
                    ItemsControl.ItemsSourceProperty,
                    GetGameBindingPath(listSource));
            }
        }

        private void SetElemVisibility(ref FrameworkElement elem, string partId, string binding)
        {
            elem = Template.FindName(partId, this) as FrameworkElement;
            if (elem != null)
            {                
                BindingTools.SetBinding(elem,
                    FrameworkElement.VisibilityProperty,
                    binding);
            }
        }

        private void SetGameItemTextBinding(ref TextBlock text, string partId, string textContent, string visibility, IValueConverter converter = null)
        {
            text = Template.FindName(partId, this) as TextBlock;
            if (text != null)
            {
                BindingTools.SetBinding(text,
                    TextBlock.TextProperty,
                    GetGameBindingPath(textContent),
                    converter: converter);
                BindingTools.SetBinding(text,
                    TextBlock.VisibilityProperty,
                    visibility);
            }
        }

        private void SetGameItemButtonBinding(ref Button button, string partId, string command, string commandParameter, string content, string visibility, IValueConverter contentConverter = null)
        {
            button = Template.FindName(partId, this) as Button;
            if (button != null)
            {
                BindingTools.SetBinding(button,
                    Button.CommandProperty,
                    command);
                BindingTools.SetBinding(button,
                    Button.CommandParameterProperty,
                    commandParameter);
                BindingTools.SetBinding(button,
                    Button.ContentProperty,
                    content,
                    converter: contentConverter);
                BindingTools.SetBinding(button,
                    Button.VisibilityProperty,
                    visibility);
            }
        }

        private string GetGameBindingPath(string path)
        {
            return $"{nameof(GameDetailsViewModel.Game)}.{path}";
        }

        private DataTemplate GetFieldItemTemplate(string command, string tooltip = null)
        {
            XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var buttonElem = new XElement(pns + nameof(Button),
                new XAttribute("Command", $"{{Binding DataContext.{command}, RelativeSource={{RelativeSource AncestorType=ItemsControl}}}}"),
                new XAttribute("CommandParameter", "{Binding}"),
                new XAttribute("Content", "{Binding Name}"),
                new XAttribute("Style", "{StaticResource PropertyItemButton}"));

            if (!tooltip.IsNullOrEmpty())
            {
                buttonElem.Add(new XAttribute("ToolTip", $"{{Binding {tooltip}}}"));
            }

            var templateDoc = new XDocument(
                new XElement(pns + nameof(DataTemplate), buttonElem)
            );

            return Xaml.FromString<DataTemplate>(templateDoc.ToString());
        }

    }
}
