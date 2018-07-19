using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class DatabaseSettings
    {
        [BsonId]
        public int Id
        {
            get; set;
        } = 1;

        /// <summary>
        /// Indicated if game states for custom games has been fixed (during update from 3.x to 4.x)
        /// </summary>
        public bool InstStatesFixed
        {
            get; set;
        }

        /// <summary>
        /// Indicates if games Source field has been set to default values (for example for Steam game to "Steam").
        /// For update from 3.x to 4.x versions.
        /// </summary>
        public bool GameSourcesUpdated
        {
            get; set;
        }

        public DatabaseSettings()
        {
        }
    }
}
