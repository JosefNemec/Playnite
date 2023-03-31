using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Extensions
{
    [TestFixture]
    public class DateTimesTests
    {
        [Test]
        public void FutureDatesAreNotWeekdays()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var today = new DateTime(2023, 3, 24);
            var releaseDate = new ReleaseDate(2023, 6, 6);
            using (DateTimes.UseCustomDates(new CustomDateTimeProvider(today)))
            {
                var dateString = DateTimes.ToDisplayString(releaseDate, new ReleaseDateFormattingOptions("d", pastWeekRelativeFormat: true));
                Assert.AreEqual("6/6/2023", dateString);
            }
        }

        private class CustomDateTimeProvider : DateTimes.IDateTimes
        {
            private readonly DateTime dateTime;

            public CustomDateTimeProvider(DateTime dateTime)
            {
                this.dateTime = dateTime;
            }

            public DateTime Now => dateTime;

            public DateTime Today => dateTime.Date;
        }
    }
}
