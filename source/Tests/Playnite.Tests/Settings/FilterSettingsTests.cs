using NUnit.Framework;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Settings
{
    [TestFixture]
    public class FilterSettingsTests
    {
        [Test]
        public void ShouldSerializeTest()
        {
            var settings = new FilterSettings();

            var json = Serialization.ToJson(settings);
            StringAssert.DoesNotContain(nameof(FilterSettings.Name), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Series), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Source), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.AgeRating), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Region), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Genre), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Publisher), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Developer), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Category), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Tag), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Platform), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Library), json);
            StringAssert.DoesNotContain(nameof(FilterSettings.Feature), json);

            settings.Name = "test";
            settings.Series = new FilterItemProperites() { Text = "test" };
            settings.Source = new FilterItemProperites() { Text = "test" };
            settings.AgeRating = new FilterItemProperites() { Text = "test" };
            settings.Region = new FilterItemProperites() { Text = "test" };
            settings.Genre = new FilterItemProperites() { Text = "test" };
            settings.Publisher = new FilterItemProperites() { Text = "test" };
            settings.Developer = new FilterItemProperites() { Text = "test" };
            settings.Category = new FilterItemProperites() { Text = "test" };
            settings.Tag = new FilterItemProperites() { Text = "test" };
            settings.Platform = new FilterItemProperites() { Text = "test" };
            settings.Library = new FilterItemProperites() { Text = "test" };
            settings.Feature = new FilterItemProperites() { Text = "test" };
            json = Serialization.ToJson(settings);

            StringAssert.Contains(nameof(FilterSettings.Name), json);
            StringAssert.Contains(nameof(FilterSettings.Series), json);
            StringAssert.Contains(nameof(FilterSettings.Source), json);
            StringAssert.Contains(nameof(FilterSettings.AgeRating), json);
            StringAssert.Contains(nameof(FilterSettings.Region), json);
            StringAssert.Contains(nameof(FilterSettings.Genre), json);
            StringAssert.Contains(nameof(FilterSettings.Publisher), json);
            StringAssert.Contains(nameof(FilterSettings.Developer), json);
            StringAssert.Contains(nameof(FilterSettings.Category), json);
            StringAssert.Contains(nameof(FilterSettings.Tag), json);
            StringAssert.Contains(nameof(FilterSettings.Platform), json);
            StringAssert.Contains(nameof(FilterSettings.Library), json);
            StringAssert.Contains(nameof(FilterSettings.Feature), json);
        }
    }
}
