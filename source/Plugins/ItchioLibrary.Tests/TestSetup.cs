using NUnit.Framework;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Tests
{
    [SetUpFixture]
    public class TestSetup
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            LogManager.Init(new NLogLogProvider());

        }

        [OneTimeTearDown]
        public void GlobalTeardown()
        {

        }
    }
}
