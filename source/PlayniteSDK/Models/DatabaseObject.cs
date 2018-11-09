using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class DatabaseObject : ObservableObject
    {
        public Guid Id { get; set; }

        public DatabaseObject()
        {
            Id = Guid.NewGuid();
        }
    }
}
