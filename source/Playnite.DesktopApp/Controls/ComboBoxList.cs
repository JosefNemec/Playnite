using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;

namespace Playnite.DesktopApp.Controls
{
    public class ComboBoxList : ComboBoxListBase
    {
        #region ItemsList
        public SelectableObjectList<object> ItemsList
        {
            get => (SelectableObjectList<object>)GetValue(ItemsListProperty);
            set => SetValue(ItemsListProperty, value);
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableObjectList<object>),
            typeof(ComboBoxList));
        #endregion

        #region ItemsSource
        public IList ItemsSource
        {
            get => (IList)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IList),
            typeof(ComboBoxList),
            new PropertyMetadata(null, ItemsSourcePropertyChangedCallback));

        private static void ItemsSourcePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as ComboBoxList;
            var newVal = (IList)e.NewValue;
            if (obj.ItemsList != null)
            {
                obj.ItemsList.SelectionChanged -= obj.List_SelectionChanged;
            }

            obj.ItemsList = new SelectableObjectList<object>(null);
            obj.ItemsList.SelectionChanged += obj.List_SelectionChanged;
            obj.SetSelectedItem();
        }
        #endregion

        internal void SetSelectedItem()
        {
            if (ItemsList == null)
            {
                return;
            }

            IgnoreChanges = true;
            ItemsList.SetItems(ItemsSource as IEnumerable<object>, SelectedItems as IEnumerable<object>);
            UpdateTextStatus();
            IgnoreChanges = false;
        }

        #region SelectedItems
        public IList SelectedItems
        {
            get => (IList)GetValue(SelectedItemsProperty);
            set
            {
                if (value == null)
                {
                    SetValue(SelectedItemsProperty, value);
                    return;
                }

                // This makes it possible to bind back into generic List collections instead of just IList.
                var binding = BindingOperations.GetBindingExpression(this, SelectedItemsProperty);
                if (binding != null)
                {
                    var targetProp = binding.ResolvedSource.GetType().GetProperty(binding.ResolvedSourcePropertyName);
                    if (targetProp.PropertyType.IsGenericList(out var itemType))
                    {
                        var newList = targetProp.PropertyType.CrateInstance();
                        var addMethod = newList.GetType().GetMethod("Add");
                        foreach (var val in value)
                        {
                            addMethod.Invoke(newList, new object[] { Convert.ChangeType(val, itemType) });
                        }

                        SetValue(SelectedItemsProperty, newList);
                        return;
                    }
                }

                SetValue(SelectedItemsProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(ComboBoxList),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedItemsPropertyChangedCallback));

        private static void SelectedItemsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as ComboBoxList;
            if (!obj.IgnoreChanges)
            {
                obj.SetSelectedItem();
            }
        }
        #endregion

        static ComboBoxList()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxList), new FrameworkPropertyMetadata(typeof(ComboBoxList)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ItemsPanel != null)
            {
                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item}"),
                            new XAttribute(nameof(CheckBox.IsThreeState), "{Binding IsThreeState, Mode=OneWay, RelativeSource={RelativeSource AncestorType=ComboBoxList}}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());
            }

            UpdateTextStatus();
        }

        public override void ClearButtonAction(RoutedEventArgs e)
        {
            SelectedItems = null;
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                var sel = ItemsList.GetSelectedItems();
                SelectedItems = sel;
                IgnoreChanges = false;
                UpdateTextStatus();
            }
        }

        private void UpdateTextStatus()
        {
            if (TextFilterString != null)
            {
                TextFilterString.Text = ItemsList?.AsString;
            }
        }
    }
}
