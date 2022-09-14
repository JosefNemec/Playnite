using Playnite.Common;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ImageGameIcon", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ImageLibraryIcon", Type = typeof(Image))]
    [TemplatePart(Name = "PART_TextName", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ItemsAdditionalInfo", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ElemHiddenStatus", Type = typeof(FrameworkElement))]
    public class SearchWindowGameItem : Control
    {
        private readonly DesktopAppViewModel mainModel;

        private Image ImageGameIcon;
        private Image ImageLibraryIcon;
        private TextBlock TextName;
        private ItemsControl ItemsAdditionalInfo;
        private FrameworkElement ElemHiddenStatus;

        static SearchWindowGameItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchWindowGameItem), new FrameworkPropertyMetadata(typeof(SearchWindowGameItem)));
        }

        public SearchWindowGameItem() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public SearchWindowGameItem(DesktopAppViewModel mainModel)
        {
            if (DesignerTools.IsInDesignMode)
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        private string GetBindingPath(string targetName)
        {
            return $"{nameof(GameSearchItemWrapper.GameView)}.{targetName}";
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ImageGameIcon = Template.FindName("PART_ImageGameIcon", this) as Image;
            if (ImageGameIcon != null)
            {
                BindingTools.SetBinding(
                    ImageGameIcon,
                    Image.VisibilityProperty,
                    mainModel.AppSettings.SearchWindowVisibility,
                    nameof(SearchWindowVisibilitySettings.GameIcon),
                    converter: Converters.BooleanToVisibilityConverter.Instance);

                var sourceBinding = new PriorityBinding();
                sourceBinding.Bindings.Add(new Binding
                {
                    Path = new PropertyPath(GetBindingPath(nameof(GamesCollectionViewEntry.IconObject))),
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneTime
                });
                sourceBinding.Bindings.Add(new Binding
                {
                    Path = new PropertyPath(GetBindingPath(nameof(GamesCollectionViewEntry.DefaultIconObject))),
                    Mode = BindingMode.OneTime
                });

                BindingOperations.SetBinding(ImageGameIcon, Image.SourceProperty, sourceBinding);
            }

            ImageLibraryIcon = Template.FindName("PART_ImageLibraryIcon", this) as Image;
            if (ImageLibraryIcon != null)
            {
                BindingTools.SetBinding(
                    ImageLibraryIcon,
                    Image.SourceProperty,
                    GetBindingPath(nameof(GamesCollectionViewEntry.LibraryIcon)),
                    BindingMode.OneTime);

                var visibilityBinding = new MultiBinding();
                visibilityBinding.Converter = Converters.MultiBooleanToVisibilityConverter.Instance;
                visibilityBinding.Bindings.Add(new Binding
                {
                    Path = new PropertyPath(GetBindingPath(nameof(GamesCollectionViewEntry.LibraryIcon))),
                    Mode = BindingMode.OneTime,
                    Converter = NullToBoolConverter.Instance
                });
                visibilityBinding.Bindings.Add(new Binding
                {
                    Path = new PropertyPath(nameof(SearchWindowVisibilitySettings.LibraryIcon)),
                    Source = mainModel.AppSettings.SearchWindowVisibility,
                    Mode = BindingMode.OneTime
                });

                BindingTools.SetBinding(
                    ImageLibraryIcon,
                    Image.VisibilityProperty,
                    visibilityBinding);
            }

            TextName = Template.FindName("PART_TextName", this) as TextBlock;
            if (TextName != null)
            {
                BindingTools.SetBinding(
                    TextName,
                    TextBlock.TextProperty,
                    GetBindingPath(nameof(GamesCollectionViewEntry.DisplayName)),
                    BindingMode.OneTime);
            }

            ElemHiddenStatus = Template.FindName("PART_ElemHiddenStatus", this) as FrameworkElement;
            if (ElemHiddenStatus != null)
            {
                BindingTools.SetBinding(
                    ElemHiddenStatus,
                    FrameworkElement.VisibilityProperty,
                    mainModel.AppSettings.SearchWindowVisibility,
                    nameof(SearchWindowVisibilitySettings.HiddenStatus),
                    converter: Converters.BooleanToVisibilityConverter.Instance);

                BindingTools.SetBinding(
                    ElemHiddenStatus,
                    FrameworkElement.VisibilityProperty,
                    GetBindingPath(nameof(GamesCollectionViewEntry.Hidden)),
                    mode: BindingMode.OneTime,
                    converter: Converters.BooleanToVisibilityConverter.Instance);
            }

            ItemsAdditionalInfo = Template.FindName("PART_ItemsAdditionalInfo", this) as ItemsControl;
            if (ItemsAdditionalInfo != null)
            {
                BindingTools.SetBinding(
                    ItemsAdditionalInfo,
                    ItemsControl.ItemsSourceProperty,
                    nameof(GameSearchItemWrapper.AdditionalInfo),
                    BindingMode.OneTime);
            }
        }
    }

    [TemplatePart(Name = "PART_ContentIcon", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_TextName", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextDescription", Type = typeof(TextBlock))]
    public class SearchWindowSearchItem : Control
    {
        private ContentControl ContentIcon;
        private TextBlock TextName;
        private TextBlock TextDescription;

        static SearchWindowSearchItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchWindowSearchItem), new FrameworkPropertyMetadata(typeof(SearchWindowSearchItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ContentIcon = Template.FindName("PART_ContentIcon", this) as ContentControl;
            if (ContentIcon != null)
            {
                BindingTools.SetBinding(
                    ContentIcon,
                    ContentControl.ContentProperty,
                    nameof(SearchItemWrapper.ItemIcon));
            }

            TextName = Template.FindName("PART_TextName", this) as TextBlock;
            if (TextName != null)
            {
                BindingTools.SetBinding(
                    TextName,
                    TextBlock.TextProperty,
                    nameof(SearchItemWrapper.Item.Name),
                    BindingMode.OneTime);
            }

            TextDescription = Template.FindName("PART_TextDescription", this) as TextBlock;
            if (TextDescription != null)
            {
                BindingTools.SetBinding(
                    TextDescription,
                    TextBlock.TextProperty,
                    nameof(SearchItemWrapper.Item.Description),
                    mode: BindingMode.OneTime);
                BindingTools.SetBinding(
                    TextDescription,
                    TextBlock.VisibilityProperty,
                    nameof(SearchItemWrapper.Item.Description),
                    mode: BindingMode.OneTime,
                    converter: StringNullOrEmptyToVisibilityConverter.Instance);
            }
        }
    }
}
