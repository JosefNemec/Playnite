using Playnite.Behaviors;
using Playnite.Commands;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using Playnite.SDK;
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

namespace Playnite.FullscreenApp.Controls
{
    [TemplatePart(Name = "PART_MenuHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ButtonBack", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonClear", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ItemsHost", Type = typeof(ItemsControl))]
    public class FilterStringListSelection : Control, IDisposable
    {
        private bool isPrimaryFilter;
        private FullscreenAppViewModel mainModel;
        private FrameworkElement MenuHost;
        private ButtonBase ButtonBack;
        private ButtonBase ButtonClear;
        private ItemsControl ItemsHost;

        internal bool IgnoreChanges { get; set; }
        public string Title { get; set; }

        public SelectableStringList ItemsList
        {
            get
            {
                return (SelectableStringList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableStringList),
            typeof(FilterStringListSelection),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as FilterStringListSelection;
            if (box.IgnoreChanges)
            {
                return;
            }

            var list = (SelectableStringList)e.NewValue;
            if (list == null)
            {
                var oldList = (SelectableStringList)e.OldValue;
                oldList.SelectionChanged -= box.List_SelectionChanged;
            }
            else
            {
                box.IgnoreChanges = true;
                list.SelectionChanged += box.List_SelectionChanged;
                if (box.FilterProperties != null)
                {
                    list.SetSelection(box.FilterProperties.Values);
                }

                box.IgnoreChanges = false;
            }

        }

        public void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                FilterProperties = new StringFilterItemProperites(ItemsList.GetSelectedItems());
                IgnoreChanges = false;
            }
        }

        public StringFilterItemProperites FilterProperties
        {
            get
            {
                return (StringFilterItemProperites)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(StringFilterItemProperites),
            typeof(FilterStringListSelection),
            new PropertyMetadata(null, FilterPropertiesPropertyChangedCallback));

        private static void FilterPropertiesPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterStringListSelection;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            if (obj.FilterProperties?.IsSet != true)
            {
                obj.ItemsList?.SetSelection(null);
            }
            else
            {
                obj.ItemsList?.SetSelection(obj.FilterProperties.Values);
            }
            obj.IgnoreChanges = false;
        }

        static FilterStringListSelection()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterStringListSelection), new FrameworkPropertyMetadata(typeof(FilterStringListSelection)));
        }

        public FilterStringListSelection() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public FilterStringListSelection(FullscreenAppViewModel mainModel, bool isPrimaryFilter = false) : base()
        {
            this.isPrimaryFilter = isPrimaryFilter;
            this.mainModel = mainModel;
            DataContext = this;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                var closeCommand = isPrimaryFilter ? mainModel.CloseSubFilterCommand : mainModel.CloseAdditionalFilterCommand;
                MenuHost = Template.FindName("PART_MenuHost", this) as FrameworkElement;
                if (MenuHost != null)
                {
                    MenuHost.InputBindings.Add(new KeyBinding(closeCommand, new KeyGesture(Key.Back)));
                    MenuHost.InputBindings.Add(new KeyBinding(closeCommand, new KeyGesture(Key.Escape)));
                    MenuHost.InputBindings.Add(new XInputBinding(closeCommand, XInputButton.B));
                }

                ButtonBack = Template.FindName("PART_ButtonBack", this) as ButtonBase;
                if (ButtonBack != null)
                {
                    ButtonBack.Command = closeCommand;
                }

                ButtonClear = Template.FindName("PART_ButtonClear", this) as ButtonBase;
                if (ButtonClear != null)
                {
                    ButtonClear.Command = new RelayCommand<object>(a =>
                    {
                        FilterProperties = null;
                        IgnoreChanges = true;
                        ItemsList?.SetSelection(null);
                        IgnoreChanges = false;
                    });
                    BindingTools.SetBinding(ButtonClear,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.SubFilterVisible));
                }

                ItemsHost = Template.FindName("PART_ItemsHost", this) as ItemsControl;
                if (ItemsHost != null)
                {
                    BindingTools.SetBinding(
                        ItemsHost,
                        ItemsControl.ItemsSourceProperty,
                        this,
                        nameof(ItemsList));
                }
            }
        }

        public void Dispose()
        {
            BindingOperations.ClearBinding(this, ItemsListProperty);
            BindingOperations.ClearBinding(this, FilterPropertiesProperty);
            if (ItemsList != null)
            {
                ItemsList.SelectionChanged -= List_SelectionChanged;
                ItemsList = null;
            }

            FilterProperties = null;

        }
    }
}
