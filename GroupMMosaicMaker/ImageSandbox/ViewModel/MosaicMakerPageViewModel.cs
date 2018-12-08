using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Annotations;
using ImageSandbox.Datatier;
using ImageSandbox.Model;
using ImageSandbox.Util;

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
        private readonly ImagePalette imagePalete;
        private List<WriteableBitmap> selectedFolderImages;
        private readonly ImageFolderReader folderReader;
        private List<FolderImage> loadedFolder;

        private bool hasMosaic;
        private bool canSave;
        private bool isBlackAndWhite;
        private bool hasGrid;

        private bool isCreatePictureMosaicEnabled;

        private bool isGridCheckEnabled;

        private bool hasTriangleGrid;

        private string numberOfImages;

        #endregion

        #region Properties

        public List<WriteableBitmap> SelectedFolderImages
        {
            get => this.selectedFolderImages;
            set
            {
                this.selectedFolderImages = value;
            }
        }

        public RelayCommand CreateSolidMosaic { get; set; }
        public RelayCommand TriangleMosaic { get; set; }
        public RelayCommand ChangeBlockSize { get; set; }
        public RelayCommand TriangleGridChecked { get; set; }
        public RelayCommand GridChecked { get; set; }
        public RelayCommand NoGridChecked { get; set; }
        public string LoadedFileType { get; private set; }

        public bool IsCreatePictureMosaicEnabled
        {
            get => this.isCreatePictureMosaicEnabled;
            set
            {
                this.isCreatePictureMosaicEnabled = value;
                this.OnPropertyChanged();
                this.CreateSolidMosaic.OnCanExecuteChanged();
                this.TriangleMosaic.OnCanExecuteChanged();
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
                this.TriangleMosaic.OnCanExecuteChanged();
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

        public bool HasTriangleGrid
        {
            get => this.hasTriangleGrid;
            set
            {
                this.hasTriangleGrid = value;
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
                this.TriangleMosaic.OnCanExecuteChanged();
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
            this.dpiX = 0;
            this.dpiY = 0;
            this.loadAllCommands();
            this.SelectedFolderImages = new List<WriteableBitmap>();
            this.folderReader = new ImageFolderReader();
            this.loadedFolder = new List<FolderImage>();
            this.imagePalete = new ImagePalette();
            this.NumberOfImages = "" + 0;
        }

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;

        private void loadAllCommands()
        {
            this.CreateSolidMosaic = new RelayCommand(this.createSolidMosaic, this.canMosaic);
            this.TriangleMosaic = new RelayCommand(this.createTriangleMosaic, this.canMosaic);
            this.ChangeBlockSize = new RelayCommand(this.changeBlockSize, this.canChangeBlockSize);
            this.TriangleGridChecked = new RelayCommand(this.createTriangleGrid, this.canCreateGrid);
            this.GridChecked = new RelayCommand(this.createGrid, this.canCreateGrid);
            this.NoGridChecked = new RelayCommand(this.createNoGrid, this.canAlwaysExecute);
        }

        private async void createTriangleMosaic(object obj)
        {
            await this.handleCreatingSolidMosaicImage(true);

        }

        private bool canCreateGrid(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50);
        }

        private async void createTriangleGrid(object obj)
        {
            this.HasTriangleGrid = true;
            await this.creatingOutlineOrignalImage();
        }

        private async void createGrid(object obj)
        {
            this.HasGrid = true;
            await this.creatingOutlineOrignalImage();
        }

        private void createNoGrid(object obj)
        {
            this.HasGrid = false;
            this.ImageDisplay = this.orignalImage;
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
            if (this.MosaicImage != null)
            {
                this.MosaicImage.BlockSize = this.blockSizeNumber;
            }
        }

        private bool canMosaic(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50);
        }

        private async void createSolidMosaic(object obj)
        {
            await this.handleCreatingSolidMosaicImage(false);
            if (this.HasGrid)
            {
                await this.creatingOutlineOrignalImage();
            }

            this.CanSave = true;
        }

        private async Task handleCreatingSolidMosaicImage(bool isTriangleMosaic)
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
                if (this.IsBlackAndWhite)
                {
                    this.MosaicImage.CreateBlackAndWhiteMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
                }else if (isTriangleMosaic)
                { 
                    this.MosaicImage.CreateTriangleMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
                }else
                {
                    this.MosaicImage.CreateSolidMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
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
            await this.imagePalete.ResizeAllImages(this.blockSizeNumber);

            var copyBitmapImage = await this.MakeACopyOfTheFileToWorkOn(this.selectedImageFile);

            using (var fileStream = await this.selectedImageFile.OpenAsync(FileAccessMode.Read))

            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform {
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
                    this.MosaicImage.CreateBlackAndWhiteMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);
                }
                else
                {
                    this.MosaicImage.CreatePictureMosaic(sourcePixels, decoder.PixelWidth,
                        decoder.PixelHeight, this.imagePalete);
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

        public void LoadAllImagesIntoImagePalette()
        {
            foreach (var currImage in this.loadedFolder)
            {
                this.imagePalete.Add(currImage);
            }

            this.loadedFolder.Clear();
        }

        public async Task LoadAllFolderImages(StorageFolder selectedFolder)
        {
            this.loadedFolder = (List<FolderImage>) await this.folderReader.LoadSelectedFolder(selectedFolder);
            this.CheckToEnablePictureMosaic();
            this.LoadAllImagesIntoImagePalette();
            this.UpdateImagePaletteCount();
        }

        private void UpdateImagePaletteCount()
        {
            this.NumberOfImages = "" + this.imagePalete.Count;
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
                this.MosaicImage = new MosaicImage(imageFile, this.blockSizeNumber);

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
                    if (this.HasGrid)
                    {
                        await this.creatingOutlineOrignalImage();
                    }
                }
            }
        }

        private void CheckToEnablePictureMosaic()
        {
            this.IsCreatePictureMosaicEnabled =
                this.blockSizeNumber != 0 && this.orignalImage != null && this.checkForImagePalette();
        }

        private bool checkForImagePalette()
        {
            return this.loadedFolder.Any() || this.imagePalete.Any();
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
                    var lineY = 0;
                    for (var currentYPoint = startingYpoint; currentYPoint < YStoppingPoint; currentYPoint++)
                    {
                        var lineX = 0;
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
                                    imageWidth, imageHeight);
                            }
                            else if (this.HasTriangleGrid && lineX == lineY)
                            {
                                pixelColor = Colors.White;
                                ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor,
                                    imageWidth, imageHeight);
                            }

                            lineX++;
                        }

                        lineY++;
                    }

                    startingXpoint += this.blockSizeNumber;
                }

                startingYpoint += this.blockSizeNumber;
            }

            var triangleCoordinates = this.MosaicImage.FindTrianglePoints(imageWidth, imageHeight);
            if (this.HasTriangleGrid)

            {
                foreach (var currentPoint in triangleCoordinates)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1,
                        imageWidth, imageHeight);
                    pixelColor = Colors.White;
                    ImagePixel.setPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1, pixelColor,
                        imageWidth, imageHeight);
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

        private async Task creatingOutlineOrignalImage()
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

                this.createOrignalImageWithOutline(sourcePixels, decoder.PixelWidth, decoder.PixelHeight);

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
                if (this.IsBlackAndWhite & (this.MosaicImage != null))
                {
                    await this.handleCreatingSolidMosaicImage(false);
                }
                else
                {
                    if (this.MosaicImage != null)
                    {
                        this.ImageDisplay = this.orignalImage;
                        await this.handleCreatingSolidMosaicImage(false);
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