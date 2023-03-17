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
    public class HtmlTextView : StackPanel
    {
        private const string defaultTemplate = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <style type=""text/css"">
        HTML,BODY
        {
            color: {foreground};
            font-family: ""{font_family}"";
            font-size: {font_size}px;
            margin: 0;
            padding: 0;
        }

        a {
            color: {link_foreground};
            text-decoration: none;
        }

        img {
            max-width: 100%;
        }
    </style>
    <title>Game Description</title>
</head>
<body>
<div>
{text}
</div>
</body>
</html>";

        private int currentLoadedLength = 0;
        private readonly int loadPartLength = 10_000;
        internal string templateContent = string.Empty;
        private readonly HtmlPanel htmlPanel;
        private readonly Button moreButton;

        public string TemplatePath
        {
            get
            {
                return (string)GetValue(TemplatePathProperty);
            }

            set
            {
                SetValue(TemplatePathProperty, value);
            }
        }

        public static readonly DependencyProperty TemplatePathProperty =
            DependencyProperty.Register(
                "TemplatePath",
                typeof(string),
                typeof(HtmlTextView),
                new PropertyMetadata(null, TemplatePathChange));

        private static void TemplatePathChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            if (e.NewValue is string path)
            {
                if (path.IsNullOrEmpty())
                {
                    obj.templateContent = string.Empty;
                }
                else if (File.Exists(path))
                {
                    obj.templateContent = File.ReadAllText(path);
                }

                obj.UpdateTextContent();
            }
        }

        public double HtmlFontSize
        {
            get
            {
                return (double)GetValue(HtmlFontSizeProperty);
            }

            set
            {
                SetValue(HtmlFontSizeProperty, value);
            }
        }

        public static readonly DependencyProperty HtmlFontSizeProperty =
            DependencyProperty.Register("HtmlFontSize", typeof(double), typeof(HtmlTextView), new PropertyMetadata(11.0, OnHtmlFontSizeChange));

        private static void OnHtmlFontSizeChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            if (e.NewValue is double size)
            {
                obj.UpdateTextContent();
            }
        }

        public FontFamily HtmlFontFamily
        {
            get
            {
                return (FontFamily)GetValue(HtmlFontFamilyProperty);
            }

            set
            {
                SetValue(HtmlFontFamilyProperty, value);
            }
        }

        public static readonly DependencyProperty HtmlFontFamilyProperty =
            DependencyProperty.Register("HtmlFontFamily", typeof(FontFamily), typeof(HtmlTextView), new PropertyMetadata(new FontFamily("Trebuchet MS"), OnHtmlFontFamilyChange));

        private static void OnHtmlFontFamilyChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            obj.UpdateTextContent();
        }

        public Color LinkForeground
        {
            get
            {
                return (Color)GetValue(LinkForegroundProperty);
            }

            set
            {
                SetValue(LinkForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty LinkForegroundProperty =
            DependencyProperty.Register("LinkForeground", typeof(Color), typeof(HtmlTextView), new PropertyMetadata(Colors.White, OnLinkForegroundChange));

        private static void OnLinkForegroundChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            if (e.NewValue is Color color)
            {
                obj.UpdateTextContent();
            }
        }

        public Color HtmlForeground
        {
            get
            {
                return (Color)GetValue(HtmlForegroundProperty);
            }

            set
            {
                SetValue(HtmlForegroundProperty, value);
            }
        }

        public static readonly DependencyProperty HtmlForegroundProperty =
            DependencyProperty.Register("HtmlForeground", typeof(Color), typeof(HtmlTextView), new PropertyMetadata(Colors.Black, OnHtmlForegroundChange));

        private static void OnHtmlForegroundChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            if (e.NewValue is Color color)
            {
                obj.UpdateTextContent();
            }
        }

        public string HtmlText
        {
            get
            {
                return (string)GetValue(HtmlTextProperty);
            }

            set
            {
                SetValue(HtmlTextProperty, value);
            }
        }

        public static readonly DependencyProperty HtmlTextProperty =
            DependencyProperty.Register("HtmlText", typeof(string), typeof(HtmlTextView), new PropertyMetadata(string.Empty, OnHtmlTextChange));

        private static void OnHtmlTextChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            obj.UpdateTextContent();
        }

        public bool PartialLoadEnabled
        {
            get
            {
                return (bool)GetValue(PartialLoadEnabledProperty);
            }

            set
            {
                SetValue(PartialLoadEnabledProperty, value);
            }
        }

        public static readonly DependencyProperty PartialLoadEnabledProperty =
            DependencyProperty.Register(
                "PartialLoadEnabled",
                typeof(bool),
                typeof(HtmlTextView),
                new PropertyMetadata(true));

        // These are for theme backwards compatibility since this control is no longer Control but FrameworkElement
        public FontStyle FontStyle { get; set; }
        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(HtmlTextView));

        public FontStretch FontStretch { get; set; }
        public static readonly DependencyProperty FontStretchProperty = DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(HtmlTextView));

        public double FontSize { get; set; }
        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(HtmlTextView));

        public FontFamily FontFamily { get; set; }
        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(HtmlTextView));

        public Brush Foreground { get; set; }
        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(HtmlTextView));

        public Thickness BorderThickness { get; set; }
        public static readonly DependencyProperty BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(HtmlTextView));

        public bool IsTabStop { get; set; }
        public static readonly DependencyProperty IsTabStopProperty = DependencyProperty.Register("IsTabStop", typeof(bool), typeof(HtmlTextView));

        public VerticalAlignment VerticalContentAlignment { get; set; }
        public static readonly DependencyProperty VerticalContentAlignmentProperty = DependencyProperty.Register("VerticalContentAlignment", typeof(VerticalAlignment), typeof(HtmlTextView));

        public int TabIndex { get; set; }
        public static readonly DependencyProperty TabIndexProperty = DependencyProperty.Register("TabIndex", typeof(int), typeof(HtmlTextView));

        public Thickness Padding { get; set; }
        public static readonly DependencyProperty PaddingProperty = DependencyProperty.Register("Padding", typeof(Thickness), typeof(HtmlTextView));

        public FontWeight FontWeight { get; set; }
        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(HtmlTextView));

        public Brush BorderBrush { get; set; }
        public static readonly DependencyProperty BorderBrushProperty = DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(HtmlTextView));

        public HorizontalAlignment HorizontalContentAlignment { get; set; }
        public static readonly DependencyProperty HorizontalContentAlignmentProperty = DependencyProperty.Register("HorizontalContentAlignment", typeof(HorizontalAlignment), typeof(HtmlTextView));

        internal void UpdateTextContent()
        {
            currentLoadedLength = loadPartLength;
            if (PartialLoadEnabled && !HtmlText.IsNullOrEmpty() && HtmlText.Length > loadPartLength)
            {
                moreButton.Visibility = Visibility.Visible;
                SetHtmlContent(HtmlText.Substring(0, loadPartLength));
            }
            else
            {
                moreButton.Visibility = Visibility.Hidden;
                SetHtmlContent(HtmlText ?? string.Empty);
            };
        }

        internal void SetHtmlContent(string htmlContent)
        {
            var content = string.Empty;
            if (!templateContent.IsNullOrEmpty())
            {
                content = templateContent;
            }
            else if (HtmlText?.Contains("<html>") != true)
            {
                content = defaultTemplate;
            }

            content = content.Replace("{foreground}", HtmlForeground.ToHtml());
            content = content.Replace("{link_foreground}", LinkForeground.ToHtml());
            content = content.Replace("{font_family}", HtmlFontFamily.ToString());
            content = content.Replace("{font_size}", HtmlFontSize.ToString());
            htmlPanel.Text = content.Replace("{text}", htmlContent);
        }

        static HtmlTextView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlTextView), new FrameworkPropertyMetadata(typeof(HtmlTextView)));
        }

        public HtmlTextView()
        {
            htmlPanel = new HtmlPanel();
            htmlPanel.Background = Brushes.Transparent;

            // Always use LTR because HtmlPanel doesn't support RTL properly
            FlowDirection = FlowDirection.LeftToRight;

            // This makes performance way better to leave scrolling to be handled by the parent layout
            ScrollViewer.SetHorizontalScrollBarVisibility(htmlPanel, ScrollBarVisibility.Disabled);
            ScrollViewer.SetVerticalScrollBarVisibility(htmlPanel, ScrollBarVisibility.Disabled);

            moreButton = new Button
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Content = LOC.LoadMore.GetLocalized(),
                Visibility = Visibility.Hidden,
                Margin = new Thickness(0, 0, 0, 5)
            };

            moreButton.Click += (_, __) =>
            {
                currentLoadedLength += loadPartLength;
                if (currentLoadedLength > HtmlText.Length)
                {
                    moreButton.Visibility = Visibility.Hidden;
                    SetHtmlContent(HtmlText);
                }
                else
                {
                    SetHtmlContent(HtmlText.Substring(0, currentLoadedLength));
                }
            };

            Children.Add(htmlPanel);
            Children.Add(moreButton);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
