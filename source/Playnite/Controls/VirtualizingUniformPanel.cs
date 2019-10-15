using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Playnite.Controls
{
    public class VirtualizingUniformPanel : VirtualizingPanel, IScrollInfo
    {
        private IRecyclingItemContainerGenerator generator;
        internal ItemsControl itemsControl;
        private int computedColumns;
        private double centerMargin;
        private int itemCount => itemsControl?.HasItems == true ? itemsControl.Items.Count : 0;

        private double cachedItemWith;
        public double ItemWidth
        {
            get
            {
                double width = 0;
                if (Children.Count > 0)
                {
                    width = Children[0].DesiredSize.Width;
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
                if (Children.Count > 0)
                {
                    height = Children[0].DesiredSize.Height;
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

        public VirtualizingUniformPanel() : base()
        {
            RenderTransform = trans;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            UpdateScrollInfo(availableSize);

            // This is for weird edge case where Measuring can occur and OnItemsChanged was not called by WPF first
            if (ItemWidth == 0)
            {
                itemsControl = ItemsControl.GetItemsOwner(this);
                generator = (IRecyclingItemContainerGenerator)ItemContainerGenerator;
                using (generator.StartAt(generator.GeneratorPositionFromIndex(0), GeneratorDirection.Forward, true))
                {
                    UIElement child = generator.GenerateNext(out var newlyRealized) as UIElement;
                    if (child != null)
                    {
                        AddInternalChild(child);
                        generator.PrepareItemContainer(child);
                        child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                        UpdateScrollInfo(availableSize);
                        RemoveInternalChildRange(0, 1);
                    }
                }                
            }

            GetVisibleRange(out var firstItemIndex, out var lastItemIndex);
            if (lastItemIndex < 0)
            {
                return new Size(0, 0);
            }

            var startPos = generator.GeneratorPositionFromIndex(firstItemIndex);
            var childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstItemIndex; itemIndex <= lastItemIndex; ++itemIndex, ++childIndex)
                {
                    UIElement child = generator.GenerateNext(out var newlyRealized) as UIElement;
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

                        generator.PrepareItemContainer(child);
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
            if (availableSize.Width == double.PositiveInfinity || availableSize.Height == double.PositiveInfinity)
            {
                return GetExtent();
            }
            else
            {
                return availableSize;
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateScrollInfo(finalSize);

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                var itemIndex = generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
                child.Arrange(GetItemRect(itemIndex));
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
            return new Rect(
                centerMargin + (column * ItemWidth),
                (row * ItemHeight),
                ItemWidth,
                ItemHeight);
        }

        private void GetVisibleRange(out int firstIndex, out int lastIndex)
        {
            if (itemCount == 0)
            {
                firstIndex = -1;
                lastIndex = -1;
                return;
            }

            var rows = 0;
            double totalHeight = 0;
            while (true)
            {
                if (offset.Y > totalHeight + ItemHeight)
                {
                    totalHeight += ItemHeight;
                    rows++;
                }
                else
                {
                    break;
                }
            }

            firstIndex = (int)((rows == 0 ? rows : rows) * computedColumns);
            var newRows = (int)Math.Ceiling(viewport.Height / ItemHeight) + 1;
            lastIndex = firstIndex + (newRows * computedColumns);
            if (lastIndex >= itemCount)
            {
                lastIndex = itemCount - 1;
            }
        }

        private void CleanUpItems(int firstIndex, int lastIndex)
        {
            for (int i = InternalChildren.Count - 1; i >= 0; i--)
            {
                var childGeneratorPos = new GeneratorPosition(i, 0);
                int itemIndex = generator.IndexFromGeneratorPosition(childGeneratorPos);
                if ((itemIndex < firstIndex || itemIndex > lastIndex) && itemIndex > 0)
                {
                    generator.Remove(childGeneratorPos, 1);
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
                    if (args.OldPosition.Index < 0)
                    {
                        InvalidateMeasure();
                        ScrollOwner?.InvalidateScrollInfo();
                        SetVerticalOffset(0);
                    }
                    else
                    {
                        RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    itemsControl = ItemsControl.GetItemsOwner(this);
                    generator = (IRecyclingItemContainerGenerator)ItemContainerGenerator;
                    InvalidateMeasure();
                    ScrollOwner?.InvalidateScrollInfo();
                    SetVerticalOffset(0);
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
                    viewport.Width,
                    itemCount * ItemHeight);
            }
            else
            {
                var totalRows = (int)Math.Ceiling(itemCount / (double)computedColumns);
                return new Size(
                    viewport.Width,
                    totalRows * ItemHeight);
            }
        }

        #region IScrollInfo

        private Size viewport = new Size(0, 0);
        private Point offset = new Point(0, 0);
        private Size extent = new Size(0, 0);
        private TranslateTransform trans = new TranslateTransform();

        public bool CanVerticallyScroll { get; set; } = false;
        public bool CanHorizontallyScroll { get; set; } = false;
        public double ExtentWidth => extent.Width;
        public double ExtentHeight => extent.Height;
        public double ViewportWidth => viewport.Width;
        public double ViewportHeight => viewport.Height;
        public double HorizontalOffset => offset.X;
        public double VerticalOffset => offset.Y;
        public ScrollViewer ScrollOwner { get; set; }

        internal void UpdateScrollInfo(Size availableSize)
        {
            if (availableSize != viewport)
            {
                viewport = availableSize;
                ScrollOwner?.InvalidateScrollInfo();
            }

            var itemWidth = ItemWidth;
            if (ItemWidth > 0)
            {
                computedColumns = (int)Math.Floor(viewport.Width / itemWidth);
                centerMargin = (viewport.Width - (computedColumns * itemWidth)) / 2;
            }
            else
            {
                computedColumns = 0;
                centerMargin = 0;
            }
            
            var newExtent = GetExtent();
            if (extent != newExtent)
            {
                extent = newExtent;
                ScrollOwner?.InvalidateScrollInfo();
            }

            if (offset.Y > extent.Height)
            {
                offset.Y = 0;
                trans.Y = 0;
                ScrollOwner?.InvalidateScrollInfo();
            }

            if (offset.X > extent.Width)
            {
                offset.X = 0;
                trans.X = 0;
                ScrollOwner?.InvalidateScrollInfo();
            }
        }

        public void SetHorizontalOffset(double newOffset)
        {
        }

        public void SetVerticalOffset(double newOffset)
        {
            if (newOffset < 0 || viewport.Height >= extent.Height)
            {
                newOffset = 0;
            }
            else
            {
                if (newOffset + viewport.Height >= extent.Height)
                {
                    newOffset = extent.Height - viewport.Height;
                }
            }

            offset.Y = newOffset;
            trans.Y = -newOffset;
            ScrollOwner?.InvalidateScrollInfo();
            InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var index = ((ItemContainerGenerator)generator).IndexFromContainer(visual);
            if (index < 0)
            {
                return rectangle;
            }

            var itemRect = GetItemRect(index);            
            var movedViewport = new Rect(0, offset.Y, 0, offset.Y + viewport.Height);
            if (itemRect.Y > movedViewport.Y && itemRect.Y + ItemHeight < movedViewport.Height)
            {
                return rectangle;
            }
            else if (itemRect.Y > movedViewport.Y && itemRect.Y + ItemHeight > movedViewport.Height && itemRect.Y < movedViewport.Height)
            {
                LineDown();
                return rectangle;
            }
            else if (itemRect.Y < movedViewport.Y && itemRect.Y + ItemHeight > movedViewport.Y)
            {
                LineUp();
                return rectangle;
            }

            SetVerticalOffset(itemRect.Y);

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
            SetVerticalOffset(VerticalOffset - viewport.Height);            
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + viewport.Height);          
        }

        #endregion IScrollInfo
    }
}
