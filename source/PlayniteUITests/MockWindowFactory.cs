using PlayniteUI;
using PlayniteUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests
{
    public class MockWindowFactory : IWindowFactory
    {
        public WindowBase Window
        {
            get;
        }

        public void BringToForeground()
        {

        }

        public void Close()
        {

        }

        public void Close(bool? resutl)
        {

        }

        public bool? CreateAndOpenDialog(object dataContext)
        {
            return null;
        }

        public void RestoreWindow()
        {

        }

        public void Show(object dataContext)
        {

        }
    }
}
