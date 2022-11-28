using NUnit.Framework;
using NUnit.Framework.Internal;
using Playnite.Common;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Models
{
    [TestFixture]
    public class CopyDiffToTests
    {
        public List<T> GenerateList<T>(int size, Random random, bool boolState, Func<T> generator = null)
        {
            var items = new List<T>(size);
            Enumerable.Range(1, random.Next(1, size)).ForEach(a => items.Add(generator == null ? GenerateObject<T>(random, boolState) : generator()));
            return items;
        }

        public ObservableCollection<T> GenerateObservableList<T>(int size, Random random, bool boolState, Func<T> generator = null)
        {
            return GenerateList<T>(size, random, boolState, generator).ToObservable();
        }

        public T GenerateObject<T>(Random random, bool boolState)
        {
            var obj = typeof(T).CrateInstance<T>();
            foreach (var prop in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(prop => prop.GetCustomAttribute(typeof(Playnite.SDK.Data.DontSerializeAttribute)) == null))
            {
                if (prop.PropertyType == typeof(string))
                {
                    prop.SetValue(obj, PlayniteTests.GetRandomString(random.Next(10, 100)));
                }
                else if (prop.PropertyType == typeof(List<string>))
                {
                    prop.SetValue(obj, GenerateList<string>(5, random, boolState, () => PlayniteTests.GetRandomString(random.Next(10, 100))));
                }
                else if (prop.PropertyType == typeof(Guid))
                {
                    prop.SetValue(obj, Guid.NewGuid());
                }
                else if (prop.PropertyType == typeof(List<Guid>))
                {
                    prop.SetValue(obj, GenerateList<Guid>(5, random, boolState, () => Guid.NewGuid()));
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    prop.SetValue(obj, boolState);
                }
                else if (prop.PropertyType == typeof(DateTime?))
                {
                    prop.SetValue(obj, new DateTime(random.Next(1980, 2020), random.Next(1, 12), random.Next(1, 28)));
                }
                else if (prop.PropertyType == typeof(ReleaseDate?))
                {
                    prop.SetValue(obj, new ReleaseDate(random.Next(1980, 2020), random.Next(1, 12), random.Next(1, 28)));
                }
                else if (prop.PropertyType == typeof(ulong?) || prop.PropertyType == typeof(ulong))
                {
                    prop.SetValue(obj, (ulong)random.Next(0, int.MaxValue));
                }
                else if (prop.PropertyType == typeof(int?) || prop.PropertyType == typeof(int))
                {
                    prop.SetValue(obj, random.Next(0, 100));
                }
                else if (prop.PropertyType == typeof(List<int>))
                {
                    prop.SetValue(obj, GenerateList<int>(5, random, boolState, () => random.Next(0, 100)));
                }
                else if (prop.PropertyType.IsEnum)
                {
                    var values = Enum.GetValues(prop.PropertyType);
                    prop.SetValue(obj, values.GetValue(random.Next(values.Length)));
                }
                else if (Nullable.GetUnderlyingType(prop.PropertyType)?.IsEnum == true)
                {
                    var values = Enum.GetValues(Nullable.GetUnderlyingType(prop.PropertyType));
                    prop.SetValue(obj, values.GetValue(random.Next(values.Length)));
                }
                else if (prop.PropertyType.FullName.StartsWith("Playnite.SDK.Models"))
                {
                    var genMethod = typeof(CopyDiffToTests).GetMethod(nameof(GenerateObject)).MakeGenericMethod(prop.PropertyType);
                    prop.SetValue(obj, genMethod.Invoke(this, new object[] { random, boolState }));
                }
                else if (prop.PropertyType.FullName.Contains("ObservableCollection`1[[Playnite.SDK.Models"))
                {
                    var baseType = prop.PropertyType.BaseType.GetGenericArguments()[0];
                    var genMethod = typeof(CopyDiffToTests).GetMethod(nameof(GenerateObservableList)).MakeGenericMethod(baseType);
                    prop.SetValue(obj, genMethod.Invoke(this, new object[] { 5, random, boolState, null }));
                }
                else
                {
                    throw new Exception($"Uknown member type {prop.PropertyType}");
                }
            }

            return obj;
        }

        [Test]
        public void GameCopyDiffToTest()
        {
            var random = new Random();
            var generated = GenerateObject<Game>(random, false);
            var generated2 = GenerateObject<Game>(random, true);
            generated2.Id = generated.Id;
            var empty = new Game() { Id = generated.Id };
            generated.CopyDiffTo(empty);
            generated.CopyDiffTo(generated2);

            Assert.AreEqual(0, generated.GetDifferences(empty).Count);
            Assert.AreEqual(0, generated.GetDifferences(generated2).Count);
            Assert.AreEqual(Serialization.ToJson(generated), Serialization.ToJson(empty));
            Assert.AreEqual(Serialization.ToJson(generated), Serialization.ToJson(generated2));

            generated2.Name = "test";
            generated2.Description = "test2";
            var changes = 0;
            generated2.PropertyChanged += (_, __) => changes++;
            generated.CopyDiffTo(generated2);
            Assert.AreEqual(2, changes);
        }

        public void CopyDiffToTest<T>() where T : DatabaseObject
        {
            var random = new Random();
            var generated = GenerateObject<T>(random, false);
            var generated2 = GenerateObject<T>(random, true);
            generated2.Id = generated.Id;
            var empty = typeof(T).CrateInstance<T>();
            empty.Id = generated.Id;
            generated.CopyDiffTo(empty);
            generated.CopyDiffTo(generated2);

            Assert.AreEqual(Serialization.ToJson(generated), Serialization.ToJson(empty));
            Assert.AreEqual(Serialization.ToJson(generated), Serialization.ToJson(generated2));
        }

        [Test]
        public void CopyDiffToTest()
        {
            CopyDiffToTest<AgeRating>();
            CopyDiffToTest<Category>();
            CopyDiffToTest<Company>();
            CopyDiffToTest<CompletionStatus>();
            CopyDiffToTest<Emulator>();
            CopyDiffToTest<GameFeature>();
            CopyDiffToTest<FilterPreset>();
            CopyDiffToTest<Genre>();
            CopyDiffToTest<ImportExclusionItem>();
            CopyDiffToTest<Platform>();
            CopyDiffToTest<Region>();
            CopyDiffToTest<GameScannerConfig>();
            CopyDiffToTest<Series>();
            CopyDiffToTest<GameSource>();
            CopyDiffToTest<Tag>();
            CopyDiffToTest<AppSoftware>();
        }

        [Test]
        public void GetCopyTest()
        {
            var random = new Random();

            var game = GenerateObject<Game>(random, true);
            Assert.AreEqual(Serialization.ToJson(game), Serialization.ToJson(game.GetCopy()));

            var gameAction = GenerateObject<GameAction>(random, true);
            Assert.AreEqual(Serialization.ToJson(gameAction), Serialization.ToJson(gameAction.GetCopy()));

            var cusProfile = GenerateObject<CustomEmulatorProfile>(random, true);
            Assert.AreEqual(Serialization.ToJson(cusProfile), Serialization.ToJson(cusProfile.GetCopy()));

            var buiProfile = GenerateObject<BuiltInEmulatorProfile>(random, true);
            Assert.AreEqual(Serialization.ToJson(buiProfile), Serialization.ToJson(buiProfile.GetCopy()));

            var emulator = GenerateObject<Emulator>(random, true);
            Assert.AreEqual(Serialization.ToJson(emulator), Serialization.ToJson(emulator.GetCopy()));
        }
    }
}
