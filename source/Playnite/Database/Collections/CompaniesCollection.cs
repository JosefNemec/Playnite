using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class CompaniesCollection : ItemCollection<Company>
    {
        private readonly GameDatabase db;

        public CompaniesCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string companyName)
        {
            if (string.IsNullOrEmpty(companyName)) throw new ArgumentNullException(nameof(companyName));
            var comp = this.FirstOrDefault(a => a.Name.Equals(companyName, StringComparison.OrdinalIgnoreCase));
            if (comp != null)
            {
                return comp.Id;
            }
            else
            {
                var newComp = new Company(companyName);
                base.Add(newComp);
                return newComp.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> companies)
        {
            var toAdd = new List<Company>();
            foreach (var company in companies)
            {
                var comp = this.FirstOrDefault(a => a.Name.Equals(company, StringComparison.OrdinalIgnoreCase));
                if (comp != null)
                {
                    yield return comp.Id;
                }
                else
                {
                    var newComp = new Company(company);
                    toAdd.Add(newComp);
                    yield return newComp.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
