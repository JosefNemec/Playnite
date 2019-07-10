using NUnit.Framework;
using Playnite.Common;
using Playnite.FullscreenApp.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp.Tests.Controls
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    public class FullscreenTilePanelTests
    {
        [Test]
        public void LayoutTest()
        {
            var viewport = new Size(1000, 500);
            var panel = new FullscreenTilePanel()
            {
                Rows = 2,
                ItemAspectRatio = new AspectRatio(2 ,1)
            };

            panel.UpdateScrollInfo(viewport);            

            //Assert.AreEqual(200, panel.GetItemHeight());
            //Assert.AreEqual(400, panel.GetItemWidth());
            //Assert.AreEqual(2, panel.Columns);
            //Assert.AreEqual(100, panel.GetSideMargin());
        }
    }
}
