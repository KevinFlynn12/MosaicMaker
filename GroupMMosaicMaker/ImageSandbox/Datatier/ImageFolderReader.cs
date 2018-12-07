using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Model;
using ImageSandbox.Util;

namespace ImageSandbox.Datatier
{
    public class ImageFolderReader
    {
        public async Task<ICollection<FolderImage>> LoadSelectedFolder(StorageFolder selectedFolder)
        {
            var loadedImage = new List<FolderImage>();

            var storedFolder = await selectedFolder.GetFilesAsync();

            return await loadImagesFromFolder(storedFolder, loadedImage);
        }

        private static async Task<ICollection<FolderImage>> loadImagesFromFolder(
            IReadOnlyList<StorageFile> storedFolder, List<FolderImage> loadedImage)
        {
            foreach (var currentFile in storedFolder)
                try
                {
                    var copyBitmapImage = await BitmapCopy.MakeACopyOfTheFileToWorkOn(currentFile);

                    using (var fileStream = await currentFile.OpenAsync(FileAccessMode.Read))

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


                        var thumbnail = currentFile.GetThumbnailAsync(ThumbnailMode.PicturesView, 5);

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
                }

            return loadedImage;
        }
    }
}