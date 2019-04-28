using Playnite.Plugins;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database
{
    public class ExplorableField
    {
        public GroupableField Field { get; }

        public ExplorableField(GroupableField field)
        {
            Field = field;
        }

        public override string ToString()
        {
            return ResourceProvider.GetString(Field.GetDescription());
        }
    }

    public class DatabaseExplorer : ObservableObject
    {

        private ExplorableField selectedField;
        public ExplorableField SelectedField
        {
            get => selectedField;
            set
            {
                selectedField = value;
                OnPropertyChanged();
            }
        }

        public List<ExplorableField> Fields { get; set; }

        private object fieldValues;
        public object FieldValues
        {
            get => fieldValues;
            set
            {
                fieldValues = value;
                OnPropertyChanged();
            }
        }

        public DatabaseExplorer(GameDatabase database, ExtensionFactory extensions)
        {
            Fields = new List<ExplorableField>();
            foreach (GroupableField val in Enum.GetValues(typeof(GroupableField)))
            {
                if (val != GroupableField.None)
                {
                    Fields.Add(new ExplorableField(val));
                }                
            }

            SelectedField = Fields.FirstOrDefault(a => a.Field == GroupableField.Library);
        }
    }
}
