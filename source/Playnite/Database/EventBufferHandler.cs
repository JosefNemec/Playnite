using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class EventBufferHandler : IDisposable
    {
        private GameDatabase database;

        public EventBufferHandler(GameDatabase db)
        {
            database = db;
            db.BeginBufferUpdate();
        }

        public void Dispose()
        {
            database.EndBufferUpdate();
        }
    }
}
