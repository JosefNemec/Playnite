using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using Playnite.Windows;

namespace Playnite.DesktopApp.ViewModels
{
    public class ImageCroppingViewModel : ObservableObject
    {
        public readonly string WindowTitle;

        public string ImageSource { get; set; }

        public BitmapSource CroppedResultImage { get; set; }

        public MetadataFile CroppedImage { get; private set; }

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

        public ImageCroppingViewModel(string url, IWindowFactory window)
        {
            ImageSource = url;
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
            CroppedImage = new MetadataFile(ImageSource);
            var croppedBitmap = (CroppedBitmap)CroppedResultImage;

            byte[] bytes;
            var frame = BitmapFrame.Create(croppedBitmap);
            //TODO: check if we might have to swap encoders
            var encoder = new PngBitmapEncoder();
            //var encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(frame);
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                bytes = new byte[(int)stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(bytes, 0, (int) stream.Length);
            }

            CroppedImage.FileName = $"cropped_{CroppedImage.FileName}";
            CroppedImage.Content = bytes;
            CloseView(true);
        }
    }
}
