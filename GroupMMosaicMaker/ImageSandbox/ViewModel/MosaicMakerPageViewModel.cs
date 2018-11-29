using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Annotations;
using ImageSandbox.Model;
using ImageSandbox.Util;
namespace ImageSandbox.ViewModel
{
    class MosaicMakerPageViewModel: INotifyPropertyChanged
    {
        #region Data members

        private double dpiX;
        private double dpiY;
        private WriteableBitmap modifiedImage;
        private WriteableBitmap orignalImage;
        private WriteableBitmap outlineOrignalImage;
        private String blockSize;
        private int blockSizeNumber;
        private StorageFile selectedImageFile;
        private WriteableBitmap imageDisplay;
        private WriteableBitmap alterImageDisplay;
        private MosaicImage mosaicImage;
        public RelayCommand CreateSolidMosaic { get; set; }
        private bool hasGrid;

        public bool HasGrid
        {
            get => this.hasGrid;
            set
            {
                this.hasGrid = value;
                this.OnPropertyChanged();
            }
        }

        public WriteableBitmap ImageDisplay
        {
            get => this.imageDisplay;
            set
            {
                this.imageDisplay = value;
                this.OnPropertyChanged();
            }
        }
        public WriteableBitmap AlterImageDisplay
        {
            get => this.alterImageDisplay;
            set
            {
                this.alterImageDisplay = value;
                this.OnPropertyChanged();
            }
        }

        public String BlockSize
        {
            get => this.blockSize;
            set
            {
                this.blockSize = value;
                this.blockSizeNumber = int.Parse(this.BlockSize);
                this.OnPropertyChanged();
            }
        }

        public MosaicImage MosaicImage
        {
            get => this.mosaicImage;
            set
            {
                this.mosaicImage = value;
                this.OnPropertyChanged();
            }
        }



        #endregion

        #region Constructors

        public MosaicMakerPageViewModel()
        {
            this.HasGrid = false;
            this.BlockSize = "5";
            this.dpiX = 0;
            this.dpiY = 0;
            this.loadAllCommands();
        }

        #endregion

        private void loadAllCommands()
        {
            this.CreateSolidMosaic = new RelayCommand(this.createSolidMosaic, this.canAlwaysExecute);
        }

        private async void createSolidMosaic(object obj)
        {
            await this.handleCreatingSolidMosaicImage();
            if (this.HasGrid)
            {
                await this.creatingOutlineOrignalImage();
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

                this.MosaicImage.CreateSolidMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight, this.blockSizeNumber, this.HasGrid);

                this.modifiedImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.AlterImageDisplay = this.modifiedImage;
                }
            }
        }
        private bool canAlwaysExecute(object obj)
        {
            return true;
        }

        /// <summary>
        /// Loads the picture.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <returns></returns>
        public async Task LoadPicture(StorageFile imageFile)
        {
            this.selectedImageFile = imageFile;

            if (this.selectedImageFile != null)
            {
                this.MosaicImage = new MosaicImage(this.HasGrid, imageFile);
                
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

                    await this.createOriginalImage(decoder, sourcePixels);
                }
            }
        }
        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputstream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputstream);
            return newImage;
        }
        private async Task createOriginalImage(BitmapDecoder decoder, byte[] sourcePixels)
        {
            this.orignalImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
            using (var writeStream = this.orignalImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);

                this.ImageDisplay = this.orignalImage;
            }
        }

        /// <summary>
        /// Saves the pircture.
        /// </summary>
        /// <param name="saveFile">The save file.</param>
        /// <returns></returns>
        public async Task SavePicture(StorageFile saveFile)
        {
            if (saveFile != null)
            {
                var stream = await saveFile.OpenAsync(FileAccessMode.ReadWrite);
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
                            var pixelColor = this.MosaicImage.getPixelBgra8(sourcePixels, currentYPoint, currentXPoint, imageWidth, imageHeight);


                            if (currentYPoint == startingYpoint || YStoppingPoint == currentYPoint
                                                                || currentXPoint == startingXpoint || XStoppingPoint == currentXPoint)
                            {
                                pixelColor = Colors.White;
                                this.MosaicImage.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor, imageWidth, imageHeight);

                            }

                        }
                    }

                    startingXpoint += this.blockSizeNumber;
                }
                startingYpoint += this.blockSizeNumber;
            }
        }
        private int UpdateStoppingPoint(uint maxValue, int coordinate)
        {
            var CoordinateStoppingPoint = coordinate + this.blockSizeNumber;
            if (CoordinateStoppingPoint > maxValue)
            {
                CoordinateStoppingPoint = (int)maxValue;
            }

            return CoordinateStoppingPoint;
        }
        

        private async Task creatingOutlineOrignalImage()
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

                this.createOrignalImageWithOutline(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

                this.outlineOrignalImage = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                using (var writeStream = this.outlineOrignalImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.ImageDisplay = this.outlineOrignalImage;
                }
            }
           
        }
        
        public async Task GridCheckboxChanged()
        {
            if (this.HasGrid == true)
            {
                await this.creatingOutlineOrignalImage();
            }
            else if (this.HasGrid == false)
            {
                this.ImageDisplay = this.orignalImage;
            }
        }
       
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
