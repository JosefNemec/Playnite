using System.Collections.Generic;
using Playnite.SDK;
using Playnite.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class ImageCroppingViewModel : ObservableObject
    {
        public string WindowTitle { get; set; }

        private ImageFileOption image;
        public ImageFileOption Image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged();
            }
        }

        public ImageFileOption CroppedImage { get; set; }

        public RelayCommand<object> CloseCommand =>
            new RelayCommand<object>((a) =>
            {
                CloseView(false);
            });

        public RelayCommand<object> ConfirmCommand =>
            new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });

        private static readonly ILogger Logger = LogManager.GetLogger();
        private readonly IWindowFactory window;

        public ImageCroppingViewModel(ImageFileOption image, IWindowFactory window)
        {
            this.image = image;
            this.window = window;

            WindowTitle = ResourceProvider.GetString("LOCCropImageTitle");
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
            //TODO: do cropping
            CloseView(true);
        }
    }
}
