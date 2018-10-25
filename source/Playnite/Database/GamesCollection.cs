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
            game.State.Installing = false;
            game.State.Uninstalling = false;
            game.State.Launching = false;
            game.State.Running = false;
        })
        {
            db = database;
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
            db.RemoveFile(item.BackgroundImage);
            return result;
        }

        public override bool Remove(Game item)
        {
            return Remove(item.Id);
        }

        public override bool Remove(IEnumerable<Game> items)
        {
            var result = base.Remove(items);
            foreach (var item in items)
            {
                db.RemoveFile(item.Icon);
                db.RemoveFile(item.CoverImage);
                db.RemoveFile(item.BackgroundImage);                
            }

            return result;
        }
    }
}
