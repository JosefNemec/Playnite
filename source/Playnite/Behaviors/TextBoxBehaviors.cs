using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Playnite.Behaviors
{
    public class TextBoxBehavior
    {
        #region AttachDirectoryBrowser
        private static readonly DependencyProperty AttachDirectoryBrowserProperty =
            DependencyProperty.RegisterAttached(
                "AttachDirectoryBrowser",
                typeof(bool),
                typeof(TextBoxBehavior),
                new PropertyMetadata(new PropertyChangedCallback(AttachmentChanged)));

        public static bool GetAttachDirectoryBrowser(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachDirectoryBrowserProperty);
        }

        public static void SetAttachDirectoryBrowser(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachDirectoryBrowserProperty, value);
        }
        #endregion AttachDirectoryBrowser

        #region FileBrowserAttachment
        private static readonly DependencyProperty FileBrowserAttachmentProperty =
            DependencyProperty.RegisterAttached(
                "FileBrowserAttachment",
                typeof(string),
                typeof(TextBoxBehavior),
                new PropertyMetadata(new PropertyChangedCallback(AttachmentChanged)));

        public static string GetFileBrowserAttachment(DependencyObject obj)
        {
            return (string)obj.GetValue(FileBrowserAttachmentProperty);
        }

        public static void SetFileBrowserAttachment(DependencyObject obj, string value)
        {
            obj.SetValue(FileBrowserAttachmentProperty, value);
        }
        #endregion FileBrowserAttachment

        private static void AttachmentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (!(obj is TextBox textBox) || DesignerProperties.GetIsInDesignMode(textBox))
            {
                return;
            }

            textBox.MouseEnter -= TextBox_MouseEnter;
            textBox.MouseLeave -= TextBox_MouseLeave;
            if (!GetFileBrowserAttachment(obj).IsNullOrEmpty() || GetAttachDirectoryBrowser(obj))
            {
                AttachPopup(textBox);
                textBox.MouseEnter += TextBox_MouseEnter;
                textBox.MouseLeave += TextBox_MouseLeave;
            }
            else
            {
                DettachPopup(textBox);
            }
        }

        private static void TextBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox box && box.GetValue(AttachedPopupProperty) is Popup popup)
            {
                popup.IsOpen = false;
            }
        }

        private static void TextBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is TextBox box && box.GetValue(AttachedPopupProperty) is Popup popup)
            {
                popup.IsOpen = true;
            }
        }

        private static void AttachPopup(TextBox box)
        {
            Button createButton(int icon, string tooltip, Func<string> func)
            {
                var btn = new Button
                {
                    Content = char.ConvertFromUtf32(icon),
                    FontFamily = new FontFamily("Segoe UI Symbol"),
                    ToolTip = tooltip
                };

                btn.Click += (_, __) =>
                {
                    var path = func();
                    if (!path.IsNullOrEmpty())
                    {
                        box.Clear();
                        box.AppendText(path);
                    }
                };

                return btn;
            }

            var fileAttachment = GetFileBrowserAttachment(box);
            var attachFile = !fileAttachment.IsNullOrEmpty();
            var attachDirectory = GetAttachDirectoryBrowser(box);
            DettachPopup(box);
            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            if (attachFile)
            {
                stack.Children.Add(
                    createButton(0xE132,
                    ResourceProvider.GetString(LOC.SelectFileTooltip),
                    () => Dialogs.SelectFile(fileAttachment)));
            }

            if (attachDirectory)
            {
                stack.Children.Add(
                    createButton(0xE188,
                    ResourceProvider.GetString(LOC.SelectDirectoryTooltip),
                    () => Dialogs.SelectFolder()));
            }

            var popup = new Popup { Child = stack };
            popup.PlacementTarget = box;
            popup.Placement = PlacementMode.Bottom;
            popup.MouseEnter += (_, __) => popup.IsOpen = true;
            popup.MouseLeave += (_, __) => popup.IsOpen = false;
            box.SetValue(AttachedPopupProperty, popup);
        }

        private static void DettachPopup(TextBox box)
        {
            box.ClearValue(AttachedPopupProperty);
        }

        public static readonly DependencyProperty AttachedPopupProperty =
            DependencyProperty.RegisterAttached("AttachedPopup", typeof(Popup), typeof(TextBoxBehavior));
    }
}
