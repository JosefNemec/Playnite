using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace Playnite.DesktopApp.Controls
{
    public class DdItemListSelectionBox : ComboBoxListBase
    {
        public SelectableDbItemList ItemsList
        {
            get
            {
                return (SelectableDbItemList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableDbItemList),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            var oldVal = (SelectableDbItemList)e.NewValue;
            if (oldVal != null)
            {
                oldVal.SelectionChanged -= obj.List_SelectionChanged;
            }

            var list = (SelectableDbItemList)e.NewValue;
            list.SelectionChanged += obj.List_SelectionChanged;
            obj.UpdateTextStatus();
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                BoundIds = ItemsList.GetSelectedIds();
                IgnoreChanges = false;
                UpdateTextStatus();
            }
        }

        public object BoundIds
        {
            get
            {
                return GetValue(BoundIdsProperty);
            }

            set
            {
                SetValue(BoundIdsProperty, value);
            }
        }

        public static readonly DependencyProperty BoundIdsProperty = DependencyProperty.Register(
            nameof(BoundIds),
            typeof(object),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(null, BoundIdsPropertyChangedCallback));

        private static void BoundIdsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            obj.ItemsList?.SetSelection(obj.BoundIds as IEnumerable<Guid>);
            obj.IgnoreChanges = false;
            obj.UpdateTextStatus();
        }

        static DdItemListSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DdItemListSelectionBox), new FrameworkPropertyMetadata(typeof(DdItemListSelectionBox)));
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
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item.Name}"),
                            new XAttribute(nameof(CheckBox.IsThreeState), "{Binding IsThreeState, Mode=OneWay, RelativeSource={RelativeSource AncestorType=DdItemListSelectionBox}}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());
            }

            UpdateTextStatus();
        }

        public override void ClearButtonAction(RoutedEventArgs e)
        {
            ItemsList.SetSelection(null);
            BoundIds = null;
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
