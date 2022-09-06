using NUnit.Framework;
using Playnite;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SdkModels = Playnite.SDK.Models;

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
            settings.Series = new IdItemFilterItemProperties() { Text = "test" };
            settings.Source = new IdItemFilterItemProperties() { Text = "test" };
            settings.AgeRating = new IdItemFilterItemProperties() { Text = "test" };
            settings.Region = new IdItemFilterItemProperties() { Text = "test" };
            settings.Genre = new IdItemFilterItemProperties() { Text = "test" };
            settings.Publisher = new IdItemFilterItemProperties() { Text = "test" };
            settings.Developer = new IdItemFilterItemProperties() { Text = "test" };
            settings.Category = new IdItemFilterItemProperties() { Text = "test" };
            settings.Tag = new IdItemFilterItemProperties() { Text = "test" };
            settings.Platform = new IdItemFilterItemProperties() { Text = "test" };
            settings.Library = new IdItemFilterItemProperties() { Text = "test" };
            settings.Feature = new IdItemFilterItemProperties() { Text = "test" };
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

        [Test]
        public void StringFilterItemPropertiesEqualsTest()
        {
            Assert.IsTrue(new StringFilterItemProperties("test").Equals(new SdkModels.StringFilterItemProperties("test")));
            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new SdkModels.StringFilterItemProperties(new List<string> { "test", "test2" })));
            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new SdkModels.StringFilterItemProperties(new List<string> { "test2", "test" })));
            Assert.IsTrue(new StringFilterItemProperties().Equals(new SdkModels.StringFilterItemProperties()));

            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new StringFilterItemProperties(new List<string> { "test", "test2" })));
            Assert.IsTrue(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new StringFilterItemProperties(new List<string> { "test2", "test" })));
            Assert.IsTrue(new StringFilterItemProperties("test").Equals(new StringFilterItemProperties("test")));
            Assert.IsTrue(new StringFilterItemProperties().Equals(new StringFilterItemProperties()));

            Assert.IsFalse(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new SdkModels.StringFilterItemProperties(new List<string> { "test" })));
            Assert.IsFalse(new StringFilterItemProperties("test").Equals(new SdkModels.StringFilterItemProperties("test2")));
            Assert.IsFalse(new StringFilterItemProperties().Equals((SdkModels.StringFilterItemProperties)null));

            Assert.IsFalse(new StringFilterItemProperties(new List<string> { "test", "test2" }).Equals(new StringFilterItemProperties(new List<string> { "test2" })));
            Assert.IsFalse(new StringFilterItemProperties().Equals((StringFilterItemProperties)null));
            Assert.IsFalse(new StringFilterItemProperties("test").Equals(new StringFilterItemProperties("test2")));
        }

        [Test]
        public void StringFilterItemPropertiesSdkModelTest()
        {
            CollectionAssert.AreEqual(new StringFilterItemProperties(new List<string> { "test", "test2" }).ToSdkModel().Values, new List<string> { "test", "test2" });
            CollectionAssert.AreEqual(new StringFilterItemProperties("test").ToSdkModel().Values, new List<string> { "test" });
            Assert.AreEqual(new StringFilterItemProperties().ToSdkModel(), null);

            CollectionAssert.AreEqual(StringFilterItemProperties.FromSdkModel(new SdkModels.StringFilterItemProperties(new List<string> { "test", "test2" })).Values, new List<string> { "test", "test2" });
            CollectionAssert.AreEqual(StringFilterItemProperties.FromSdkModel(new SdkModels.StringFilterItemProperties("test")).Values, new List<string> { "test" });
            Assert.AreEqual(StringFilterItemProperties.FromSdkModel(new SdkModels.StringFilterItemProperties()), null);
        }

        [Test]
        public void EnumFilterItemPropertiesEqualsTest()
        {
            Assert.IsTrue(new EnumFilterItemProperties(1).Equals(new SdkModels.EnumFilterItemProperties(1)));
            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new SdkModels.EnumFilterItemProperties(new List<int> { 1, 2 })));
            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new SdkModels.EnumFilterItemProperties(new List<int> { 2, 1 })));
            Assert.IsTrue(new EnumFilterItemProperties().Equals(new SdkModels.EnumFilterItemProperties()));

            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new EnumFilterItemProperties(new List<int> { 1, 2 })));
            Assert.IsTrue(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new EnumFilterItemProperties(new List<int> { 2, 1 })));
            Assert.IsTrue(new EnumFilterItemProperties(1).Equals(new EnumFilterItemProperties(1)));
            Assert.IsTrue(new EnumFilterItemProperties().Equals(new EnumFilterItemProperties()));

            Assert.IsFalse(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new SdkModels.EnumFilterItemProperties(new List<int> { 1 })));
            Assert.IsFalse(new EnumFilterItemProperties(1).Equals(new SdkModels.EnumFilterItemProperties(2)));
            Assert.IsFalse(new EnumFilterItemProperties().Equals((SdkModels.EnumFilterItemProperties)null));

            Assert.IsFalse(new EnumFilterItemProperties(new List<int> { 1, 2 }).Equals(new EnumFilterItemProperties(new List<int> { 2 })));
            Assert.IsFalse(new EnumFilterItemProperties().Equals((EnumFilterItemProperties)null));
            Assert.IsFalse(new EnumFilterItemProperties(1).Equals(new EnumFilterItemProperties(2)));
        }

        [Test]
        public void EnumFilterItemPropertiesSdkModelTest()
        {
            CollectionAssert.AreEqual(new EnumFilterItemProperties(new List<int> { 1, 2 }).ToSdkModel().Values, new List<int> { 1, 2 });
            CollectionAssert.AreEqual(new EnumFilterItemProperties(1).ToSdkModel().Values, new List<int> { 1 });
            Assert.IsNull(new EnumFilterItemProperties().ToSdkModel());

            CollectionAssert.AreEqual(EnumFilterItemProperties.FromSdkModel(new SdkModels.EnumFilterItemProperties(new List<int> { 1, 2 })).Values, new List<int> { 1, 2 });
            CollectionAssert.AreEqual(EnumFilterItemProperties.FromSdkModel(new SdkModels.EnumFilterItemProperties(1)).Values, new List<int> { 1 });
            Assert.IsNull(EnumFilterItemProperties.FromSdkModel(new SdkModels.EnumFilterItemProperties()));
        }

        [Test]
        public void FilterItemPropertiesEqualsTest()
        {
            var id = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            Assert.IsTrue(new IdItemFilterItemProperties("test").Equals(new IdItemFilterItemProperties("test")));
            Assert.IsTrue(new IdItemFilterItemProperties(id).Equals(new IdItemFilterItemProperties(id)));
            Assert.IsTrue(new IdItemFilterItemProperties(new List<Guid> { id, id2 }).Equals(new IdItemFilterItemProperties(new List<Guid> { id2, id })));
            Assert.IsTrue(new IdItemFilterItemProperties().Equals(new IdItemFilterItemProperties()));

            Assert.IsTrue(new IdItemFilterItemProperties("test").Equals(new SdkModels.IdItemFilterItemProperties("test")));
            Assert.IsTrue(new IdItemFilterItemProperties(id).Equals(new SdkModels.IdItemFilterItemProperties(id)));
            Assert.IsTrue(new IdItemFilterItemProperties(new List<Guid> { id, id2 }).Equals(new SdkModels.IdItemFilterItemProperties(new List<Guid> { id2, id })));
            Assert.IsTrue(new IdItemFilterItemProperties().Equals(new SdkModels.IdItemFilterItemProperties()));

            Assert.IsFalse(new IdItemFilterItemProperties("test").Equals(new IdItemFilterItemProperties("test2")));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new IdItemFilterItemProperties(id2)));
            Assert.IsFalse(new IdItemFilterItemProperties(new List<Guid> { id }).Equals(new IdItemFilterItemProperties(new List<Guid> { id, id2 })));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new IdItemFilterItemProperties()));
            Assert.IsFalse(new IdItemFilterItemProperties().Equals(new IdItemFilterItemProperties(id)));

            Assert.IsFalse(new IdItemFilterItemProperties("test").Equals(new SdkModels.IdItemFilterItemProperties("test2")));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new SdkModels.IdItemFilterItemProperties(id2)));
            Assert.IsFalse(new IdItemFilterItemProperties(new List<Guid> { id }).Equals(new SdkModels.IdItemFilterItemProperties(new List<Guid> { id, id2 })));
            Assert.IsFalse(new IdItemFilterItemProperties(id).Equals(new SdkModels.IdItemFilterItemProperties()));
            Assert.IsFalse(new IdItemFilterItemProperties().Equals(new SdkModels.IdItemFilterItemProperties(id)));
        }

        [Test]
        public void FilterItemPropertiesSdkModelTest()
        {
            var id = Guid.NewGuid();
            Assert.AreEqual(new IdItemFilterItemProperties("test").ToSdkModel().Text, "test");
            CollectionAssert.AreEqual(new IdItemFilterItemProperties(id).ToSdkModel().Ids, new List<Guid> { id });
            CollectionAssert.AreEqual(new IdItemFilterItemProperties(new List<Guid> { id }).ToSdkModel().Ids, new List<Guid> { id });
            Assert.IsNull(new IdItemFilterItemProperties().ToSdkModel());

            Assert.AreEqual(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties("test")).Text, "test");
            Assert.AreEqual(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties(id)).Ids, new List<Guid> { id });
            Assert.AreEqual(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties(new List<Guid> { id })).Ids, new List<Guid> { id });
            Assert.IsNull(IdItemFilterItemProperties.FromSdkModel(new SdkModels.IdItemFilterItemProperties()));
        }
    }
}
