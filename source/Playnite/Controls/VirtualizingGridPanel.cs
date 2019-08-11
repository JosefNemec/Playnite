using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System;
using System.Diagnostics;
using System.Collections.Specialized;
using System.ComponentModel;
using Playnite;

namespace Playnite.Controls
{
    public class VirtualizingGridPanel : VirtualizingPanel, IScrollInfo
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public int Columns
        {
            get { return (int)GetValue(ColumnsProperty); }
            set {
                SetValue(ColumnsProperty, value);
            }
        }

        public int Rows
        {
            get { return (int)GetValue(RowsProperty); }
            set
            {
                SetValue(RowsProperty, value);
            }
        }

        public static readonly DependencyProperty RowsProperty
            = DependencyProperty.RegisterAttached(nameof(Rows), typeof(int), typeof(VirtualizingGridPanel), new FrameworkPropertyMetadata(1, OnItemsSourceChanged));

        public static readonly DependencyProperty ColumnsProperty
            = DependencyProperty.RegisterAttached(nameof(Columns), typeof(int), typeof(VirtualizingGridPanel), new FrameworkPropertyMetadata(1, OnItemsSourceChanged));

        public double ItemWidth
        {
            get
            {
                return Math.Floor(ViewportWidth / Columns);
            }
        }

        public double ItemHeight
        {
            get
            {
                return Math.Floor(ViewportHeight / Rows);
            }
        }

        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var panel = obj as VirtualizingGridPanel;
            if (panel._itemsControl == null)
            {
                return;
            }

            panel.InvalidateMeasure();
            panel._owner?.InvalidateScrollInfo();

            if (panel.currentlyVisible != null)
            {
                var index = panel.GeneratorContainer.IndexFromContainer(panel.currentlyVisible);
                if (index >= 0)
                {
                    panel.MakeVisible(panel.currentlyVisible, new Rect(new Size(panel.ItemWidth, panel.ItemHeight)));
                }
                else
                {
                    panel.SetVerticalOffset(0);
                }
            }
        }

        private IRecyclingItemContainerGenerator Generator;

        private ItemContainerGenerator GeneratorContainer
        {
            get => (ItemContainerGenerator)Generator;
        }

        public ItemsControl _itemsControl;

        public VirtualizingGridPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                Dispatcher.BeginInvoke((Action)delegate
                {
                    _itemsControl = ItemsControl.GetItemsOwner(this);
                    Generator = (IRecyclingItemContainerGenerator)ItemContainerGenerator;
                    InvalidateMeasure();
                });
            }

            // For use in the IScrollInfo implementation
            this.RenderTransform = _trans;
        }

        /// <summary>
        /// Measure the children
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns>Size desired</returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_itemsControl == null)
            {
                if (availableSize.Width == double.PositiveInfinity || availableSize.Height == double.PositiveInfinity)
                {
                    return Size.Empty;
                }
                else
                {
                    return availableSize;
                }
            }

            UpdateScrollInfo(availableSize);

            // Figure out range that's visible based on layout algorithm            
            GetVisibleRange(out var firstVisibleItemIndex, out var lastVisibleItemIndex);
            if (lastVisibleItemIndex < 0)
            {
                return availableSize;
            }

            // We need to access InternalChildren before the generator to work around a bug
            UIElementCollection children = this.InternalChildren;

            CleanUpItems(firstVisibleItemIndex, lastVisibleItemIndex);

            // Get the generator position of the first visible data item
            GeneratorPosition startPos = Generator.GeneratorPositionFromIndex(firstVisibleItemIndex);

            // Get index where we'd insert the child for this position. If the item is realized
            // (position.Offset == 0), it's just position.Index, otherwise we have to add one to
            // insert after the corresponding child
            int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

            using (Generator.StartAt(startPos, GeneratorDirection.Forward, true))
            {
                for (int itemIndex = firstVisibleItemIndex; itemIndex <= lastVisibleItemIndex; ++itemIndex, ++childIndex)
                {
                    // Get or create the child
                    UIElement child = Generator.GenerateNext(out var newlyRealized) as UIElement;
                    if (child == null)
                    {
                        continue;
                    }

                    if (newlyRealized)
                    {
                        // Figure out if we need to insert the child at the end or somewhere in the middle
                        if (childIndex >= children.Count)
                        {
                            AddInternalChild(child);
                        }
                        else
                        {
                            InsertInternalChild(childIndex, child);
                        }

                        Generator.PrepareItemContainer(child);
                    }
                    //If we get a recycled element
                    else if (!InternalChildren.Contains(child))
                    {
                        InsertInternalChild(childIndex, child);
                        ItemContainerGenerator.PrepareItemContainer(child);
                    }

                    // Measurements will depend on layout algorithm
                    child.Measure(GetInitialChildSize(child));
                }
            }

            return availableSize;
        }

        /// <summary>
        /// Revirtualize items that are no longer visible
        /// </summary>
        /// <param name="minDesiredGenerated">first item index that should be visible</param>
        /// <param name="maxDesiredGenerated">last item index that should be visible</param>
        private void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
        {
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                GeneratorPosition childGeneratorPosition = new GeneratorPosition(i, 0);
                int iIndex = ItemContainerGenerator.IndexFromGeneratorPosition(childGeneratorPosition);
                if ((iIndex < minDesiredGenerated || iIndex > maxDesiredGenerated) && iIndex > 0)
                {
                    try
                    {
                        Generator.Recycle(childGeneratorPosition, 1);
                        RemoveInternalChildRange(i, 1);
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        // There are some weird null-reference crash reports from Generator.Recycle
                        logger.Error(e, $"Failed to recycle item {iIndex}, {minDesiredGenerated}, {maxDesiredGenerated}, {i}.");
                    }
                }
            }
        }

        /// <summary>
        /// Arrange the children
        /// </summary>
        /// <param name="finalSize">Size available</param>
        /// <returns>Size used</returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            for (int i = 0; i < Children.Count; i++)
            {
                UIElement child = Children[i];

                // Map the child offset to an item offset
                int itemIndex = Generator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));

                ArrangeChild(itemIndex, child, finalSize);
            }

            UpdateScrollInfo(finalSize);
            return finalSize;
        }

        /// <summary>
        /// When items are removed, remove the corresponding UI if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
        {            
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (args.Position.Index < 0)
                    {
                        InvalidateMeasure();
                        _owner?.InvalidateScrollInfo();
                        SetVerticalOffset(0);
                    }
                    else
                    {
                        RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    InvalidateMeasure();
                    _owner?.InvalidateScrollInfo();
                    SetVerticalOffset(0);
                    break;
            }
        }

        #region Layout specific code
        // I've isolated the layout specific code to this region. If you want to do something other than tiling, this is
        // where you'll make your changes

        /// <summary>
        /// Get the range of children that are visible
        /// </summary>
        /// <param name="firstVisibleItemIndex">The item index of the first visible item</param>
        /// <param name="lastVisibleItemIndex">The item index of the last visible item</param>
        private void GetVisibleRange(out int firstVisibleItemIndex, out int lastVisibleItemIndex)
        {
            int itemCount = _itemsControl.HasItems ? _itemsControl.Items.Count : 0;
            if (itemCount == 0)
            {
                firstVisibleItemIndex = -1;
                lastVisibleItemIndex = -1;
                return;
            }

            var previousRows = Math.Ceiling(_offset.Y / ItemHeight);
            var origFirst = (int)Math.Ceiling(previousRows * Columns);
            firstVisibleItemIndex = origFirst;
            if (firstVisibleItemIndex - Columns >= 0)
            {
                firstVisibleItemIndex = firstVisibleItemIndex - Columns;
            }
 
            lastVisibleItemIndex = (origFirst - 1) + (Columns * Rows);
            if (lastVisibleItemIndex >= itemCount)
            {
                lastVisibleItemIndex = itemCount;
            }
            else if (lastVisibleItemIndex + Columns <= itemCount)
            {
                lastVisibleItemIndex = lastVisibleItemIndex + Columns;
            }

            if (Rows == 1)
            {
                if (firstVisibleItemIndex - Columns >= 0)
                {
                    firstVisibleItemIndex -= Columns;
                }

                if (lastVisibleItemIndex + Columns <= itemCount)
                {
                    lastVisibleItemIndex += Columns;
                }
            }
        }

        /// <summary>
        /// Get the size of the children. We assume they are all the same
        /// </summary>
        /// <returns>The size</returns>
        private Size GetInitialChildSize(UIElement child)
        {
            return new Size(ItemWidth, ItemHeight);
        }

        private int GetItemRow(int itemIndex, int itemPerRow)
        {
            int column = itemIndex % itemPerRow;
            return itemIndex < column ? 0 : (int)Math.Floor(itemIndex / (double)itemPerRow);
        }

        /// <summary>
        /// Position a child
        /// </summary>
        /// <param name="itemIndex">The data item index of the child</param>
        /// <param name="child">The element to position</param>
        /// <param name="finalSize">The size of the panel</param>
        private void ArrangeChild(int itemIndex, UIElement child, Size finalSize)
        {
            int column = itemIndex % Columns;
            int row = GetItemRow(itemIndex, Columns);
            var targetRect = new Rect(
                column * ItemWidth,
                GetTotalHeightForRow(row),
                ItemWidth,
                ItemHeight);

            child.Arrange(targetRect);
        }

        /// <summary>
        /// Helper function for tiling layout
        /// </summary>
        /// <param name="availableSize">Size available</param>
        /// <returns></returns>
        private int CalculateChildrenPerRow(Size availableSize)
        {
            if (!Double.IsNaN(Columns))
            {                
                return Convert.ToInt32(Columns);
            }

            // Figure out how many children fit on each row
            int childrenPerRow;
            if (availableSize.Width == Double.PositiveInfinity)
            {
                childrenPerRow = Children.Count;
            }
            else
            {
                childrenPerRow = Math.Max(1, (int)Math.Floor(availableSize.Width / ItemWidth));
            }

            return childrenPerRow;
        }

        #endregion

        #region IScrollInfo implementation
        // See Ben Constable's series of posts at http://blogs.msdn.com/bencon/

        private double GetTotalHeightForRow(int row)
        {
            return (ItemHeight * row);
        }

        private double GetTotalHeight(Size availableSize)
        {
            int itemCount = _itemsControl.HasItems ? _itemsControl.Items.Count : 0;
            var totalRows = (int)Math.Ceiling(itemCount / (double)Columns);
            double totalHeight = totalRows * ItemHeight;
            return totalHeight;
        }

        private void UpdateScrollInfo(Size availableSize)
        {
            if (_itemsControl == null)
            {
                return;
            }

            // See how many items there are
            int itemCount = _itemsControl.HasItems ? _itemsControl.Items.Count : 0;
            var totalHeight = GetTotalHeight(availableSize);

            if (_offset.Y > totalHeight)
            {
                _offset.Y = 0;
                _trans.Y = 0;
            }

            Size extent = new Size(Columns * ItemWidth, totalHeight);            
            
            // Update extent
            if (extent != _extent)
            {
                _extent = extent;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }

            // Update viewport
            if (availableSize != _viewport)
            {
                _viewport = availableSize;
                if (_owner != null)
                    _owner.InvalidateScrollInfo();
            }
        }

        public ScrollViewer ScrollOwner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public bool CanHorizontallyScroll
        {
            get { return _canHScroll; }
            set { _canHScroll = value; }
        }

        public bool CanVerticallyScroll
        {
            get { return _canVScroll; }
            set { _canVScroll = value; }
        }

        public double HorizontalOffset
        {
            get { return _offset.X; }
        }

        public double VerticalOffset
        {
            get { return _offset.Y; }
        }

        public double ExtentHeight
        {
            get { return _extent.Height; }
        }

        public double ExtentWidth
        {
            get { return _extent.Width; }
        }

        public double ViewportHeight
        {
            get { return _viewport.Height; }
        }

        public double ViewportWidth
        {
            get { return _viewport.Width; }
        }

        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - ItemHeight);
        }

        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + ItemHeight);
        }

        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - (ItemHeight * Rows));
        }

        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + (ItemHeight * Rows));
        }

        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - ItemHeight);
        }

        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + ItemHeight);
        }

        public void LineLeft()
        {

        }

        public void LineRight()
        {

        }

        private Visual currentlyVisible;

        public Rect MakeVisible(Visual visual, Rect rectangle)
        {
            var defaultRect = new Rect(new Size(ItemWidth, ItemHeight)); ;
            var index = GeneratorContainer.IndexFromContainer(visual);
            if (index < 0)
            {
                return rectangle;
            }

            currentlyVisible = visual;
            var perRow = Columns;
            var row = GetItemRow(index, perRow);
            var offset = GetTotalHeightForRow(row);            
            var offsetSize = offset + ItemHeight;
            var offsetBottom = _offset.Y + _viewport.Height;
            if (offset > _offset.Y && offsetSize < offsetBottom)
            {
                return defaultRect;
            }
            else if (offset > _offset.Y && (offsetBottom - offset < ItemHeight))
            {
                offset = _offset.Y + (ItemHeight - (offsetBottom - offset));
            }
            else if (Math.Floor((offsetBottom - offset)) == Math.Floor(ItemHeight))
            {
                return defaultRect;
            }

            var maxOffset = GetTotalHeight(new Size(1, 1)) - (Rows * ItemHeight);
            if (offset > 0 && offset > maxOffset)
            {
                _offset.Y = maxOffset;
            }
            else
            {
                _offset.Y = offset;
            }

            _trans.Y = -_offset.Y;
            InvalidateMeasure();
            return defaultRect;
        }        

        public void MouseWheelLeft()
        {

        }

        public void MouseWheelRight()
        {

        }

        public void PageLeft()
        {

        }

        public void PageRight()
        {

        }

        public void SetHorizontalOffset(double offset)
        {

        }

        public void SetVerticalOffset(double offset)
        {
            // Forces srolling per whole item
            var line = Math.Round(offset / ItemHeight, MidpointRounding.AwayFromZero);
            offset = line * ItemHeight;

            if (offset < 0 || _viewport.Height >= _extent.Height)
            {
                offset = 0;
            }
            else
            {
                if (offset + _viewport.Height >= _extent.Height)
                {
                    offset = _extent.Height - _viewport.Height;
                }
            }

            _offset.Y = offset;

            if (_owner != null)
                _owner.InvalidateScrollInfo();

            _trans.Y = -offset;
            InvalidateMeasure();
        }

        private TranslateTransform _trans = new TranslateTransform();
        private ScrollViewer _owner;
        private bool _canHScroll = false;
        private bool _canVScroll = false;
        private Size _extent = new Size(0, 0);
        private Size _viewport = new Size(0, 0);
        private Point _offset;

        #endregion

    }
}
