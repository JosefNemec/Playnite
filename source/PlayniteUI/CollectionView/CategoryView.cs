using System;
using Playnite.SDK;

namespace PlayniteUI
{
    public class CategoryView : IComparable
    {
        public string Category
        {
            get; set;
        }

        public CategoryView(string category)
        {
            Category = category;
        }

        public int CompareTo(object obj)
        {
            var cat = (obj as CategoryView).Category;

            if (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(cat))
            {
                return 0;
            }
            if (string.IsNullOrEmpty(Category))
            {
                return 1;
            }
            if (string.IsNullOrEmpty(cat))
            {
                return -1;
            }
            if (Category.Equals(cat))
            {
                return 0;
            }

            return string.Compare(Category, cat, true);
        }

        public override bool Equals(object obj)
        {
            var cat = ((CategoryView)obj).Category;

            if (string.IsNullOrEmpty(Category) && string.IsNullOrEmpty(cat))
            {
                return true;
            }
            if (string.IsNullOrEmpty(Category))
            {
                return false;
            }
            if (string.IsNullOrEmpty(cat))
            {
                return false;
            }
            if (Category.Equals(cat))
            {
                return true;
            }

            return string.Compare(Category, cat, true) == 0;
        }

        public override int GetHashCode()
        {
            if (Category == null)
            {
                return 0;
            }
            else
            {
                return Category.GetHashCode();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Category) ? DefaultResourceProvider.FindString("LOCNoCategory") : Category;
        }
    }
}
