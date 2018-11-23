using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.View;
using ImageMagick;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageSandbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        private double dpiX;
        private double dpiY;
        private WriteableBitmap modifiedImage;
        private WriteableBitmap orignalImage;
        private int pixelArea;
        private MosaicInformationDialog displayMosaicInformation;
        private StorageFile selectedImageFile;

        #endregion

        #region Constructors

        public MainPage()
        {
            
            this.displayMosaicInformation = new MosaicInformationDialog();
            this.InitializeComponent();
            this.PixelAreaOf5.IsChecked = true;
            this.ModifyMoasicButton.IsEnabled = false;
            this.RefreshMosaicButton.IsEnabled = false;
            this.dpiX = 0;
            this.dpiY = 0;
        }

        #endregion

       
        private async void saveWritableBitmap()
        {
            var fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> { ".png" });
            var savefile = await fileSavePicker.PickSaveFileAsync();

            if (savefile != null)
            {
                var stream = await savefile.OpenAsync(FileAccessMode.ReadWrite);
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream);

                var pixelStream = this.modifiedImage.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint)this.modifiedImage.PixelWidth,
                    (uint)this.modifiedImage.PixelHeight, this.dpiX, this.dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }




        private void createPictureMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var y = 0;
            while (y <= imageHeight)
            {
                var x = 0;
                while (x <= imageWidth)
                {
                    var XStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    var YStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);

                    var averageColor =
                        this.FindAverageColor(sourcePixels, imageWidth, imageHeight, y,
                            YStoppingPoint, x, XStoppingPoint);


                    x += this.pixelArea;
                }
                y += this.pixelArea;
            }
        }


        private void createSolidMosaic(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var y = 0;
            while (y < imageHeight)
            {
                var x = 0;
                while (x < imageWidth)
                {
                    var XStoppingPoint = this.UpdateStoppingPoint(imageWidth, x);

                    var YStoppingPoint = this.UpdateStoppingPoint(imageHeight, y);

                   


                    var averageColor = 
                        this.FindAverageColor(sourcePixels, imageWidth, imageHeight, y,
                            YStoppingPoint, x, XStoppingPoint);

                    this.setNewColorValue(sourcePixels, imageWidth, imageHeight, y, YStoppingPoint, x, XStoppingPoint, averageColor);

                    x += this.pixelArea;
                }
                y += this.pixelArea;
            }
        }

        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int y, int YStoppingPoint, int x,
            int XStoppingPoint, Color averageColor)
        {
            for (var YStartingPoint = y; YStartingPoint < YStoppingPoint; YStartingPoint++)
            {
                for (var XStartingPoint = x; XStartingPoint < XStoppingPoint; XStartingPoint++)
                {
                    var pixelColor = this.getPixelBgra8(sourcePixels, YStartingPoint, XStartingPoint, imageWidth, imageHeight);

                    if (this.outLineCheckbox.IsChecked == true)
                    {
                        if (coordinateIsValidForOutline(y, YStoppingPoint, x, XStoppingPoint, XStartingPoint, YStartingPoint))
                        {
                            pixelColor.R = 255;
                            pixelColor.B = 255;
                            pixelColor.G = 255;
                        }
                    }
                    else
                    {

                        pixelColor.R = averageColor.R;
                        pixelColor.B = averageColor.B;
                        pixelColor.G = averageColor.G;

                    }


                    this.setPixelBgra8(sourcePixels, YStartingPoint, XStartingPoint, pixelColor, imageWidth, imageHeight);
                }
            }
        }

        private static bool coordinateIsValidForOutline(int y, int YStoppingPoint, int x, int XStoppingPoint, int XStartingPoint, int YStartingPoint)
        {
            return CoordinateIsValidForOutline(x, XStoppingPoint, XStartingPoint) || CoordinateIsValidForOutline(y, YStoppingPoint, YStartingPoint);
        }

        private static bool CoordinateIsValidForOutline(int coordinate, int coordinateStoppingPoint, int coordinateStartingPoint)
        {
            return coordinateStartingPoint == coordinateStoppingPoint || coordinateStartingPoint == coordinate;
        }

        private int UpdateStoppingPoint(uint maxValue, int coordinate)
        {
            var CoordinateStoppingPoint = coordinate + this.pixelArea;
            if (CoordinateStoppingPoint > maxValue)
            {
                CoordinateStoppingPoint = (int) maxValue;
            }

            return CoordinateStoppingPoint;
        }

        private Color FindAverageColor(byte[] sourcePixels, uint imageWidth, uint imageHeight, int currentY, int YStoppingPoint, int currentX,
            int XStoppingPoint)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;

            for (var YStartingPoint = currentY; YStartingPoint < YStoppingPoint; YStartingPoint++)
            {
                for (var XStartingPoint = currentX; XStartingPoint < XStoppingPoint; XStartingPoint++)
                {
                    pixelCount++;
                    var pixelColor = this.getPixelBgra8(sourcePixels, YStartingPoint, XStartingPoint, imageWidth, imageHeight);
                    totalRed += pixelColor.R;
                    totalBlue += pixelColor.B;
                    totalGreen += pixelColor.G;
                }
            }

            var averageRed = totalRed / pixelCount;
            var averageBlue = totalBlue / pixelCount;
            var averageGreen = totalGreen / pixelCount;

            var newColor = new Color();
            newColor.R = (byte) averageRed;
            newColor.B = (byte) averageBlue;
            newColor.G = (byte) averageGreen;
            return newColor;
        }

        private void giveImageRedTint(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            for (var y = 0; y < imageHeight; y++)
            {
                for (var x = 0; x < imageWidth; x++)
                {
                    var pixelColor = this.getPixelBgra8(sourcePixels, y, x, imageWidth, imageHeight);
                    pixelColor.R = 255;
                    this.setPixelBgra8(sourcePixels, y, x, pixelColor, imageWidth, imageHeight);
                }
            }
        }

        private async Task<StorageFile> selectSourceImageFile()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }

        private Color getPixelBgra8(byte[] pixels, int x, int y, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            var r = pixels[offset + 2];
            var g = pixels[offset + 1];
            var b = pixels[offset + 0];
            return Color.FromArgb(0, r, g, b);
        }

        private void setPixelBgra8(byte[] pixels, int x, int y, Color color, uint width, uint height)
        {
            var offset = (x * (int)width + y) * 4;
            pixels[offset + 2] = color.R;
            pixels[offset + 1] = color.G;
            pixels[offset + 0] = color.B;
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.saveWritableBitmap();
        }


        private async void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            await this.HandleLoadPicture();
        }

        private async Task HandleLoadPicture()
        {
            this.selectedImageFile = await this.selectSourceImageFile();
            
            if (this.selectedImageFile != null)
            {
                var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(this.selectedImageFile);

                using (var fileStream = await this.selectedImageFile.OpenAsync(FileAccessMode.Read))
                    {
                        var decoder = await BitmapDecoder.CreateAsync(fileStream);

                        var transform = new BitmapTransform {
                            ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                            ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                        };

                        this.dpiX = decoder.DpiX;
                        this.dpiY = decoder.DpiY;

                        var pixelData = await decoder.GetPixelDataAsync(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Straight,
                            transform,
                            ExifOrientationMode.IgnoreExifOrientation,
                            ColorManagementMode.DoNotColorManage
                        );
                        
                        var sourcePixels = pixelData.DetachPixelData();

                        await this.createOrignalImage(decoder, sourcePixels);
                    }
            }
        }

        private async Task handleCreatingSolidMosaicImage()
        {

            if (this.selectedImageFile != null)
            {
                var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(this.selectedImageFile);

                using (var fileStream = await this.selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(fileStream);

                    var transform = new BitmapTransform
                    {
                        ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                        ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                    };

                    this.dpiX = decoder.DpiX;
                    this.dpiY = decoder.DpiY;

                    var pixelData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        transform,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage
                    );

                    var sourcePixels = pixelData.DetachPixelData();

                    await this.createMosaicImage(decoder, sourcePixels);
                }
            }
        }

        private async Task createMosaicImage(BitmapDecoder decoder, byte[] sourcePixels)
        {
           this.createSolidMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

            this.modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
            using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                this.AlterImageDisplay.Source = this.modifiedImage;
            }
        }

        private async Task createOrignalImage(BitmapDecoder decoder, byte[] sourcePixels)
        {
            this.orignalImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
            using (var writeStream = this.orignalImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                this.imageDisplay.Source = this.orignalImage;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.PixelAreaOf5.IsChecked == true)
            {
                this.pixelArea = 5;
            }
            if (this.PixelAreaOf15.IsChecked == true)
            {
                this.pixelArea = 15;
            }
            if (this.PixelAreaOf25.IsChecked == true)
            {
                this.pixelArea = 25;
            }
            if (this.PixelAreaOf55.IsChecked == true)
            {
                this.pixelArea = 55;
            }

        }

        private async void CreateMosaicImageButton_Click(object sender, RoutedEventArgs e)
        {


           await this.handleCreatingSolidMosaicImage();
        }
    }
}
