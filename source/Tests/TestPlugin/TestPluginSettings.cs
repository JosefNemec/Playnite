using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    public class TestPluginSettings : ISettings
    {
        public string Option1 { get; set; }
        public int Option2 { get; set; }

        public void BeginEdit()
        {
        }

        public void CancelEdit()
        {
        }

        public void EndEdit()
        {
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = null;
            return true;
        }
    }
}
