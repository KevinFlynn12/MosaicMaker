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

        private bool hasTriangleMosaic;
        private bool hasSolidMosaic;
        private bool hasPictureMosaic;

        private bool hasBlockSizeChanged;
        private bool canSave;
        private bool isBlackAndWhite;
        private bool useAllImagesOnce;
        private bool hasGrid;

        private bool isCreatePictureMosaicEnabled;

        private bool isGridCheckEnabled;

        private bool hasTriangleGrid;

        private string numberOfImages;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the selected folder images.
        /// </summary>
        /// <value>
        /// The selected folder images.
        /// </value>
        public List<WriteableBitmap> SelectedFolderImages
        {
            get => this.selectedFolderImages;
            set
            {
                this.OnPropertyChanged();
                this.selectedFolderImages = value;
            }
        }

        /// <summary>
        /// Gets or sets the create solid mosaic.
        /// </summary>
        /// <value>
        /// The create solid mosaic.
        /// </value>
        public RelayCommand CreateSolidMosaic { get; set; }
        /// <summary>
        /// Gets or sets the triangle mosaic.
        /// </summary>
        /// <value>
        /// The triangle mosaic.
        /// </value>
        public RelayCommand TriangleMosaic { get; set; }
        /// <summary>
        /// Gets or sets the size of the change block.
        /// </summary>
        /// <value>
        /// The size of the change block.
        /// </value>
        public RelayCommand ChangeBlockSize { get; set; }
        /// <summary>
        /// Gets or sets the triangle grid checked.
        /// </summary>
        /// <value>
        /// The triangle grid checked.
        /// </value>
        public RelayCommand TriangleGridChecked { get; set; }
        /// <summary>
        /// Gets or sets the grid checked.
        /// </summary>
        /// <value>
        /// The grid checked.
        /// </value>
        public RelayCommand GridChecked { get; set; }
        /// <summary>
        /// Gets or sets the no grid checked.
        /// </summary>
        /// <value>
        /// The no grid checked.
        /// </value>
        public RelayCommand NoGridChecked { get; set; }
        
        public RelayCommand ClearPalette { get; set; }
        public RelayCommand UseImagesOnce { get; set; }

        /// <summary>
        /// Gets the type of the loaded file.
        /// </summary>
        /// <value>
        /// The type of the loaded file.
        /// </value>
        public string LoadedFileType { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is create picture mosaic enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is create picture mosaic enabled; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is grid check enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is grid check enabled; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance can save.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can save; otherwise, <c>false</c>.
        /// </value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance has grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has grid; otherwise, <c>false</c>.
        /// </value>
        public bool HasGrid
        {
            get => this.hasGrid;
            set
            {
                this.hasGrid = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance has triangle grid.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has triangle grid; otherwise, <c>false</c>.
        /// </value>
        public bool HasTriangleGrid
        {
            get => this.hasTriangleGrid;
            set
            {
                this.hasTriangleGrid = value;
                this.OnPropertyChanged();
            }
        }


        public bool UseAllImagesOnce
        {
            get => this.useAllImagesOnce;
            set
            {
                this.useAllImagesOnce = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is black and white.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is black and white; otherwise, <c>false</c>.
        /// </value>
        public bool IsBlackAndWhite
        {
            get => this.isBlackAndWhite;
            set
            {
                this.isBlackAndWhite = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the image display.
        /// </summary>
        /// <value>
        /// The image display.
        /// </value>
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

        /// <summary>
        /// Gets or sets the alter image display.
        /// </summary>
        /// <value>
        /// The alter image display.
        /// </value>
        public WriteableBitmap AlterImageDisplay
        {
            get => this.alterImageDisplay;
            set
            {
                this.alterImageDisplay = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the size of the block.
        /// </summary>
        /// <value>
        /// The size of the block.
        /// </value>
        public string BlockSize
        {
            get => this.blockSize;
            set
            {
                this.blockSize = value;
                this.OnPropertyChanged();
                this.ChangeBlockSize.OnCanExecuteChanged();
                this.hasBlockSizeChanged = true;
            }
        }

        /// <summary>
        /// Gets or sets the number of images.
        /// </summary>
        /// <value>
        /// The number of images.
        /// </value>
        public string NumberOfImages
        {
            get => this.numberOfImages;
            set
            {
                this.numberOfImages = value;
                this.OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the mosaic image.
        /// </summary>
        /// <value>
        /// The mosaic image.
        /// </value>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="MosaicMakerPageViewModel"/> class.
        /// </summary>
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
            this.CreateSolidMosaic = new RelayCommand(this.createSolidMosaic, this.canSolidMosaic);
            this.TriangleMosaic = new RelayCommand(this.createTriangleMosaic, this.canTriangleMosaic);
            this.ChangeBlockSize = new RelayCommand(this.changeBlockSize, this.canChangeBlockSize);
            this.TriangleGridChecked = new RelayCommand(this.createTriangleGrid, this.canCreateGrid);
            this.GridChecked = new RelayCommand(this.createGrid, this.canCreateGrid);
            this.NoGridChecked = new RelayCommand(this.createNoGrid, this.canAlwaysExecute);
            
            this.ClearPalette = new RelayCommand(this.clearPalette, this.canClearPalette);
            this.UseImagesOnce = new RelayCommand(this.useImagesOnce, this.canAlwaysExecute);
        }

        private void useImagesOnce(object obj)
        {
            this.UseAllImagesOnce = true;
        }

        private bool canClearPalette(object obj)
        {
            //TODO
            return true;
        }

        private void clearPalette(object obj)
        {
            //throw new NotImplementedException();
        }
        

        private void addImage(StorageFile file)
        {
            //this.imagePalete.Add(new FolderImage());

        }

        private async void createSolidMosaic(object obj)
        {
            this.changeMosaicType(true, false, false);
            await this.handleCreatingSolidMosaicImage();
            
            this.CanSave = true;
        }
        private bool canTriangleMosaic(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50) &  (!this.hasTriangleMosaic || this.hasBlockSizeChanged);
        }



        private bool canCreateGrid(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50);
        }

        private async void createTriangleGrid(object obj)
        {
            this.changeGridType(false, true);
            await this.creatingOutlineOrignalImage();
            
        }

        private async void createGrid(object obj)
        {
            this.changeGridType(true, false);
            await this.creatingOutlineOrignalImage();
        }

        private void createNoGrid(object obj)
        {
            this.changeGridType(false, false);
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

            
            this.IsGridCheckEnabled = true;
            
            if (this.MosaicImage != null)
            {
                this.MosaicImage.BlockSize = this.blockSizeNumber;
            }
            this.CheckToEnablePictureMosaic();
            this.CreateSolidMosaic.OnCanExecuteChanged();
            this.TriangleMosaic.OnCanExecuteChanged();
            this.TriangleGridChecked.OnCanExecuteChanged();
            this.GridChecked.OnCanExecuteChanged();
            this.hasBlockSizeChanged = false;
        }

        private bool canSolidMosaic(object obj)
        {
            return (this.selectedImageFile != null) & (this.blockSizeNumber >= 5) & (this.blockSizeNumber <= 50) &
                   (!this.hasSolidMosaic || this.hasBlockSizeChanged);
        }

        private async void createTriangleMosaic(object obj)
        {
            this.changeMosaicType(false, true, false);

            await this.handleCreatingSolidMosaicImage();
            if (this.HasGrid)
            {
                await this.creatingOutlineOrignalImage();
            }

            this.CanSave = true;
        }
        private bool canAlwaysExecute(object obj)
        {
            return true;
        }
        private async Task handleCreatingSolidMosaicImage()
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
                if (this.hasTriangleMosaic)
                { 
                    this.MosaicImage.CreateTriangleMosaic(sourcePixels, isBlackAndWhite);
                    
                }else
                {
                    this.MosaicImage.CreateSolidMosaic(sourcePixels, this.isBlackAndWhite);
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
                await this.MosaicImage.CreatePictureMosaic(sourcePixels, this.imagePalete, this.useAllImagesOnce);

                this.modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = this.modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    this.AlterImageDisplay = this.modifiedImage;
                }
            }

            this.changeMosaicType(false, false, true);
            this.CanSave = true;
        }

        /// <summary>
        /// Loads all images into image palette.
        /// </summary>
        public void LoadAllImagesIntoImagePalette()
        {
            this.SelectedFolderImages = new List<WriteableBitmap>();
            foreach (var images in this.loadedFolder)
            {
                this.imagePalete.Add(images);
                this.SelectedFolderImages.Add(images.ImageBitmap);
            }

            this.loadedFolder.Clear();
        }

        /// <summary>
        /// Loads all folder images.
        /// </summary>
        /// <param name="selectedFolder">The selected folder.</param>
        /// <returns>A task</returns>
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

        

        private void changeMosaicType(bool isSolidMosaic, bool isTriangleMosaic, bool isPictureMosaic)
        {
            this.hasSolidMosaic = isSolidMosaic;
            this.hasTriangleMosaic = isTriangleMosaic;
            this.hasPictureMosaic = isPictureMosaic;
            this.CreateSolidMosaic.OnCanExecuteChanged();
            this.TriangleMosaic.OnCanExecuteChanged();
            this.CheckToEnablePictureMosaic();
        }
        private void changeGridType(bool isGrid, bool isTriangleGrid)
        {
            this.HasGrid = isGrid;
            this.hasTriangleGrid = isTriangleGrid;
            
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
                    if (this.AlterImageDisplay != null)
                    {
                        this.AlterImageDisplay = null;
                    }
                    var sourcePixels = pixelData.DetachPixelData();
                    this.MosaicImage = new MosaicImage(imageFile, this.blockSizeNumber, decoder.PixelHeight, decoder.PixelWidth);
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
                this.blockSizeNumber != 0 && this.orignalImage != null && this.checkForImagePalette() & (!this.hasPictureMosaic || this.hasBlockSizeChanged);
        }

        private bool checkForImagePalette()
        {
            return this.loadedFolder.Any() || this.imagePalete.Any();
        }

        private async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
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
        /// <returns>A task.</returns>
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
                                ImagePixel.SetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor,
                                    imageWidth, imageHeight);
                            }
                            else if (this.HasTriangleGrid && lineX == lineY)
                            {
                                pixelColor = Colors.White;
                                ImagePixel.SetPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor,
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
        /// <summary>
        /// Determines what happens when Black and White check box is pressed
        /// </summary>
        /// <returns>A task.</returns>
        public async Task BlackAndWhiteCheckboxChanged()
        {
            if (this.MosaicImage != null & this.orignalImage != null)
            {
                this.ImageDisplay = this.orignalImage;
                await this.handleCreatingSolidMosaicImage();
            }
            else if(this.orignalImage != null)
            {
                this.ImageDisplay = this.orignalImage;
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