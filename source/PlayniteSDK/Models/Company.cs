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

        public static readonly Company Empty = new Company { Id = Guid.Empty, Name = string.Empty };
    }

    public class Developer : Company
    {
        public Developer() : base()
        {
        }

        public Developer(string name) : base()
        {

        }
    }


    public class Publisher : Company
    {
        public Publisher() : base()
        {
        }

        public Publisher(string name) : base()
        {

        }
    }
}
