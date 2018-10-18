using Playnite.API;
using Playnite.Plugins;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class Library : FilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Library(Guid id, string name, bool selected)
        {
            Id = id;
            Name = name;
            Selected = selected;
        }
    }

    public class FilterItem : ObservableObject
    {
        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                OnAutoPropertyChanged();
            }
        }
    }

    public class DatabaseFilter : ObservableObject
    {
        private GameDatabase database;
        private ExtensionFactory extensions;
        private FilterSettings filter;

        // TODO: get rid of ignore* variables and introduce better binding between DatabaseFilter and FilterSettings (or merge them)
        private bool ignoreFilterChanges = false;
        private bool ignoreLibChanges = false;

        public List<Library> Libraries { get; set; }
        public string LibrariesString
        {
            get => string.Join(", ", Libraries.Where(a => a.Selected)?.Select(a => a.Name)?.ToArray());
        }

        public DatabaseFilter(GameDatabase database, ExtensionFactory extensions, FilterSettings filter)
        {
            this.database = database;
            this.extensions = extensions;
            this.filter = filter;

            Libraries = this.extensions.LibraryPlugins.Select(a => new Library(a.Value.Plugin.Id, a.Value.Plugin.Name, filter.Libraries?.Contains(a.Value.Plugin.Id) == true)).ToList();
            Libraries.Add(new Library(Guid.Empty, "Custom", filter.Libraries?.Contains(Guid.Empty) == true));
            Libraries.ForEach(a => a.PropertyChanged += LibrarySelection_PropertyChanged);
            filter.FilterChanged += Filter_FilterChanged;

            // Remove filters for unloaded plugins
            var missing = filter.Libraries?.Where(a => Libraries.FirstOrDefault(b => b.Id == a) == null)?.ToList();
            if (missing?.Any() == true)
            {
                if (filter.Libraries != null)
                {
                    missing.ForEach(a => filter.Libraries.Remove(a));                   
                }
            }
        }

        private void Filter_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (ignoreFilterChanges)
            {
                return;
            }

            if (e.Fields?.Contains("Libraries") == true)
            {
                UpdateFilterStates();
            }
        }

        private void LibrarySelection_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ignoreLibChanges)
            {
                return;
            }

            if (e.PropertyName == "Selected")
            {
                ignoreFilterChanges = true;
                var selected = Libraries.Where(a => a.Selected);
                if (selected.Any())
                {
                    filter.Libraries = selected.Select(a => a.Id).ToList();
                }
                else
                {
                    filter.Libraries = null;
                }

                ignoreFilterChanges = false;
            }

            OnPropertyChanged("LibrariesString");
        }

        private void UpdateFilterStates()
        {
            ignoreLibChanges = true;
            foreach (var lib in Libraries)
            {
                if (filter.Libraries?.Contains(lib.Id) == true)
                {
                    lib.Selected = true;
                }
                else
                {
                    lib.Selected = false;
                }
            }

            ignoreLibChanges = false;
            OnPropertyChanged("LibrariesString");
        }
    }
}
