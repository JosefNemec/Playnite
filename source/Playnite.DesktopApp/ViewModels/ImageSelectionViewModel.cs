using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class ImageSelectionViewModel : ObservableObject
    {
        public string WindowTitle { get; set; }

        private List<ImageFileOption> images = new List<ImageFileOption>();
        public List<ImageFileOption> Images
        {
            get
            {
                return images;
            }

            set
            {
                images = value;
                OnPropertyChanged();
            }
        }

        private ImageFileOption selectedImage;
        public ImageFileOption SelectedImage
        {
            get => selectedImage;
            set
            {
                selectedImage = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            }, (a) => SelectedImage != null);
        }
        
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;

        public ImageSelectionViewModel(
            List<ImageFileOption> images,
            IWindowFactory window,
            string caption = null)
        {
            Images = images;
            this.window = window;
            if (caption.IsNullOrEmpty())
            {
                WindowTitle = ResourceProvider.GetString("LOCSelectImageTitle");
            }
            else
            {
                WindowTitle = caption;
            }
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        public void ConfirmDialog()
        {
            CloseView(true);
        }
    }
}
