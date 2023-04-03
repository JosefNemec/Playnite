using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Playnite;

public class SystemDialogs
{
    public static string SaveFile(Window? owner, string filter, bool promptOverwrite)
    {
        var dialog = new SaveFileDialog()
        {
            Filter = filter,
            OverwritePrompt = promptOverwrite
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

    public static string SaveFile(Window owner, string filter)
    {
        return SaveFile(owner, filter, true);
    }

    public static string SaveFile(string filter, bool promptOverwrite)
    {
        return SaveFile(null, filter, promptOverwrite);
    }

    public static string SaveFile(string filter)
    {
        return SaveFile(null, filter, true);
    }

    public static string? SelectFolder(Window? owner)
    {
        throw new NotImplementedException();
        // TODO: https://stackoverflow.com/a/66187224
        // TODO: use Vanara

        //var dialog = new CommonOpenFileDialog()
        //{
        //    IsFolderPicker = true
        //};

        //var dialogResult = owner == null ? dialog.ShowDialog() : dialog.ShowDialog(owner);
        //if (dialogResult == CommonFileDialogResult.Ok)
        //{
        //    return dialog.FileName;
        //}
        //else
        //{
        //    return null;
        //}
    }

    public static List<string>? SelectFiles(Window? owner, string filter)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = filter,
            Multiselect = true
        };

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

    public static List<string>? SelectFiles(string filter)
    {
        return SelectFiles(null, filter);
    }

    public static string SelectFile(Window? owner, string filter, string? initialDir = null)
    {
        var dialog = new OpenFileDialog()
        {
            Filter = filter
        };

        if (!initialDir.IsNullOrEmpty() && Directory.Exists(initialDir))
        {
            dialog.InitialDirectory = Path.GetFullPath(initialDir);
        }

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

    public static string SelectIconFile(Window? owner)
    {
        return SelectFile(owner, "Icon Files|*.bmp;*.jpg*;*.jpeg*;*.png;*.gif;*.ico;*.tga;*.exe;*.tif;*.webp");
    }

    public static string SelectIconFile()
    {
        return SelectIconFile(null);
    }

    public static string SelectImageFile(Window? owner)
    {
        return SelectFile(owner, "Image Files|*.bmp;*.jpg*;*.jpeg*;*.png;*.gif;*.tga;*.tif;*.webp");
    }

    public static string SelectImageFile()
    {
        return SelectIconFile(null);
    }
}
