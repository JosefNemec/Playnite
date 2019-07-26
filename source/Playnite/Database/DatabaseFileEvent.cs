using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public enum FileEvent
    {
        Added,
        Removed
    }

    public class DatabaseFileEventArgs : EventArgs
    {
        public string FileId
        {
            get;
        }

        public FileEvent EventType
        {
            get;
        }

        public DatabaseFileEventArgs(string fileId, FileEvent eventType)
        {
            FileId = fileId;
            EventType = eventType;
        }
    }
}
