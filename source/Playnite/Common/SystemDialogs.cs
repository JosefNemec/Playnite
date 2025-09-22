using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Common
{
    public class SystemDialogs
    {
        public static string SaveFile(Window owner, string filter, bool promptOverwrite, string initialDir = null)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = filter,
                OverwritePrompt = promptOverwrite
            };

            if (initialDir != null && Directory.Exists(initialDir))
                dialog.InitialDirectory = initialDir;

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

        public static string SaveFile(Window owner, string filter, string initialDir = null)
        {
            return SaveFile(owner, filter, true, initialDir);
        }

        public static string SaveFile(string filter, bool promptOverwrite, string initialDir = null)
        {
            return SaveFile(null, filter, promptOverwrite, initialDir);
        }

        public static string SaveFile(string filter, string initialDir = null)
        {
            return SaveFile(null, filter, true, initialDir);
        }

        public static string SelectFolder(Window owner, string initialDir = null)
        {
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (initialDir != null && Directory.Exists(initialDir))
                dialog.InitialDirectory = initialDir;

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

        public static List<string> SelectFiles(Window owner, string filter, string initialDir = null)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = filter,
                Multiselect = true
            };

            if (!initialDir.IsNullOrWhiteSpace() && Directory.Exists(initialDir))
                dialog.InitialDirectory = initialDir;

            var dialogResult = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
            if (dialogResult == true)
            {
                return dialog.FileNames.ToList();
            }
            else
            {
                return null;
            }
        }

        public static List<string> SelectFiles(string filter)
        {
            return SelectFiles(null, filter);
        }

        public static string SelectFile(Window owner, string filter, string initialDir = null)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = filter
            };

            if (!initialDir.IsNullOrWhiteSpace() && Directory.Exists(initialDir))
                dialog.InitialDirectory = initialDir;

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

        public static string SelectFile(string filter, string initialDir = null)
        {
            return SelectFile(null, filter, initialDir);
        }

        public static string SelectIconFile(Window owner, string initialDir = null)
        {
            return SelectFile(owner, "Icon Files|*.bmp;*.jpg*;*.jpeg*;*.png;*.gif;*.ico;*.tga;*.exe;*.tif;*.webp;*.avif", initialDir);
        }

        public static string SelectIconFile(string initialDir = null)
        {
            return SelectIconFile(null, initialDir);
        }

        public static string SelectImageFile(Window owner, string initialDir = null)
        {
            return SelectFile(owner, "Image Files|*.bmp;*.jpg*;*.jpeg*;*.png;*.gif;*.tga;*.tif;*.webp;*.avif", initialDir);
        }

        public static string SelectImageFile(string initialDir = null)
        {
            return SelectIconFile(null, initialDir);
        }
    }
}
