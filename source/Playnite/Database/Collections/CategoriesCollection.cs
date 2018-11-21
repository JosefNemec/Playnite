using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class CategoriesCollection : ItemCollection<Category>
    {
        private readonly GameDatabase db;

        public CategoriesCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Category Add(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName)) throw new ArgumentNullException(nameof(categoryName));
            var category = this.FirstOrDefault(a => a.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            if (category != null)
            {
                return category;
            }
            else
            {
                var newCategory = new Category(categoryName);
                base.Add(newCategory);
                return newCategory;
            }
        }

        public IEnumerable<Category> Add(List<string> categories)
        {
            var toAdd = new List<Category>();
            foreach (var category in categories)
            {
                var cat = this.FirstOrDefault(a => a.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
                if (cat != null)
                {
                    yield return cat;
                }
                else
                {
                    var newCategory = new Category(category);
                    toAdd.Add(newCategory);
                    yield return newCategory;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }

        // TODO: remove categories from games when removing from collection
    }
}
