using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public class Dialogs
    {
        public static string SaveFile(Window owner, string filter)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = filter,
                OverwritePrompt = true
            };

            var dialogResult = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            if (dialogResult == true)
            {
                return dialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string SaveFile(string filter)
        {
            return SaveFile(null, filter);
        }

        public static string SelectFolder(Window owner)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Title = "Select Folder..."
            };

            var dialogResult = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            if (dialogResult == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string SelectFile(Window owner, string filter)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = filter
            };

            var dialogResult = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            if (dialogResult == true)
            {
                return dialog.FileName;
            }
            else
            {
                return string.Empty;
            }
        }

        public static string SelectFile(string filter)
        {
            return SelectFile(null, filter);
        }

        public static string SelectIconFile(Window owner)
        {
            return SelectFile(owner, "Image Files (*.bmp, *.jpg, *.png, *.gif, *.ico)|*.bmp;*.jpg*;*.png;*.gif;*.ico|Executable (.exe)|*.exe");
        }

        public static string SelectIconFile()
        {
            return SelectIconFile(null);
        }

        public static string SelectImageFile(Window owner)
        {
            return SelectFile(owner, "Image Files (*.bmp, *.jpg, *.png, *.gif)|*.bmp;*.jpg*;*.png;*.gif");
        }

        public static string SelectImageFile()
        {
            return SelectIconFile(null);
        }
    }
}
