using Playnite;
using Playnite.Models;
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
            var template = Playnite.Resources.ReadFileFromResource("PlayniteUI.Resources.DescriptionView.html");
            template = template.Replace("{text}", obj.HtmlText);
            template = template.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            template = template.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            obj.Text = template.Replace("{font_family}", font);
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
            var template = Playnite.Resources.ReadFileFromResource("PlayniteUI.Resources.DescriptionView.html");
            template = template.Replace("{text}", obj.HtmlText);
            template = template.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            template = template.Replace("{font_family}", obj.HtmlFontFamily.ToString());
            obj.Text = template.Replace("{link_foreground}", color.ToHtml());
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
            var template = Playnite.Resources.ReadFileFromResource("PlayniteUI.Resources.DescriptionView.html");
            template = template.Replace("{text}", obj.HtmlText);
            template = template.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            template = template.Replace("{font_family}", obj.HtmlFontFamily.ToString());
            obj.Text = template.Replace("{foreground}", color.ToHtml());
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
            var template = Playnite.Resources.ReadFileFromResource("PlayniteUI.Resources.DescriptionView.html");
            template = template.Replace("{foreground}", obj.HtmlForeground.ToHtml());
            template = template.Replace("{link_foreground}", obj.LinkForeground.ToHtml());
            template = template.Replace("{font_family}", obj.HtmlFontFamily.ToString());
            obj.Text = template.Replace("{text}", e.NewValue?.ToString());
        }

        static HtmlTextView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HtmlTextView), new FrameworkPropertyMetadata(typeof(HtmlTextView)));
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
