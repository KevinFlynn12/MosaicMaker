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
using ImageSandbox.View;

namespace ImageSandbox.ViewModel
{
    internal class MosaicMakerPageViewModel : INotifyPropertyChanged
    {
        #region Constructors

        public MosaicMakerPageViewModel()
        {
            HasGrid = false;
            dpiX = 0;
            dpiY = 0;
            loadAllCommands();
            SelectedFolderImages = new List<WriteableBitmap>();
            folderReader = new ImageFolderReader();
            loadedFolder = new List<FolderImage>();
            imagePalete = new ImagePalette();
            NumberOfImages = "" + 0;
        }

        #endregion

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

        public List<WriteableBitmap> SelectedFolderImages
        {
            get => selectedFolderImages;
            set
            {
                selectedFolderImages = value;
                ViewPalette.OnCanExecuteChanged();
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
        public string LoadedFileType { get; private set; }

        public bool IsCreatePictureMosaicEnabled
        {
            get => isCreatePictureMosaicEnabled;
            set
            {
                isCreatePictureMosaicEnabled = value;
                OnPropertyChanged();
                CreateSolidMosaic.OnCanExecuteChanged();
            }
        }

        public bool IsGridCheckEnabled
        {
            get => isGridCheckEnabled;
            set
            {
                isGridCheckEnabled = value;
                OnPropertyChanged();
                ChangeBlockSize.OnCanExecuteChanged();
            }
        }

        public bool CanSave
        {
            get => canSave;
            set
            {
                canSave = value;
                OnPropertyChanged();
                CreateSolidMosaic.OnCanExecuteChanged();
            }
        }

        public bool HasMosaic
        {
            get => hasMosaic;
            set
            {
                hasMosaic = value;
                OnPropertyChanged();
            }
        }

        public bool HasGrid
        {
            get => hasGrid;
            set
            {
                hasGrid = value;
                OnPropertyChanged();
            }
        }

        public bool IsBlackAndWhite
        {
            get => isBlackAndWhite;
            set
            {
                isBlackAndWhite = value;
                OnPropertyChanged();
            }
        }

        public WriteableBitmap ImageDisplay
        {
            get => imageDisplay;
            set
            {
                imageDisplay = value;
                OnPropertyChanged();
                CreateSolidMosaic.OnCanExecuteChanged();
                TriangleGridChecked.OnCanExecuteChanged();
                GridChecked.OnCanExecuteChanged();
            }
        }

        public WriteableBitmap AlterImageDisplay
        {
            get => alterImageDisplay;
            set
            {
                alterImageDisplay = value;
                OnPropertyChanged();
            }
        }

        public string BlockSize
        {
            get => blockSize;
            set
            {
                blockSize = value;
                OnPropertyChanged();
                ChangeBlockSize.OnCanExecuteChanged();
            }
        }

        private string numberOfImages;

        public string NumberOfImages
        {
            get => numberOfImages;
            set
            {
                numberOfImages = value;
                OnPropertyChanged();
            }
        }

        public MosaicImage MosaicImage
        {
            get => mosaicImage;
            set
            {
                mosaicImage = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Methods

        public event PropertyChangedEventHandler PropertyChanged;

        private void loadAllCommands()
        {
            CreateSolidMosaic = new RelayCommand(createSolidMosaic, canSolidMosaic);
            ChangeBlockSize = new RelayCommand(changeBlockSize, canChangeBlockSize);
            ViewPalette = new RelayCommand(viewPalette, canViewPalette);
            TriangleGridChecked = new RelayCommand(createTriangleGrid, canCreateGrid);
            GridChecked = new RelayCommand(createGrid, canCreateGrid);
            NoGridChecked = new RelayCommand(createNoGrid, canAlwaysExecute);
        }

        private bool canCreateGrid(object obj)
        {
            return (selectedImageFile != null) & (blockSizeNumber >= 5) & (blockSizeNumber <= 50);
        }

        private async void createTriangleGrid(object obj)
        {
            HasGrid = true;
            await creatingOutlineOrignalImage(true);
        }

        private async void createGrid(object obj)
        {
            HasGrid = true;
            await creatingOutlineOrignalImage(false);
        }

        private void createNoGrid(object obj)
        {
            HasGrid = false;
            ImageDisplay = orignalImage;
        }

        private bool canViewPalette(object obj)
        {
            return selectedFolderImages.Count != 0;
        }

        private async void viewPalette(object obj)
        {
            var contentDialog = new ReviseImagePalletteDialog();
            await contentDialog.ShowAsync();
        }

        private bool canChangeBlockSize(object obj)
        {
            int parsedBlockSize;
            int.TryParse(BlockSize, out parsedBlockSize);
            return parsedBlockSize >= 5 && parsedBlockSize <= 50;
        }

        private void changeBlockSize(object obj)
        {
            blockSizeNumber = int.Parse(BlockSize);
            CheckToEnablePictureMosaic();
            IsGridCheckEnabled = true;
            CreateSolidMosaic.OnCanExecuteChanged();
            TriangleGridChecked.OnCanExecuteChanged();
            GridChecked.OnCanExecuteChanged();
        }

        private bool canSolidMosaic(object obj)
        {
            return (selectedImageFile != null) & (blockSizeNumber >= 5) & (blockSizeNumber <= 50);
        }

        private async void createSolidMosaic(object obj)
        {
            await handleCreatingSolidMosaicImage();
            if (HasGrid) await creatingOutlineOrignalImage(false);

            hasMosaic = true;
            CanSave = true;
        }

        private async Task handleCreatingSolidMosaicImage()
        {
            var copyBitmapImage = await MakeACopyOfTheFileToWorkOn(selectedImageFile);

            using (var fileStream = await selectedImageFile.OpenAsync(FileAccessMode.Read))

            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                dpiX = decoder.DpiX;
                dpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();
                if (IsBlackAndWhite)
                    MosaicImage.CreateBlackAndWhiteMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight,
                        blockSizeNumber, HasGrid);
                else
                    MosaicImage.CreateSolidMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight,
                        blockSizeNumber);

                modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    AlterImageDisplay = modifiedImage;
                }
            }
        }

        /// <summary>
        ///     Displays the picture mosaic.
        /// </summary>
        /// <param name="selectedFolder">The selected folder.</param>
        public async void DisplayPictureMosaic(StorageFolder selectedFolder)
        {
            await imagePalete.ResizeAllImages(blockSizeNumber);

            var copyBitmapImage = await MakeACopyOfTheFileToWorkOn(selectedImageFile);

            using (var fileStream = await selectedImageFile.OpenAsync(FileAccessMode.Read))

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

                if (IsBlackAndWhite)
                    MosaicImage.CreateBlackAndWhiteMosaic(sourcePixels, decoder.PixelWidth, decoder.PixelHeight,
                        blockSizeNumber, HasGrid);
                else
                    MosaicImage.CreatePictureMosaic(sourcePixels, decoder.PixelWidth,
                        decoder.PixelHeight, blockSizeNumber, imagePalete);

                modifiedImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = modifiedImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    AlterImageDisplay = modifiedImage;
                }
            }

            hasMosaic = true;
            CanSave = true;
        }


        public void LoadAllImagesIntoImagePalette()
        {
            foreach (var currImage in loadedFolder) imagePalete.Add(currImage);
            loadedFolder.Clear();
        }

        public async Task LoadAllFolderImages(StorageFolder selectedFolder)
        {
            loadedFolder = (List<FolderImage>) await folderReader.LoadSelectedFolder(selectedFolder);
            CheckToEnablePictureMosaic();
            LoadAllImagesIntoImagePalette();
            this.UpdateImagePaletteCount();
        }

        private void UpdateImagePaletteCount()
        {
            NumberOfImages = "" + imagePalete.Count;
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
            selectedImageFile = imageFile;

            LoadedFileType = imageFile.FileType;

            if (selectedImageFile != null)
            {
                MosaicImage = new MosaicImage(HasGrid, imageFile);

                var copyBitmapImage = await MakeACopyOfTheFileToWorkOn(selectedImageFile);

                using (var fileStream = await selectedImageFile.OpenAsync(FileAccessMode.Read))
                {
                    var decoder = await BitmapDecoder.CreateAsync(fileStream);

                    var transform = new BitmapTransform
                    {
                        ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                        ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                    };

                    dpiX = decoder.DpiX;
                    dpiY = decoder.DpiY;

                    var pixelData = await decoder.GetPixelDataAsync(
                        BitmapPixelFormat.Bgra8,
                        BitmapAlphaMode.Straight,
                        transform,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.DoNotColorManage
                    );

                    var sourcePixels = pixelData.DetachPixelData();
                    await createOriginalImage(decoder, sourcePixels);
                    if (HasGrid) await creatingOutlineOrignalImage(false);
                }
            }
        }


        private void CheckToEnablePictureMosaic()
        {
            IsCreatePictureMosaicEnabled = blockSizeNumber != 0 && orignalImage != null && checkForImagePalette();
        }


        private bool checkForImagePalette()
        {
            return loadedFolder.Any() || imagePalete.Any();
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
            orignalImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
            using (var writeStream = orignalImage.PixelBuffer.AsStream())
            {
                await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);

                ImageDisplay = orignalImage;
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

                var pixelStream = modifiedImage.PixelBuffer.AsStream();
                var pixels = new byte[pixelStream.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) modifiedImage.PixelWidth,
                    (uint) modifiedImage.PixelHeight, dpiX, dpiY, pixels);
                await encoder.FlushAsync();

                stream.Dispose();
            }
        }




        private void createOrignalImageWithOutline(byte[] sourcePixels, uint imageWidth, uint imageHeight,
            bool isTriangle)
        {
            var startingYpoint = 0;
            while (startingYpoint < imageHeight)
            {
                var startingXpoint = 0;
                while (startingXpoint < imageWidth)
                {
                    var XStoppingPoint = UpdateStoppingPoint(imageWidth, startingXpoint);

                    var YStoppingPoint = UpdateStoppingPoint(imageHeight, startingYpoint);
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
                            else if (isTriangle && lineX == lineY)
                            {
                                pixelColor = Colors.White;
                                ImagePixel.setPixelBgra8(sourcePixels, currentYPoint, currentXPoint, pixelColor,
                                    imageWidth, imageHeight);
                            }


                            lineX++;
                        }

                        lineY++;
                    }

                    startingXpoint += blockSizeNumber;
                }

                startingYpoint += blockSizeNumber;
            }








            var triangleCoordinates =
                MosaicImage.FindTrianglePoints(imageWidth, imageHeight, blockSizeNumber);
            if (isTriangle)

                foreach (var currentPoint in triangleCoordinates)
                {
                    var pixelColor = ImagePixel.GetPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1,
                        imageWidth, imageHeight);
                    pixelColor = Colors.White;
                    ImagePixel.setPixelBgra8(sourcePixels, currentPoint.Item2, currentPoint.Item1, pixelColor,
                        imageWidth, imageHeight);
                }
        }

        private int UpdateStoppingPoint(uint maxValue, int coordinate)
        {
            var CoordinateStoppingPoint = coordinate + blockSizeNumber;
            if (CoordinateStoppingPoint > maxValue) CoordinateStoppingPoint = (int) maxValue;

            return CoordinateStoppingPoint;
        }

        private async Task creatingOutlineOrignalImage(bool isTriangle)
        {
            var copyBitmapImage = await MakeACopyOfTheFileToWorkOn(selectedImageFile);

            using (var fileStream = await selectedImageFile.OpenAsync(FileAccessMode.Read))
            {
                var decoder = await BitmapDecoder.CreateAsync(fileStream);

                var transform = new BitmapTransform
                {
                    ScaledWidth = Convert.ToUInt32(copyBitmapImage.PixelWidth),
                    ScaledHeight = Convert.ToUInt32(copyBitmapImage.PixelHeight)
                };

                dpiX = decoder.DpiX;
                dpiY = decoder.DpiY;

                var pixelData = await decoder.GetPixelDataAsync(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Straight,
                    transform,
                    ExifOrientationMode.IgnoreExifOrientation,
                    ColorManagementMode.DoNotColorManage
                );

                var sourcePixels = pixelData.DetachPixelData();

                createOrignalImageWithOutline(sourcePixels, decoder.PixelWidth, decoder.PixelHeight, isTriangle);

                outlineOrignalImage = new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);
                using (var writeStream = outlineOrignalImage.PixelBuffer.AsStream())
                {
                    await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);
                    ImageDisplay = outlineOrignalImage;
                }
            }
        }


        public async Task BlackAndWhiteCheckboxChanged()
        {
            if (orignalImage != null)
            {
                if (IsBlackAndWhite & HasMosaic)
                {
                    await handleCreatingSolidMosaicImage();
                }
                else
                {
                    if (HasMosaic)
                    {
                        ImageDisplay = orignalImage;
                        await handleCreatingSolidMosaicImage();
                    }
                    else
                    {
                        ImageDisplay = orignalImage;
                    }
                }
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}