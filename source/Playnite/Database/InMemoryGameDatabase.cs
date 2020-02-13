using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class InMemoryItemCollection<TItem> : ItemCollection<TItem> where TItem : DatabaseObject
    {
        public InMemoryItemCollection() : base(false)
        {
        }
    }

    public class InMemoryGameDatabase : IGameDatabase
    {
        public IItemCollection<Game> Games { get; } = new InMemoryItemCollection<Game>();
        public IItemCollection<Platform> Platforms { get; } = new InMemoryItemCollection<Platform>();
        public IItemCollection<Emulator> Emulators { get; } = new InMemoryItemCollection<Emulator>();
        public IItemCollection<Genre> Genres { get; } = new InMemoryItemCollection<Genre>();
        public IItemCollection<Company> Companies { get; } = new InMemoryItemCollection<Company>();
        public IItemCollection<Tag> Tags { get; } = new InMemoryItemCollection<Tag>();
        public IItemCollection<Category> Categories { get; } = new InMemoryItemCollection<Category>();
        public IItemCollection<Series> Series { get; } = new InMemoryItemCollection<Series>();
        public IItemCollection<AgeRating> AgeRatings { get; } = new InMemoryItemCollection<AgeRating>();
        public IItemCollection<Region> Regions { get; } = new InMemoryItemCollection<Region>();
        public IItemCollection<GameSource> Sources { get; } = new InMemoryItemCollection<GameSource>();
        public IItemCollection<GameFeature> Features { get; } = new InMemoryItemCollection<GameFeature>();
        public bool IsOpen => true;

#pragma warning disable CS0067
        public event EventHandler DatabaseOpened;
#pragma warning restore CS0067

        public InMemoryGameDatabase()
        {
        }

        public Game ImportGame(GameInfo game)
        {
            throw new NotImplementedException();
        }

        public Game ImportGame(GameInfo game, LibraryPlugin sourcePlugin)
        {
            throw new NotImplementedException();
        }
    }
}
