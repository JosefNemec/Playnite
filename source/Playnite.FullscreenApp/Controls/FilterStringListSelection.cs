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
using System.Windows.Automation;
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

        public SelectableObjectList<NamedObject<string>> ItemsList
        {
            get
            {
                return (SelectableObjectList<NamedObject<string>>)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableObjectList<NamedObject<string>>),
            typeof(FilterStringListSelection),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as FilterStringListSelection;
            if (box.IgnoreChanges)
            {
                return;
            }

            var list = (SelectableObjectList<NamedObject<string>>)e.NewValue;
            if (list == null)
            {
                var oldList = (SelectableObjectList<NamedObject<string>>)e.OldValue;
                oldList.SelectionChanged -= box.List_SelectionChanged;
            }
            else
            {
                box.IgnoreChanges = true;
                list.SelectionChanged += box.List_SelectionChanged;
                if (box.FilterProperties != null)
                {
                    list.SetSelection(box.FilterProperties.Values?.Select(a => new NamedObject<string>(a)));
                }

                box.IgnoreChanges = false;
            }
        }

        public void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                FilterProperties = new StringFilterItemProperties(ItemsList.GetSelectedItems().Select(a => a.Value).ToList());
                IgnoreChanges = false;
            }
        }

        public StringFilterItemProperties FilterProperties
        {
            get
            {
                return (StringFilterItemProperties)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(StringFilterItemProperties),
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
                obj.ItemsList?.SetSelection(obj.FilterProperties.Values?.Select(a => new NamedObject<string>(a)));
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
                    var backInput = new GameControllerInputBinding { Command = closeCommand };
                    BindingTools.SetBinding(backInput,
                        GameControllerInputBinding.ButtonProperty,
                        null,
                        typeof(GameControllerGesture).GetProperty(nameof(GameControllerGesture.CancellationBinding)));
                    MenuHost.InputBindings.Add(backInput);
                }

                ButtonBack = Template.FindName("PART_ButtonBack", this) as ButtonBase;
                if (ButtonBack != null)
                {
                    ButtonBack.Command = closeCommand;
                    AutomationProperties.SetName(ButtonBack, LOC.BackLabel.GetLocalized());
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
                    FilterDbItemtSelection.AssignItemListPanel(ItemsHost);
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
            if (ItemsList != null)
            {
                ItemsList.SelectionChanged -= List_SelectionChanged;
                ItemsList.SetSelection(null);
            }

            BindingOperations.ClearBinding(this, ItemsListProperty);
            BindingOperations.ClearBinding(this, FilterPropertiesProperty);
            ItemsList = null;
            FilterProperties = null;
        }
    }
}
