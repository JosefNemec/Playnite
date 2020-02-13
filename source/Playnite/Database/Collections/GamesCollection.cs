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

        public GamesCollection(GameDatabase database) : base((Game game) =>
        {
            game.IsInstalling = false;
            game.IsUninstalling = false;
            game.IsLaunching = false;
            game.IsRunning = false;
        })
        {
            db = database;
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
            item.Added = DateTime.Today;
            base.Add(item);
        }

        public override void Add(IEnumerable<Game> items)
        {
            foreach (var item in items)
            {
                item.Added = DateTime.Today;
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
