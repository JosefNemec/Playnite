using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public class SingleItemSelectionViewModel<TItem> : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private SelectableNamedObject<TItem> selectedItem;

        public List<SelectableNamedObject<TItem>> Items { get; set; }
        public int StartIndex { get; private set; }
        public string HeaderText { get; set; }
        public string MessageText { get; set; }

        public RelayCommand<SelectableNamedObject<TItem>> SelectItemCommand { get; }
        public RelayCommand CancelCommand => new RelayCommand(() => window.Close(null));

        public SingleItemSelectionViewModel(IWindowFactory window, string header, string message)
        {
            this.window = window;
            HeaderText = header?.GetLocalized();
            MessageText = message?.GetLocalized();
            SelectItemCommand = new RelayCommand<SelectableNamedObject<TItem>>((item) =>
            {
                selectedItem = item;
                window.Close(true);
            });
        }

        public bool SelectItem(List<SelectableNamedObject<TItem>> items, out TItem selected)
        {
            if (!items.HasItems())
            {
                selected = default;
                return false;
            }

            Items = items;
            // Selected property is actually not used for selection it's setting focus.
            // So some item has to be selected (and have focus) otherwise the view would not be controllable.
            if (Items.All(a => a.Selected == false))
            {
                Items[0].Selected = true;
            }

            // This is for the virtualized view to know where to scroll on view load.
            StartIndex = Items.IndexOf(Items.First(a => a.Selected));

            if (window.CreateAndOpenDialog(this) == true)
            {
                selected = selectedItem.Value;
                return true;
            }
            else
            {
                selected = default;
                return false;
            }
        }
    }

    public class MultiItemSelectionViewModel<TItem> : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;

        public List<SelectableNamedObject<TItem>> Items { get; set; }
        public string HeaderText { get; set; }
        public string MessageText { get; set; }

        public RelayCommand CancelCommand => new RelayCommand(() => window.Close(null));
        public RelayCommand ConfirmCommand => new RelayCommand(() => window.Close(true));
        public RelayCommand ToggleSelectionCommand => new RelayCommand(() => ToggleSelection());

        public MultiItemSelectionViewModel(IWindowFactory window, string header, string message)
        {
            this.window = window;
            HeaderText = header?.GetLocalized();
            MessageText = message?.GetLocalized();
        }

        public bool SelectItem(List<SelectableNamedObject<TItem>> items, out List<TItem> selected)
        {
            Items = items;
            if (window.CreateAndOpenDialog(this) == true)
            {
                selected = Items.Where(a => a.Selected).Select(a => a.Value).ToList();
                return true;
            }
            else
            {
                selected = null;
                return false;
            }
        }

        private void ToggleSelection()
        {
            if (!Items.HasItems())
            {
                return;
            }

            var toggle = !Items[0].Selected;
            Items.ForEach(a => a.Selected = toggle);
        }
    }
}
