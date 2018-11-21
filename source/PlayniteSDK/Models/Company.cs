using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class Company : DatabaseObject
    {
        public Company() : base()
        {
        }

        public Company(string name) : base()
        {
            Name = name;
        }
    }
}
