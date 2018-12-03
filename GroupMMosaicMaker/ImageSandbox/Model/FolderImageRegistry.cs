using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    class FolderImageRegistry : IList<FolderImage>
    {
        private IList<FolderImage>  SelectedFolderImages;

        public FolderImageRegistry()
        {
            this.SelectedFolderImages = new List<FolderImage>();
        }

        public FolderImage this[int index] { get => SelectedFolderImages[index]; set => SelectedFolderImages[index] = value; }

        public int Count => SelectedFolderImages.Count;

        public bool IsReadOnly => SelectedFolderImages.IsReadOnly;

        public void Add(FolderImage item)
        {
            SelectedFolderImages.Add(item);
        }

        public void Clear()
        {
            SelectedFolderImages.Clear();
        }

        public bool Contains(FolderImage item)
        {
            return SelectedFolderImages.Contains(item);
        }

        public void CopyTo(FolderImage[] array, int arrayIndex)
        {
            SelectedFolderImages.CopyTo(array, arrayIndex);
        }

        public IEnumerator<FolderImage> GetEnumerator()
        {
            return SelectedFolderImages.GetEnumerator();
        }

        public int IndexOf(FolderImage item)
        {
            return SelectedFolderImages.IndexOf(item);
        }

        public void Insert(int index, FolderImage item)
        {
            SelectedFolderImages.Insert(index, item);
        }

        public bool Remove(FolderImage item)
        {
            return SelectedFolderImages.Remove(item);
        }

        public void RemoveAt(int index)
        {
            SelectedFolderImages.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return SelectedFolderImages.GetEnumerator();
        }


        /// <summary>
        /// Finds the closest matching image.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <returns>The folder image that has the closest color to the selected color</returns>
        public FolderImage FindClosestMatchingImage(Color color)
        {
            if (this.SelectedFolderImages == null)
            {
                throw new ArgumentException("List cannot be null");
            }
            FolderImage matchingImage = null;
            var closestValue = 1000;
            foreach (var currImage in this.SelectedFolderImages)
            {
                var matchingValue = ColorDIfference.GetColorDifference( color, currImage.FindAverageColor());

                if (matchingValue == 0)
                {
                    return currImage;
                }
                else if (Math.Abs(matchingValue) < closestValue)
                {
                    closestValue = matchingValue;
                    matchingImage = currImage;
                }

            }

            return matchingImage;
        }


        /// <summary>
        /// Resizes all images in folder.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">List cannot be null</exception>
        public async Task ResizeAllImagesInFolder(uint width, uint height)
        {
            if (this.SelectedFolderImages == null)
            {
                throw new ArgumentException("List cannot be null");
            }
            
            foreach (var currImage in this.SelectedFolderImages)
            {
               await currImage.ResizeWritableBitmap(width, height);

            }


        }

    }
}
