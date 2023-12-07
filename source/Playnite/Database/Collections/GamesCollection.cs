using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class GamesCollection : ItemCollection<Game>
    {
        private readonly GameDatabase db;

        public GamesCollection(GameDatabase database, LiteDB.BsonMapper mapper) : base((Game game) =>
        {
            game.IsInstalling = false;
            game.IsUninstalling = false;
            game.IsLaunching = false;
            game.IsRunning = false;
        }, mapper, type: GameDatabaseCollection.Games)
        {
            db = database;
        }

        public static void MapLiteDbEntities(LiteDB.BsonMapper mapper)
        {
            mapper.RegisterType<ReleaseDate>
            (
                (date) => date.Serialize(),
                (bson) => ReleaseDate.Deserialize(bson.AsString)
            );

            mapper.Entity<Game>().
                Id(a => a.Id, false).
                Ignore(a => a.Genres).
                Ignore(a => a.Developers).
                Ignore(a => a.Publishers).
                Ignore(a => a.Tags).
                Ignore(a => a.Features).
                Ignore(a => a.Categories).
                Ignore(a => a.Platforms).
                Ignore(a => a.Series).
                Ignore(a => a.AgeRatings).
                Ignore(a => a.Regions).
                Ignore(a => a.Source).
                Ignore(a => a.ReleaseYear).
                Ignore(a => a.UserScoreRating).
                Ignore(a => a.CommunityScoreRating).
                Ignore(a => a.CriticScoreRating).
                Ignore(a => a.UserScoreGroup).
                Ignore(a => a.CommunityScoreGroup).
                Ignore(a => a.CriticScoreGroup).
                Ignore(a => a.LastActivitySegment).
                Ignore(a => a.AddedSegment).
                Ignore(a => a.ModifiedSegment).
                Ignore(a => a.PlaytimeCategory).
                Ignore(a => a.IsCustomGame).
                Ignore(a => a.InstallationStatus);
        }

        public override Game Add(string itemName)
        {
            throw new NotSupportedException();
        }

        public override Game Add(string itemName, Func<Game, string, bool> existingComparer)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<Game> Add(List<string> items)
        {
            throw new NotSupportedException();
        }

        public override IEnumerable<Game> Add(List<string> itemsToAdd, Func<Game, string, bool> existingComparer)
        {
            throw new NotSupportedException();
        }

        public override void Add(Game item)
        {
            item.Added = DateTime.Now;
            item.Modified = item.Added;
            base.Add(item);
        }

        public override void Add(IEnumerable<Game> items)
        {
            foreach (var item in items)
            {
                item.Added = DateTime.Now;
                item.Modified = item.Added;
            }

            base.Add(items);
        }

        public override bool Remove(Guid id)
        {
            var item = Get(id);
            var result = base.Remove(id);
            db.RemoveFile(item.Icon);
            db.RemoveFile(item.CoverImage);

            if (item.BackgroundImage.IsHttpUrl())
            {
                HttpFileCache.ClearCache(item.BackgroundImage);
            }
            else
            {
                db.RemoveFile(item.BackgroundImage);
            }

            return result;
        }

        public override bool Remove(Game item)
        {
            return Remove(item.Id);
        }

        public override bool Remove(IEnumerable<Game> items)
        {
            foreach (var item in items)
            {
                // Get item from in case that passed platform doesn't have actual metadata.
                var dbItem = Get(item.Id);
                db.RemoveFile(dbItem.Icon);
                db.RemoveFile(dbItem.CoverImage);

                if (dbItem.BackgroundImage.IsHttpUrl())
                {
                    HttpFileCache.ClearCache(dbItem.BackgroundImage);
                }
                else
                {
                    db.RemoveFile(dbItem.BackgroundImage);
                }
            }

            var result = base.Remove(items);
            return result;
        }

        public override void Update(Game itemToUpdate)
        {
            var dbItem = Get(itemToUpdate.Id);
            if (!dbItem.Icon.IsNullOrEmpty() && dbItem.Icon != itemToUpdate.Icon)
            {
                db.RemoveFile(dbItem.Icon);
            }

            if (!dbItem.CoverImage.IsNullOrEmpty() && dbItem.CoverImage != itemToUpdate.CoverImage)
            {
                db.RemoveFile(dbItem.CoverImage);
            }

            if (!dbItem.BackgroundImage.IsNullOrEmpty() && !dbItem.BackgroundImage.IsHttpUrl() && dbItem.BackgroundImage != itemToUpdate.BackgroundImage)
            {
                db.RemoveFile(dbItem.BackgroundImage);
            }
            else if (dbItem.BackgroundImage.IsHttpUrl() && dbItem.BackgroundImage != itemToUpdate.BackgroundImage)
            {
                HttpFileCache.ClearCache(dbItem.BackgroundImage);
            }

            base.Update(itemToUpdate);
        }

        public override void Update(IEnumerable<Game> itemsToUpdate)
        {
            foreach (var item in itemsToUpdate)
            {
                var dbItem = Get(item.Id);
                if (!dbItem.Icon.IsNullOrEmpty() && dbItem.Icon != item.Icon)
                {
                    db.RemoveFile(dbItem.Icon);
                }

                if (!dbItem.CoverImage.IsNullOrEmpty() && dbItem.CoverImage != item.CoverImage)
                {
                    db.RemoveFile(dbItem.CoverImage);
                }

                if (!dbItem.BackgroundImage.IsNullOrEmpty())
                {
                    if (!dbItem.BackgroundImage.IsHttpUrl() && dbItem.BackgroundImage != item.BackgroundImage)
                    {
                        db.RemoveFile(dbItem.BackgroundImage);
                    }
                    else if (dbItem.BackgroundImage.IsHttpUrl() && dbItem.BackgroundImage != item.BackgroundImage)
                    {
                        HttpFileCache.ClearCache(dbItem.BackgroundImage);
                    }
                }
            }

            base.Update(itemsToUpdate);
        }
    }
}
