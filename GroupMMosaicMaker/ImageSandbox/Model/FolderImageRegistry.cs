﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    internal class FolderImageRegistry : IList<FolderImage>
    {
        #region Data members

        private readonly IList<FolderImage> selectedFolderImages;

        #endregion

        #region Properties

        public FolderImage this[int index]
        {
            get => this.selectedFolderImages[index];
            set => this.selectedFolderImages[index] = value;
        }

        public int Count => this.selectedFolderImages.Count;

        public bool IsReadOnly => this.selectedFolderImages.IsReadOnly;

        #endregion

        #region Constructors

        public FolderImageRegistry()
        {
            this.selectedFolderImages = new List<FolderImage>();
        }

        #endregion

        #region Methods

        public void Add(FolderImage item)
        {
            this.selectedFolderImages.Add(item);
        }

        public void Clear()
        {
            this.selectedFolderImages.Clear();
        }

        public bool Contains(FolderImage item)
        {
            return this.selectedFolderImages.Contains(item);
        }

        public void CopyTo(FolderImage[] array, int arrayIndex)
        {
            this.selectedFolderImages.CopyTo(array, arrayIndex);
        }

        public IEnumerator<FolderImage> GetEnumerator()
        {
            return this.selectedFolderImages.GetEnumerator();
        }

        public int IndexOf(FolderImage item)
        {
            return this.selectedFolderImages.IndexOf(item);
        }

        public void Insert(int index, FolderImage item)
        {
            this.selectedFolderImages.Insert(index, item);
        }

        public bool Remove(FolderImage item)
        {
            return this.selectedFolderImages.Remove(item);
        }

        public void RemoveAt(int index)
        {
            this.selectedFolderImages.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.selectedFolderImages.GetEnumerator();
        }

        /// <summary>
        ///     Finds the closest matching image.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The folder image that has the closest color to the selected color</returns>
        public FolderImage FindClosestMatchingImage(Color color)
        {
            if (this.selectedFolderImages == null)
            {
                throw new ArgumentException("List cannot be null");
            }

            FolderImage matchingImage = null;
            var closestValue = 1000;
            foreach (var currentImage in this.selectedFolderImages)
            {
                var matchingValue = ColorDifference.GetColorDifference(color, currentImage.FindAverageColor());

                if (matchingValue == 0)
                {
                    return currentImage;
                }

                if (Math.Abs(matchingValue) < closestValue)
                {
                    closestValue = matchingValue;
                    matchingImage = currentImage;
                }
            }

            return matchingImage;
        }

        /// <summary>
        ///     Resizes all images in folder.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">List cannot be null</exception>
        public async Task ResizeAllImagesInFolder(uint width, uint height)
        {
            if (this.selectedFolderImages == null)
            {
                throw new ArgumentException("List cannot be null");
            }

            foreach (var currentImage in this.selectedFolderImages)
            {
                await currentImage.ResizeWritableBitmap(width, height);
            }
        }

        #endregion
    }
}