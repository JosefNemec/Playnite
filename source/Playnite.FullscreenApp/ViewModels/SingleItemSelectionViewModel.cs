using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.FullscreenApp.ViewModels
{
    public class SingleItemSelectionViewModel<TItem> : ObservableObject where TItem : class
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;

        public List<TItem> Items { get; set; }
        public TItem SelectedItem { get; private set; }
        public string HeaderText { get; set; }

        public RelayCommand<TItem> SelectItemCommand { get; }
        public RelayCommand CancelCommand => new RelayCommand(() => window.Close(null));

        public SingleItemSelectionViewModel(IWindowFactory window, string header)
        {
            this.window = window;
            HeaderText = header;
            SelectItemCommand = new RelayCommand<TItem>((item) =>
            {
                SelectedItem = item;
                window.Close(true);
            });
        }

        public TItem SelectItem(List<TItem> items)
        {
            Items = items;
            if (window.CreateAndOpenDialog(this) == true)
            {
                return SelectedItem;
            }
            else
            {
                return null;
            }
        }
    }
}
