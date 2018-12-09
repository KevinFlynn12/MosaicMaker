using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Model;
using ImageSandbox.Util;

namespace ImageSandbox.Datatier
{
    public class ImageFolderReader
    {
        #region Methods

        /// <summary>
        ///     Loads the selected folder.
        /// </summary>
        /// <param name="selectedFolder">The selected folder.</param>
        /// <returns>The images in the folder</returns>
        public async Task<IList<FolderImage>> LoadSelectedFolder(StorageFolder selectedFolder)
        {
            var storedFolder = await selectedFolder.GetFilesAsync();

            return await this.loadImagesFromFolder(storedFolder);
        }

        private async Task<IList<FolderImage>> loadImagesFromFolder(
            IReadOnlyList<StorageFile> storedFolder)
        {
            var loadedImage = new List<FolderImage>();

            foreach (var currentFile in storedFolder)
            {
                try
                {
                    var copyBitmapImage = await BitmapCopy.MakeACopyOfTheFileToWorkOn(currentFile);

                    using (var fileStream = await currentFile.OpenAsync(FileAccessMode.Read))

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

                        var fileWriteableBitmap =
                            new WriteableBitmap((int) decoder.PixelWidth, (int) decoder.PixelHeight);

                        using (var writeStream = fileWriteableBitmap.PixelBuffer.AsStream())
                        {
                            await writeStream.WriteAsync(sourcePixels, 0, sourcePixels.Length);

                            var selectedFolderImage = new FolderImage(fileWriteableBitmap, currentFile);

                            loadedImage.Add(selectedFolderImage);
                        }
                    }
                }
                catch (Exception)
                {
                    var folderLoadDialog = new ContentDialog {Title = "The folder was loaded", CloseButtonText = "OK"};
                    await folderLoadDialog.ShowAsync();
                }
            }

            return loadedImage;
        }

        #endregion
    }
}