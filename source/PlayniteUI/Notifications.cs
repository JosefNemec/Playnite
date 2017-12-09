using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public enum NotificationType
    {
        Info,
        Error
    }

    public class NotificationMessage
    {
        public int Id
        {
            get; set;
        }

        public string Text
        {
            get; set;
        }

        public NotificationType Type
        {
            get; set;
        }

        public delegate void ClickDelegate();
        public ClickDelegate ClickAction
        {
            get; set;
        }

        public NotificationMessage(int id, string text, NotificationType type, ClickDelegate action)
        {
            Id = id;
            Text = text;
            Type = type;
            ClickAction = action;
        }
    }    

    public static class NotificationCodes
    {
        public static readonly int GOGLibDownloadError = 1;
        public static readonly int SteamLibDownloadError = 2;
        public static readonly int GOGLInstalledImportError = 3;
        public static readonly int SteamInstalledImportError = 4;
        public static readonly int OriginInstalledImportError = 5;
        public static readonly int OriginLibDownloadError = 6;
        public static readonly int UplayInstalledImportError = 7;
        public static readonly int BattleNetInstalledImportError = 8;
        public static readonly int BattleNetLibDownloadImportError = 9;
        public static readonly int SteamCatImportError = 10;
    }
}
