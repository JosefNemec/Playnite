using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models
{
    /// <summary>
    /// Represents a user for which we have profile information, ie. that we can connect as, etc.
    /// </summary>
    public class Profile
    {
        /// <summary>
        /// itch.io user ID, doubling as profile ID
        /// </summary>
        public long id;

        /// <summary>
        /// Timestamp the user last connected at(to the client)
        /// </summary>
        public DateTime? lastConnected;

        //User information
        public User user;
    }

    public class ProfileList
    {
        public List<Profile> profiles;
    }
}
