using Playnite.Extensions.Markup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheArtOfDev.HtmlRenderer.WPF;

namespace Playnite.Controls
{
    public class GridEx : Grid
    {
        #region RowCount Property

        /// <summary>
        /// Adds the specified number of Rows to RowDefinitions.
        /// Default Height is Auto
        /// </summary>
        public static readonly DependencyProperty RowCountProperty =
            DependencyProperty.Register(
                "RowCount", typeof(int), typeof(GridEx),
                new PropertyMetadata(-1, RowCountChanged));

        // Get
        public static int GetRowCount(DependencyObject obj)
        {
            return (int)obj.GetValue(RowCountProperty);
        }

        // Set
        public static void SetRowCount(DependencyObject obj, int value)
        {
            obj.SetValue(RowCountProperty, value);
        }

        // Change Event - Adds the Rows
        public static void RowCountChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.RowDefinitions.Clear();

            for (int i = 0; i < (int)e.NewValue; i++)
                grid.RowDefinitions.Add(
                    new RowDefinition() { Height = GridLength.Auto });

            SetStarRows(grid);
        }

        #endregion

        #region ColumnCount Property

        /// <summary>
        /// Adds the specified number of Columns to ColumnDefinitions.
        /// Default Width is Auto
        /// </summary>
        public static readonly DependencyProperty ColumnCountProperty =
            DependencyProperty.Register(
                "ColumnCount", typeof(int), typeof(GridEx),
                new PropertyMetadata(-1, ColumnCountChanged));

        // Get
        public static int GetColumnCount(DependencyObject obj)
        {
            return (int)obj.GetValue(ColumnCountProperty);
        }

        // Set
        public static void SetColumnCount(DependencyObject obj, int value)
        {
            obj.SetValue(ColumnCountProperty, value);
        }

        // Change Event - Add the Columns
        public static void ColumnCountChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || (int)e.NewValue < 0)
                return;

            Grid grid = (Grid)obj;
            grid.ColumnDefinitions.Clear();

            for (int i = 0; i < (int)e.NewValue; i++)
                grid.ColumnDefinitions.Add(
                    new ColumnDefinition() { Width = GridLength.Auto });

            SetStarColumns(grid);
        }

        #endregion

        #region StarRows Property

        /// <summary>
        /// Makes the specified Row's Height equal to Star.
        /// Can set on multiple Rows
        /// </summary>
        public static readonly DependencyProperty StarRowsProperty =
            DependencyProperty.Register(
                "StarRows", typeof(string), typeof(GridEx),
                new PropertyMetadata(string.Empty, StarRowsChanged));

        // Get
        public static string GetStarRows(DependencyObject obj)
        {
            return (string)obj.GetValue(StarRowsProperty);
        }

        // Set
        public static void SetStarRows(DependencyObject obj, string value)
        {
            obj.SetValue(StarRowsProperty, value);
        }

        // Change Event - Makes specified Row's Height equal to Star
        public static void StarRowsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            SetStarRows((Grid)obj);
        }

        #endregion

        #region StarColumns Property

        /// <summary>
        /// Makes the specified Column's Width equal to Star.
        /// Can set on multiple Columns
        /// </summary>
        public static readonly DependencyProperty StarColumnsProperty =
            DependencyProperty.Register(
                "StarColumns", typeof(string), typeof(GridEx),
                new PropertyMetadata(string.Empty, StarColumnsChanged));

        // Get
        public static string GetStarColumns(DependencyObject obj)
        {
            return (string)obj.GetValue(StarColumnsProperty);
        }

        // Set
        public static void SetStarColumns(DependencyObject obj, string value)
        {
            obj.SetValue(StarColumnsProperty, value);
        }

        // Change Event - Makes specified Column's Width equal to Star
        public static void StarColumnsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (!(obj is Grid) || string.IsNullOrEmpty(e.NewValue.ToString()))
                return;

            SetStarColumns((Grid)obj);
        }

        #endregion

        #region AutoLayoutColumns
        public static readonly DependencyProperty AutoLayoutColumnsProperty =
            DependencyProperty.Register(
                "AutoLayoutColumns", typeof(int), typeof(GridEx),
                new PropertyMetadata(-1, AutoLayoutColumnsChanged));

        public static int GetAutoLayoutColumns(DependencyObject obj)
        {
            return (int)obj.GetValue(AutoLayoutColumnsProperty);
        }

        public static void SetAutoLayoutColumns(DependencyObject obj, int value)
        {
            obj.SetValue(AutoLayoutColumnsProperty, value);
        }

        public static void AutoLayoutColumnsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            ((GridEx)obj).ArrangeChildren();
        }

        #endregion AutoLayoutColumns

        public GridEx() : base()
        {
        }

        internal void ArrangeChildren()
        {
            var columns = GetAutoLayoutColumns(this);
            if (columns == -1)
            {
                return;
            }

            var index = 0;
            foreach (UIElement elem in Children)
            {
                var span = Grid.GetColumnSpan(elem);
                SetColumn(elem, index % columns);
                SetRow(elem, index / columns);
                if (span > 0)
                {
                    index += span;
                }
                else
                {
                    index++;
                }
            }
        }

        private static void SetStarColumns(Grid grid)
        {
            var starColumns = GetStarColumns(grid).Split(',');
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                if (starColumns.Contains(i.ToString()))
                {
                    grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    grid.ColumnDefinitions[i].Width = new GridLength(1, GridUnitType.Auto);
                }
            }
        }

        private static void SetStarRows(Grid grid)
        {
            var starRows = GetStarRows(grid).Split(',');
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                if (starRows.Contains(i.ToString()))
                {
                    grid.RowDefinitions[i].Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    grid.RowDefinitions[i].Height = new GridLength(1, GridUnitType.Auto);
                }
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            ArrangeChildren();
            return base.MeasureOverride(constraint);
        }
    }
}
