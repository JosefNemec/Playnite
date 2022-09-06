using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Linq;

namespace Playnite.DesktopApp.Controls
{
    public class GridViewPanel : VirtualizingPanel, IScrollInfo
    {
        private int computedColumns;
        private double centerMargin;
        private int itemCount => ((ItemContainerGenerator)ItemContainerGenerator).Items.Count;

        // Important for grouped virtualization to work
        protected override bool CanHierarchicallyScrollAndVirtualizeCore => true;

        private DependencyObject itemsOwner;
        protected DependencyObject ItemsOwner
        {
            get
            {
                if (itemsOwner is null)
                {
                    var getItemsOwnerInternalMethod = typeof(ItemsControl).GetMethod(
                        "GetItemsOwnerInternal",
                        BindingFlags.Static | BindingFlags.NonPublic,
                        null,
                        new Type[] { typeof(DependencyObject) },
                        null
                    );

                    itemsOwner = (DependencyObject)getItemsOwnerInternalMethod.Invoke(null, new object[] { this });
                }

                return itemsOwner;
            }
        }

        private IRecyclingItemContainerGenerator itemContainerGenerator;
        protected new IRecyclingItemContainerGenerator ItemContainerGenerator
        {
            get
            {
                if (itemContainerGenerator is null)
                {
                    /* Because of a bug in the framework the ItemContainerGenerator
                     * is null until InternalChildren accessed at least one time. */
                    var children = InternalChildren;
                    itemContainerGenerator = (IRecyclingItemContainerGenerator)base.ItemContainerGenerator;
                }
                return itemContainerGenerator;
            }
        }

        private double cachedItemWith;
        public double ItemWidth
        {
            get
            {
                double width = 0;
                if (InternalChildren.Count > 0)
                {
                    width = InternalChildren[0].DesiredSize.Width;
                }

                if (width > 0)
                {
                    cachedItemWith = width;
                    return width;
                }
                else
                {
                    return cachedItemWith;
                }
            }
        }

        private double cachedItemHeight;
        public double ItemHeight
        {
            get
            {
                double height = 0;
                if (InternalChildren.Count > 0)
                {
                    height = InternalChildren[0].DesiredSize.Height;
                }

                if (height > 0)
                {
                    cachedItemHeight = height;
                    return height;
                }
                else
                {
                    return cachedItemHeight;
                }
            }
        }

        public GridViewPanel() : base()
        {
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            // This is for weird edge case where Measuring can occur and OnItemsChanged was not called by WPF first
            if (ItemWidth == 0)
            {
                var startPosition = ItemContainerGenerator.GeneratorPositionFromIndex(0);
                using (ItemContainerGenerator.StartAt(startPosition, GeneratorDirection.Forward, true))
                {
                    var child = (UIElement)ItemContainerGenerator.GenerateNext();
                    if (child != null)
                    {
                        AddInternalChild(child);
                        ItemContainerGenerator.PrepareItemContainer(child);
                        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                    }
                }
            }

            double expanderOffset = 0;
            var groupItem = ItemsOwner as IHierarchicalVirtualizationAndScrollInfo;
            if (groupItem != null)
            {
                // This is a workaround for issues like #2454, caused by some bug in WPF.
                // Collapsed groups are SOMETIMES treated as expandeded and will force initialization of child items.
                var expander = ElementTreeHelper.FindVisualChildren<Expander>(groupItem as GroupItem).FirstOrDefault();
                if (expander != null && expander.IsExpanded == false)
                {
                    UpdateScrollInfo(new Size(0, 0));
                    CleanUpItems();
                    return new Size(0, 0);
                }
                else
                {
                    UpdateScrollInfo(groupItem.Constraints.Viewport.Size);
                }

                if (expander != null)
                {
                    var toggle = ElementTreeHelper.FindVisualChildren<ToggleButton>(groupItem as GroupItem).FirstOrDefault();
                    if (toggle != null)
                    {
                        expanderOffset = toggle.ActualHeight;
                    }
                }

                Offset = groupItem.Constraints.Viewport.Location;
            }
            else
            {
                UpdateScrollInfo(availableSize);
            }

            GetVisibleRange(expanderOffset, out var firstItemIndex, out var lastItemIndex);
            if (lastItemIndex < 0)
            {
                return Extent;
            }

            var startPos = ItemContainerGenerator.GeneratorPositionFromIndex(firstItemIndex);
            var childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;
            using (ItemContainerGenerator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstItemIndex; itemIndex <= lastItemIndex; ++itemIndex, ++childIndex)
                {
                    UIElement child = ItemContainerGenerator.GenerateNext(out var newlyRealized) as UIElement;
                    if (child == null)
                    {
                        continue;
                    }

                    if (newlyRealized)
                    {
                        if (childIndex >= InternalChildren.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }

                        ItemContainerGenerator.PrepareItemContainer(child);
                    }
                    else if (!InternalChildren.Contains(child))
                    {
                        InsertInternalChild(childIndex, child);
                        ItemContainerGenerator.PrepareItemContainer(child);
                    }

                    child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                }
            }

            CleanUpItems(firstItemIndex, lastItemIndex);
            return Extent;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < InternalChildren.Count; i++)
            {
                var child = InternalChildren[i];
                var itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                // Negative index means that child is disconnected item
                if (itemIndex >= 0)
                {
                    child.Arrange(GetItemRect(itemIndex));
                }
            }

            return finalSize;
        }

        private Rect GetItemRect(int itemIndex)
        {
            if (computedColumns == 0)
            {
                return new Rect();
            }

            var column = itemIndex % computedColumns;
            var row = itemIndex < column ? 0 : (int)Math.Floor(itemIndex / (double)computedColumns);
            var offset = ItemsOwner is IHierarchicalVirtualizationAndScrollInfo ? 0 : Offset.Y;
            return new Rect(
                centerMargin + (column * ItemWidth),
                (row * ItemHeight) - offset,
                ItemWidth,
                ItemHeight);
        }

        private void GetVisibleRange(double expanderOffset, out int firstIndex, out int lastIndex)
        {
            if (itemCount == 0 || Viewport.Height == 0)
            {
                firstIndex = -1;
                lastIndex = -1;
                return;
            }

            var startRow = 0;
            double totalHeight = 0;
            while (true)
            {
                if (Offset.Y - expanderOffset > totalHeight + ItemHeight)
                {
                    totalHeight += ItemHeight;
                    startRow++;
                }
                else
                {
                    break;
                }
            }

            firstIndex = startRow * computedColumns;
            var newRows = (int)Math.Ceiling(Viewport.Height / ItemHeight) + 1;
            lastIndex = firstIndex + (newRows * computedColumns);
            if (lastIndex >= itemCount)
            {
                lastIndex = itemCount - 1;
            }
        }

        private void CleanUpItems()
        {
            RemoveInternalChildRange(0, InternalChildren.Count - 1);
        }

        private void CleanUpItems(int firstIndex, int lastIndex)
        {
            for (int i = InternalChildren.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(childGeneratorPos);
                var child = InternalChildren[i];
                if ((itemIndex < firstIndex || itemIndex > lastIndex) && itemIndex > 0)
                {
                    try
                    {
                        ItemContainerGenerator.Recycle(childGeneratorPos, 1);
                    }
                    catch
                    {
                        // There are some weird null-reference crash reports from Generator.Recycle
                    }

                    RemoveInternalChildRange(i, 1);
                }
                else if (child.ToString().Contains("{DisconnectedItem}"))
                {
                    RemoveInternalChildRange(i, 1);
                }
            }
        }

        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    break;
            }
        }

        internal Size GetExtent()
        {
            if (itemCount == 0)
            {
                return new Size(0, 0);
            }

            if (computedColumns == 0)
            {
                return new Size(
                    Viewport.Width,
                    itemCount * ItemHeight);
            }
            else
            {
                var totalRows = (int)Math.Ceiling(itemCount / (double)computedColumns);
                return new Size(
                    Viewport.Width,
                    totalRows * ItemHeight);
            }
        }

        private Size Viewport = new Size(0, 0);
        private Point Offset = new Point(0, 0);
        private Size Extent = new Size(0, 0);

        public bool CanVerticallyScroll { get; set; }
        public bool CanHorizontallyScroll { get; set; }
        public double ExtentWidth => Extent.Width;
        public double ExtentHeight => Extent.Height;
        public double ViewportWidth => Viewport.Width;
        public double ViewportHeight => Viewport.Height;
        public double HorizontalOffset => Offset.X;
        public double VerticalOffset => Offset.Y;
        public ScrollViewer ScrollOwner { get; set; }

        internal void UpdateScrollInfo(Size availableSize)
        {
            var invalidate = false;
            if (availableSize != Viewport)
            {
                Viewport = availableSize;
            }

            var itemWidth = ItemWidth;
            if (ItemWidth > 0)
            {
                computedColumns = (int)Math.Floor(Viewport.Width / itemWidth);
                centerMargin = (Viewport.Width - (computedColumns * itemWidth)) / 2;
            }
            else
            {
                computedColumns = 0;
                centerMargin = 0;
            }

            var newExtent = GetExtent();
            if (Extent != newExtent)
            {
                Extent = newExtent;
                invalidate = true;
            }

            if (Offset.Y > Extent.Height)
            {
                Offset.Y = 0;
                invalidate = true;
            }

            if (Offset.X > Extent.Width)
            {
                Offset.X = 0;
                invalidate = true;
            }

            if (invalidate)
            {
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public void SetHorizontalOffset(double newOffset)
        {
        }

        public void SetVerticalOffset(double newOffset)
        {
            if (newOffset < 0 || Viewport.Height >= Extent.Height)
            {
                newOffset = 0;
            }
            else
            {
                if (newOffset + Viewport.Height >= Extent.Height)
                {
                    newOffset = Extent.Height - Viewport.Height;
                }
            }

            Offset.Y = newOffset;
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        protected override void BringIndexIntoView(int index)
        {
            if (index < 0)
            {
                return;
            }

            var itemRect = GetItemRect(index);
            if (itemRect.Y > 0 && itemRect.Bottom < Viewport.Height)
            {
                return;
            }
            else if (itemRect.Bottom > Viewport.Height)
            {
                SetVerticalOffset(Offset.Y + (itemRect.Bottom - Viewport.Height));
                return;
            }
            else if (itemRect.Y < 0)
            {
                SetVerticalOffset(Offset.Y + itemRect.Y);
                return;
            }
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var index = ((ItemContainerGenerator)ItemContainerGenerator).IndexFromContainer(visual);
            if (index < 0)
            {
                return rectangle;
            }

            var itemRect = GetItemRect(index);
            if (itemRect.Y > 0 && itemRect.Bottom < Viewport.Height)
            {
                return rectangle;
            }
            else if (itemRect.Bottom > Viewport.Height)
            {
                SetVerticalOffset(Offset.Y + (itemRect.Bottom - Viewport.Height));
                return rectangle;
            }
            else if (itemRect.Y < 0)
            {
                SetVerticalOffset(Offset.Y + itemRect.Y);
                return rectangle;
            }

            return rectangle;
        }

        public void LineLeft()
        {
        }

        public void LineRight()
        {
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - ItemHeight);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + ItemHeight);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + (ItemHeight / 2));
        }

        public void MouseWheelLeft()
        {
            LineLeft();
        }

        public void MouseWheelRight()
        {
            LineRight();
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - (ItemHeight / 2));
        }

        public void PageLeft()
        {
        }

        public void PageRight()
        {
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - Viewport.Height);
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + Viewport.Height);
        }
    }
}
