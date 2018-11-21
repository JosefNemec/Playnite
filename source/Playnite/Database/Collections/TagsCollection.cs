using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class TagsCollection : ItemCollection<Tag>
    {
        private readonly GameDatabase db;

        public TagsCollection(GameDatabase database) : base()
        {
            db = database;
        }

        public Guid Add(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) throw new ArgumentNullException(nameof(tagName));
            var tag = this.FirstOrDefault(a => a.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase));
            if (tag != null)
            {
                return tag.Id;
            }
            else
            {
                var newTag = new Tag(tagName);
                base.Add(newTag);
                return newTag.Id;
            }
        }

        public IEnumerable<Guid> Add(List<string> tags)
        {
            var toAdd = new List<Tag>();
            foreach (var tag in tags)
            {
                var exTag = this.FirstOrDefault(a => a.Name.Equals(tag, StringComparison.OrdinalIgnoreCase));
                if (exTag != null)
                {
                    yield return exTag.Id;
                }
                else
                {
                    var newTag = new Tag(tag);
                    toAdd.Add(newTag);
                    yield return newTag.Id;
                }
            }

            if (toAdd?.Any() == true)
            {
                base.Add(toAdd);
            }
        }
    }
}
