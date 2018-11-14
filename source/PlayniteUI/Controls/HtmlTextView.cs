using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace PlayniteUI.Controls
{
    public class HtmlTextView : HtmlPanel
    {
        private static string template;

        public int HtmlFontSize
        {
            get
            {
                return (int)GetValue(HtmlFontSizeProperty);
            }

            set
            {
                SetValue(HtmlFontSizeProperty, value);
            }
        }

        public static readonly DependencyProperty HtmlFontSizeProperty =
            DependencyProperty.Register("HtmlFontSize", typeof(int), typeof(HtmlTextView), new PropertyMetadata(11, OnHtmlFontSizeChange));

        private static void OnHtmlFontSizeChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as HtmlTextView;
            var size = (int)e.NewValue;
            var content = template;
            content = content.Replace("{text}", obj.HtmlText);
            content = content.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            content = content.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            content = content.Replace("{font_size}", size.ToString());
            obj.Text = content.Replace("{font_family}", obj.HtmlFontFamily.ToString());
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
            var font = e.NewValue.ToString();
            var content = template;
            content = content.Replace("{text}", obj.HtmlText);
            content = content.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            content = content.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            content = content.Replace("{font_size}", obj.HtmlFontSize.ToString());
            obj.Text = content.Replace("{font_family}", font);
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
            var color = (Color)e.NewValue;
            var content = template;
            content = content.Replace("{text}", obj.HtmlText);
            content = content.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            content = content.Replace("{font_family}", obj.HtmlFontFamily.ToString());
            content = content.Replace("{font_size}", obj.HtmlFontSize.ToString());
            obj.Text = content.Replace("{link_foreground}", color.ToHtml());
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
            var color = (Color)e.NewValue;
            var content = template;
            content = content.Replace("{text}", obj.HtmlText);
            content = content.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            content = content.Replace("{font_family}", obj.HtmlFontFamily.ToString());
            content = content.Replace("{font_size}", obj.HtmlFontSize.ToString());
            obj.Text = content.Replace("{foreground}", color.ToHtml());
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
            var content = template;
            content = content.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            content = content.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            content = content.Replace("{font_family}", obj.HtmlFontFamily.ToString());
            content = content.Replace("{font_size}", obj.HtmlFontSize.ToString());
            obj.Text = content.Replace("{text}", e.NewValue?.ToString());
        }

        static HtmlTextView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlTextView), new FrameworkPropertyMetadata(typeof(HtmlTextView)));
            template = Playnite.Resources.ReadFileFromResource("PlayniteUI.Resources.DescriptionView.html");
        }

        public HtmlTextView()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
