using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models
{
    public enum LogLevel
    {        
        debug,
        info,
        warning,
        error
    }

    public class LogMessage
    {
        public LogLevel level;
        public string message;
    }
}
