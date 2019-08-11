﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents base database object item.
    /// </summary>
    public class DatabaseObject : ObservableObject, IComparable, IIdentifiable
    {
        /// <summary>
        /// Gets or sets identifier of database object.
        /// </summary>
        public Guid Id { get; set; }

        private string name;
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="DatabaseObject"/>.
        /// </summary>
        public DatabaseObject()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Compares Names of database object.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            var objName = (obj as DatabaseObject).Name;
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(objName))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(Name))
            {
                return 1;
            }

            if (string.IsNullOrEmpty(objName))
            {
                return -1;
            }

            return string.Compare(Name, objName, true);
        }

        /// <summary>
        /// DO NOT use for actual equality check, this only checks if db Ids are equal!
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DatabaseObject dbObj)
            {
                return dbObj.Id == Id;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            if (Id == Guid.Empty)
            {
                return 0;
            }
            else
            {
                return Id.GetHashCode();
            }
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name ?? string.Empty;
        }
    }
}
