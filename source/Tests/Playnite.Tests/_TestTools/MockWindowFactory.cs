using Playnite.Controls;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    public class MockWindowFactory : IWindowFactory
    {
        public bool IsClosed
        {
            get;
        }

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
