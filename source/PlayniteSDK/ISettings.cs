using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface ISettings : IEditableObject
    {
        bool VerifySettings(out List<string> errors);
    }
}
