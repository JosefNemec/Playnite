using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface IGameDatabase
    {
        IItemCollection<Game> Games { get; }
        IItemCollection<Platform> Platforms { get; }
        IItemCollection<Emulator> Emulators { get; }
        IItemCollection<Genre> Genres { get; }
        IItemCollection<Company> Companies { get; }
        IItemCollection<Tag> Tags { get; }
        IItemCollection<Category> Categories { get; }
        IItemCollection<Series> Series { get; }
        IItemCollection<AgeRating> AgeRatings { get; }
        IItemCollection<Region> Regions { get; }
        IItemCollection<GameSource> Sources { get; }
    }
}
