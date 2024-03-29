﻿using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Util
{
    public static class BitmapCopy
    {
        #region Methods

        /// <summary>
        ///     Makes a copy of the file to work on.
        /// </summary>
        /// <param name="imageFile">The image file.</param>
        /// <returns>Bitmap Image</returns>
        public static async Task<BitmapImage> MakeACopyOfTheFileToWorkOn(StorageFile imageFile)
        {
            IRandomAccessStream inputStream = await imageFile.OpenReadAsync();
            var newImage = new BitmapImage();
            newImage.SetSource(inputStream);
            return newImage;
        }

        #endregion
    }
}