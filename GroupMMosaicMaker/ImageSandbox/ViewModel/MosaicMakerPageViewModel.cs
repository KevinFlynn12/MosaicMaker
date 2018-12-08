using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Annotations;
using ImageSandbox.Datatier;
using ImageSandbox.Model;
using ImageSandbox.Util;
using ImageSandbox.View;

namespace ImageSandbox.ViewModel
{
    internal class MosaicMakerPageViewModel : INotifyPropertyChanged
    {
        #region Data members

        private double dpiX;
        private double dpiY;
        private WriteableBitmap modifiedImage;
        private WriteableBitmap orignalImage;
        private WriteableBitmap outlineOrignalImage;
        private string blockSize;
        private int blockSizeNumber;
        private StorageFile selectedImageFile;
        private WriteableBitmap imageDisplay;
        private WriteableBitmap alterImageDisplay;
        private MosaicImage mosaicImage;
        private FolderImageRegistry imagePallete;
        private List<WriteableBitmap> selectedFolderImages;
        private ImageFolderReader folderReader;
        private List<FolderImage> loadedFolder;

        public List<WriteableBitmap> SelectedFolderImages
        {
            get => this.selectedFolderImages;
            set
            {
                this.selectedFolderImages = value;
                this.ViewPalette.OnCanExecuteChanged();
            }
        }

        private bool hasMosaic;
        private bool canSave;
        private bool isBlackAndWhite;
        private bool hasGrid;

        private bool isCreatePictureMosaicEnabled;

        private bool isGridCheckEnabled;

        #endregion

        #region Properties

        public RelayCommand CreateSolidMosaic { get; set; }
        public RelayCommand ChangeBlockSize { get; set; }
        public RelayCommand ViewPalette { get; set; }
        public RelayCommand TriangleGridChecked { get; set; }
        public RelayCommand GridChecked { get; set; }
        public RelayCommand NoGridChecked { get; set; }
        public String LoadedFileType { get; private set; }

        public bool IsCreatePictureMosaicEnabled
        {
            get => this.isCreatePictureMosaicEnabled;
            set
            {
                this.isCreatePictureMosaicEnabled = value;
                this.OnPropertyChanged();
                this.CreateSolidMosaic.OnCanExecuteChanged();
            }
        }

        public bool IsGridCheckEnabled
        {
            get => this.isGridCheckEnabled;
            set
            {
                this.isGridCheckEnabled = value;
                this.OnPropertyChanged();
                this.ChangeBlockSize.OnCanExecuteChanged();
            }
        }

        public bool CanSave
        {
            get => this.canSave;
            set
            {
                this.canSave = value;
                this.OnPropertyChanged();
                this.CreateSolidMosaic.OnCanExecuteChanged();
            }
        }

        public bool HasMosaic
        {
            get => this.hasMosaic;
            set
            {
                this.hasMosaic = value;
                this.OnPropertyChanged();
            }
        }

        public bool HasGrid
        {
            get => this.hasGrid;
            set
            {
                this.hasGrid = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsBlackAndWhite
        {
            get => this.isBlackAndWhite;
            set
            {
                this.isBlackAndWhite = value;
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
                this.CreateSolidMosaic.OnCanExecuteChanged();
                this.TriangleGridChecked.OnCanExecuteChanged();
                this.GridChecked.OnCanExecuteChanged();
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

        public string BlockSize
        {
            get => this.blockSize;
            set
            {
                this.blockSize = value;
                this.OnPropertyChanged();
                this.ChangeBlockSize.OnCanExecuteChanged();
                
                
            }
        }

        private string numberOfImages;
        public string NumberOfImages
        {
            get => this.numberOfImages;
            set
            {
                this.numberOfImages = value;
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
            this.dpiX = 0;
            this.dpiY = 0;
            this.loadAllCommands();
            this.SelectedFolderImages = new List<WriteableBitmap>();
            this.folderReader = new ImageFolderReader();
            this.loadedFolder = new List<FolderImage>();
            this.imagePallete = new FolderImageRegistry();
            this.NumberOfImages = "" + 0;

        }

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;

        private void loadAllCommands()
        {
            this.CreateSolidMosaic = new RelayCommand(this.createSolidMosaic, this.canSolidMosaic);
            this.ChangeBlockSize = new RelayCommand(this.changeBlockSize, this.canChangeBlockSize);
            this.ViewPalette = new RelayCommand(this.viewPalette, this.canViewPalette);
            this.TriangleGridChecked = new RelayCommand(this.createTriangleGrid, this.canCreateGrid);
            this.GridChecked = new RelayCommand(this.createGrid, this.canCreateGrid);
            this.NoGridChecked = new RelayCommand(this.createNoGrid, this.canAlwaysExecute);
        }

        private bool canCreateGrid(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50);
        }

        private async void createTriangleGrid(object obj)
        {
            this.HasGrid = true;
            await this.creatingOutlineOrignalImage(true);
        }

        private async void createGrid(object obj)
        {
            this.HasGrid = true;
            await this.creatingOutlineOrignalImage(false);
        }

        private void createNoGrid(object obj)
        {
            this.HasGrid = false;
            this.ImageDisplay = this.orignalImage;
        }

        private bool canViewPalette(object obj)
        {
            return this.selectedFolderImages.Count != 0;
        }

        private async void viewPalette(object obj)
        {
            var contentDialog = new ReviseImagePalletteDialog();
            await contentDialog.ShowAsync();
        }

        private bool canChangeBlockSize(object obj)
        {
            int parsedBlockSize;
            int.TryParse(this.BlockSize, out parsedBlockSize);
            return parsedBlockSize >= 5 && parsedBlockSize <= 50;
        }

        private void changeBlockSize(object obj)
        {
            this.blockSizeNumber = int.Parse(this.BlockSize);
            this.CheckToEnablePictureMosaic();
            this.IsGridCheckEnabled = true;
            this.CreateSolidMosaic.OnCanExecuteChanged();
            this.TriangleGridChecked.OnCanExecuteChanged();
            this.GridChecked.OnCanExecuteChanged();

        }

        private bool canSolidMosaic(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50);
        }

        private async void createSolidMosaic(object obj)
        {
            await this.handleCreatingSolidMosaicImage();
            if (this.HasGrid)
            {
                await this.creatingOutlineOrignalImage(false);
            }

            this.hasMosaic = true;
            this.CanSave = true;
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
                if (this.IsBlackAndWhite)
                {
                    this.MosaicImage.CreateBlackAndWhiteMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight,
                        this.blockSizeNumber, this.HasGrid);
                }
                else
                {
                    this.MosaicImage.CreateSolidMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight,
                        this.blockSizeNumber, this.HasGrid);
                }

                this.modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.AlterImageDisplay = this.modifiedImage;
                }
            }
        }

        /// <summary>
        ///     Displays the picture mosaic.
        /// </summary>
        /// <param name="selectedFolder">The selected folder.</param>
        public async void DisplayPictureMosaic(StorageFolder selectedFolder)
        {
            if (this.loadedFolder.Any())
            {
                this.LoadAllImagesIntoImagePallete();
            }
            await this.imagePallete.ResizeAllImages(this.blockSizeNumber);

            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(this.selectedImageFile);

            using (var fileStream = await this.selectedImageFile.OpenAsync(FileAccessMode.Read))

            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

             

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                if (this.IsBlackAndWhite)
                {
                    this.MosaicImage.CreateBlackAndWhiteMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight,
                        this.blockSizeNumber, this.HasGrid);
                }
                else
                {
                    this.MosaicImage.CreatePictureMosaic(sourcePixels, decoder.PixelWidth,
                        decoder.PixelHeight, this.blockSizeNumber, this.imagePallete);
                }

                this.modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.AlterImageDisplay = this.modifiedImage;
                }
            }

            this.hasMosaic = true;
            this.CanSave = true;
        }


        public void LoadAllImagesIntoImagePallete()
        {
            foreach (var currImage in this.loadedFolder)
            {
                this.imagePallete.Add(currImage);
            }
            this.loadedFolder.Clear();
        }
        public async Task LoadAllFolderImages(StorageFolder selectedFolder)
        {

            this.loadedFolder = (List<FolderImage>) await this.folderReader.LoadSelectedFolder(selectedFolder);
            this.CheckToEnablePictureMosaic();

            this.NumberOfImages = "" + this.SelectedFolderImages.Count;

        }







        
        private bool canAlwaysExecute(object obj)
        {
            return true;
        }

        /// <summary>
        ///     Loads the picture.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <returns></returns>
        public async Task LoadPicture(StorageFile imageFile)
        {
            this.selectedImageFile = imageFile;

            this.LoadedFileType = imageFile.FileType;

            if (this.selectedImageFile != null)
            {
                this.MosaicImage = new MosaicImage(this.HasGrid, imageFile);

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
                    await this.createOriginalImage(decoder, sourcePixels);
                    if (this.HasGrid)
                    {
                        await this.creatingOutlineOrignalImage(false);
                    }
                }

            }


        }
    

        private void CheckToEnablePictureMosaic()
        {
            this.IsCreatePictureMosaicEnabled = this.blockSizeNumber != 0 && this.orignalImage != null && this.checkForImagePalette();
        }


        private bool checkForImagePalette()
        {
            return this.loadedFolder.Any() || this.imagePallete.Any();
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
        ///     Saves the picture.
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
                    (uint) this.modifiedImage.PixelWidth,
                    (uint) this.modifiedImage.PixelHeight, this.dpiX, this.dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }

        private void createOrignalImageWithOutline(byte[] sourcePixels, uint imageWidth, uint imageHeight, bool isTriangle)
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
                            var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentYPoint, currentXPoint,
                                imageWidth, imageHeight);

                            if (currentYPoint == startingYpoint || YStoppingPoint == currentYPoint
                                                                || currentXPoint == startingXpoint ||
                                                                XStoppingPoint == currentXPoint)
                            {
                                pixelColor = Colors.White;
                                ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor,
                                    imageWidth, imageHeight, this.isBlackAndWhite);
                            }
                        }
                    }

                    startingXpoint += this.blockSizeNumber;
                }

                startingYpoint += this.blockSizeNumber;
            }
            var triangleCoordinates =
                this.MosaicImage.FindTrianglePoints(imageWidth, imageHeight, this.blockSizeNumber);
            if (isTriangle)
            {
                foreach (var currentPoint in triangleCoordinates)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1,
                        imageWidth, imageHeight);
                    pixelColor = Colors.White;
                    ImagePixel.setPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1, pixelColor,
                        imageWidth, imageHeight, false);
                }
            }
        }

        private int UpdateStoppingPoint(uint maxValue, int coordinate)
        {
            var CoordinateStoppingPoint = coordinate + this.blockSizeNumber;
            if (CoordinateStoppingPoint > maxValue)
            {
                CoordinateStoppingPoint = (int) maxValue;
            }

            return CoordinateStoppingPoint;
        }

        private async Task creatingOutlineOrignalImage(bool isTriangle)
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

                this.createOrignalImageWithOutline(sourcePixels, decoder.PixelWidth, decoder.PixelHeight, isTriangle);

                this.outlineOrignalImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = this.outlineOrignalImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.ImageDisplay = this.outlineOrignalImage;
                }
            }
        }

        
        public async Task BlackAndWhiteCheckboxChanged()
        {
            if (this.orignalImage != null)
            {
                if (this.IsBlackAndWhite & this.HasMosaic)
                {
                    await this.handleCreatingSolidMosaicImage();
                }
                else
                {
                    if (this.HasMosaic)
                    {
                        this.ImageDisplay = this.orignalImage;
                        await this.handleCreatingSolidMosaicImage();
                    }
                    else
                    {
                        this.ImageDisplay = this.orignalImage;
                    }
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}