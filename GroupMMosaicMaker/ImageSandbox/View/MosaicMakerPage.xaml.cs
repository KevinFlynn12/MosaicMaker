﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.ViewModel;

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
        private WriteableBitmap outlineOrignalImage;
        private int blockSize;
        private StorageFile selectedImageFile;
        private MosaicMakerPageViewModel viewModel;

        #endregion

        #region Constructors

        public MainPage()
        {
            
            this.InitializeComponent();
            this.ModifyMoasicButton.IsEnabled = false;
            this.RefreshMosaicButton.IsEnabled = false;
            this.viewModel = new MosaicMakerPageViewModel();
            this.DataContext = this.viewModel;
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

            await this.viewModel.SavePicture(savefile);
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

                    x += this.blockSize;
                }
                y += this.blockSize;
            }
        }

        private void setNewColorValue(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint,
            int XStoppingPoint, Color averageColor)
        {
            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    var pixelColor = this.getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);

                    if (this.outLineCheckbox.IsChecked == true)
                    {
                        if (currentYPoint == startingYPoint || YStoppingPoint == currentYPoint
                                                || currentXPoint == startingXPoint || XStoppingPoint == currentXPoint)
                        {
                            pixelColor = Colors.White;

                        }
                        else
                        {
                            pixelColor.R = averageColor.R;
                            pixelColor.B = averageColor.B;
                            pixelColor.G = averageColor.G;
                        }
                    }
                    else
                    {
                        pixelColor.R = averageColor.R;
                        pixelColor.B = averageColor.B;
                        pixelColor.G = averageColor.G;
                    }


                    this.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth, imageHeight);
                }
            }
        }

        private void createOrignalImageWithOutline(byte[] sourcePixels, uint imageWidth, uint imageHeight)
        {
            var startingYpoint = 0;
            while (startingYpoint < imageHeight)
            {
                var startingXpoint = 0;
                while (startingXpoint < imageWidth)
                {
                    var XStoppingPoint = this.UpdateStoppingPoint(imageWidth, startingXpoint);

                    var YStoppingPoint = this.UpdateStoppingPoint(imageHeight, startingYpoint);

                    for (var currentYPoint = startingYpoint; currentYPoint < YStoppingPoint; currentYPoint++)
                    {
                        for (var currentXPoint = startingXpoint; currentXPoint < XStoppingPoint; currentXPoint++)
                        {
                            var pixelColor = this.getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);

                          
                          if (currentYPoint == startingYpoint || YStoppingPoint == currentYPoint
                                                  || currentXPoint == startingXpoint || XStoppingPoint == currentXPoint)
                          {
                            pixelColor = Colors.White;
                            this.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth, imageHeight);

                            }

                        }
                    }

                    startingXpoint += this.blockSize;
                }
                startingYpoint += this.blockSize;
            }
        }

        private static bool validCoordinatesForOutline(int startingCoordinate, int currentCoordinate, int coordinateStopingPoint)
        {
            return currentCoordinate == startingCoordinate || currentCoordinate == coordinateStopingPoint;
        }

        private int UpdateStoppingPoint(uint maxValue, int coordinate)
        {
            var CoordinateStoppingPoint = coordinate + this.blockSize;
            if (CoordinateStoppingPoint > maxValue)
            {
                CoordinateStoppingPoint = (int) maxValue;
            }

            return CoordinateStoppingPoint;
        }

        private Color FindAverageColor(byte[] sourcePixels, uint imageWidth, uint imageHeight, int startingYPoint, int YStoppingPoint, int startingXPoint,
            int XStoppingPoint)
        {
            var pixelCount = 0.0;
            var totalRed = 0.0;
            var totalBlue = 0.0;
            var totalGreen = 0.0;

            for (var currentYPoint = startingYPoint; currentYPoint < YStoppingPoint; currentYPoint++)
            {
                for (var currentXPoint = startingXPoint; currentXPoint < XStoppingPoint; currentXPoint++)
                {
                    pixelCount++;
                    var pixelColor = this.getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);
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
            this.selectedImageFile = await this.selectSourceImageFile();
            if (this.selectedImageFile != null)
            {
                await this.viewModel.LoadPicture(this.selectedImageFile);
            }

            //await this.HandleLoadPicture();
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

                    await this.createSolidMosaicImage(decoder, sourcePixels);
                }
        }
        private async Task createSolidMosaicImage(BitmapDecoder decoder, byte[] sourcePixels)
        {
            this.createSolidMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

            await this.handleCreatingMosaicImage(decoder, sourcePixels);
        }




        private async Task handleCreatingOutlineOrignalImage()
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

                await this.createOutlineOrignalImage(decoder, sourcePixels);
            }
        }

        private async Task createOutlineOrignalImage(BitmapDecoder decoder, byte[] sourcePixels)
        {
            this.createOrignalImageWithOutline(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

            this.outlineOrignalImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
            using (var writeStream = this.outlineOrignalImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                this.imageDisplay.Source = this.outlineOrignalImage;
            }
        }

        
       

        private async Task handleCreatingMosaicImage(BitmapDecoder decoder, byte[] sourcePixels)
        {
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

        private async void CreateMosaicImageButton_Click(object sender, RoutedEventArgs e)
        {
            
           await this.handleCreatingSolidMosaicImage();

            if(this.outLineCheckbox.IsChecked == true)
            {
                await this.handleCreatingOutlineOrignalImage();
            }
        }

        private void TextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            throw new NotImplementedException();
        }
    }
}