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
    public class FilterEnumListSelection : Control, IDisposable
    {
        public class SelectionObject
        {
            public string Name { get; }
            public int Value { get; }

            public SelectionObject(Enum enumValue)
            {
                Value = Convert.ToInt32(enumValue);
                Name = enumValue.GetDescription();
            }
        }

        private bool isPrimaryFilter = false;
        private FullscreenAppViewModel mainModel;
        private FrameworkElement MenuHost;
        private ButtonBase ButtonBack;
        private ButtonBase ButtonClear;
        private ItemsControl ItemsHost;

        internal bool IgnoreChanges { get; set; }
        public string Title { get; set; }
        public List<SelectableItem<SelectionObject>> ItemsList { get; set; }

        public Type EnumType
        {
            get
            {
                return (Type)GetValue(EnumTypeProperty);
            }

            set
            {
                SetValue(EnumTypeProperty, value);
            }
        }

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType),
            typeof(Type),
            typeof(FilterEnumListSelection),
            new PropertyMetadata(null, EnumTypePropertyChangedCallback));

        private static void EnumTypePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterEnumListSelection;
            var list = (Type)e.NewValue;
            var items = new List<SelectableItem<SelectionObject>>();
            foreach (Enum en in list.GetEnumValues())
            {
                var newItem = new SelectableItem<SelectionObject>(new SelectionObject(en));
                if (obj.FilterProperties != null)
                {
                    newItem.Selected = obj.FilterProperties.Values?.Contains(newItem.Item.Value) == true;
                }

                newItem.PropertyChanged += (s, args) =>
                {
                    if (obj.IgnoreChanges)
                    {
                        return;
                    }

                    if (args.PropertyName == nameof(newItem.Selected))
                    {
                        var selected = obj.ItemsList.Where(a => a.Selected == true);
                        if (selected.HasItems())
                        {
                            obj.FilterProperties = new EnumFilterItemProperites(selected.Select(a => a.Item.Value).ToList());
                        }
                        else
                        {
                            obj.FilterProperties = null;
                        }
                    }
                };

                items.Add(newItem);
            }

            obj.ItemsList = items;
        }

        public EnumFilterItemProperites FilterProperties
        {
            get
            {
                return (EnumFilterItemProperites)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(EnumFilterItemProperites),
            typeof(FilterEnumListSelection),
            new PropertyMetadata(null, FilterPropertiesPropertyChangedCallback));

        private static void FilterPropertiesPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterEnumListSelection;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            if (obj.FilterProperties?.IsSet != true)
            {
                obj.ItemsList?.ForEach(a => a.Selected = false);
            }
            else
            {
                obj.ItemsList?.ForEach(a => a.Selected = obj.FilterProperties.Values.Contains(a.Item.Value));
            }

            obj.IgnoreChanges = false;
        }

        static FilterEnumListSelection()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterEnumListSelection), new FrameworkPropertyMetadata(typeof(FilterEnumListSelection)));
        }

        public FilterEnumListSelection() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public FilterEnumListSelection(FullscreenAppViewModel mainModel, bool isPrimaryFilter = false) : base()
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
                        ItemsList?.ForEach(b => b.Selected = false);
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
            BindingOperations.ClearBinding(this, EnumTypeProperty);
            BindingOperations.ClearBinding(this, FilterPropertiesProperty);
            ItemsList = null;
            FilterProperties = null;
        }
    }
}
