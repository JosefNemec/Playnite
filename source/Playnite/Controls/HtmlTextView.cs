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
    public class HtmlTextView : HtmlPanel
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

        internal string templateContent = string.Empty;

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

        internal void UpdateTextContent()
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
            Text = content.Replace("{text}", HtmlText?.ToString());
        }

        static HtmlTextView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlTextView), new FrameworkPropertyMetadata(typeof(HtmlTextView)));
        }

        public HtmlTextView()
        {
            Background = Brushes.Transparent;

            // Always use LTR because HtmlPanel doesn't support RTL properly
            FlowDirection = FlowDirection.LeftToRight;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
