using NUnit.Framework;
using Playnite.Converters;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Tests.Converters
{
    [TestFixture]
    public class PlayTimeToStringConverterTests
    {
        private static string Convert(object value, object formatToDaysParameter = null)
        {
            return PlayTimeToStringConverter.Instance.Convert(value, typeof(string), formatToDaysParameter, CultureInfo.CurrentCulture) as string;
        }

        [Test]
        public void ConvertLocalizationStringsTest()
        {
            var LOCPlayedNone = ResourceProvider.GetString("LOCPlayedNone");
            var LOCPlayedSeconds = ResourceProvider.GetString("LOCPlayedSeconds");
            var LOCPlayedMinutes = ResourceProvider.GetString("LOCPlayedMinutes");
            var LOCPlayedHours = ResourceProvider.GetString("LOCPlayedHours");
            var LOCPlayedDays = ResourceProvider.GetString("LOCPlayedDays");

            ulong? zeroSeconds = 0;
            ulong? totalSecondsInMinute = 60;
            ulong? totalSecondsInHour = totalSecondsInMinute * 60;
            ulong? totalSecondsInDay = totalSecondsInHour * 24;

            // formatToDaysParameter set to null/false tests
            Assert.AreEqual(LOCPlayedNone, Convert(zeroSeconds, false));
            Assert.AreEqual(LOCPlayedSeconds, Convert(totalSecondsInMinute - 1, false));
            Assert.AreEqual(LOCPlayedMinutes, Convert(totalSecondsInMinute, false));
            Assert.AreEqual(LOCPlayedHours, Convert(totalSecondsInDay, false));

            // formatToDaysParameter set to true
            Assert.AreEqual(LOCPlayedNone, Convert(zeroSeconds, true));
            Assert.AreEqual(LOCPlayedSeconds, Convert(totalSecondsInMinute - 1, true));
            Assert.AreEqual(LOCPlayedMinutes, Convert(totalSecondsInMinute, true));
            Assert.AreEqual(LOCPlayedHours, Convert(totalSecondsInDay - 1, true));
            Assert.AreEqual(LOCPlayedDays, Convert(totalSecondsInDay, true));
        }
    }
}
