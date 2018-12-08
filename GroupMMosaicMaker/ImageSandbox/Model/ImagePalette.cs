using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI;
using ImageSandbox.Util;

namespace ImageSandbox.Model
{
    public class ImagePalette : IList<FolderImage>
    {
        #region Data members

        private readonly IList<FolderImage> selectedFolderImages;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FolderImage"/> at the specified index.
        /// </summary>
        /// <value>
        /// The <see cref="FolderImage"/>.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The image at the given index</returns>
        public FolderImage this[int index]
        {
            get => this.selectedFolderImages[index];
            set => this.selectedFolderImages[index] = value;
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public int Count => this.selectedFolderImages.Count;

        /// <summary>
        /// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1" /> is read-only.
        /// </summary>
        public bool IsReadOnly => this.selectedFolderImages.IsReadOnly;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePalette"/> class.
        /// </summary>
        public ImagePalette()
        {
            this.selectedFolderImages = new List<FolderImage>();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        public void Add(FolderImage item)
        {
            this.selectedFolderImages.Add(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        public void Clear()
        {
            this.selectedFolderImages.Clear();
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1" /> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> is found in the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false.
        /// </returns>
        public bool Contains(FolderImage item)
        {
            return this.selectedFolderImages.Contains(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1" /> to an <see cref="T:System.Array" />, starting at a particular <see cref="T:System.Array" /> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array" /> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1" />. The <see cref="T:System.Array" /> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in <paramref name="array" /> at which copying begins.</param>
        public void CopyTo(FolderImage[] array, int arrayIndex)
        {
            this.selectedFolderImages.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<FolderImage> GetEnumerator()
        {
            return this.selectedFolderImages.GetEnumerator();
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1" />.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        /// <returns>
        /// The index of <paramref name="item" /> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(FolderImage item)
        {
            return this.selectedFolderImages.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1" /> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item" /> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1" />.</param>
        public void Insert(int index, FolderImage item)
        {
            this.selectedFolderImages.Insert(index, item);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1" />.</param>
        /// <returns>
        /// true if <paramref name="item" /> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1" />; otherwise, false. This method also returns false if <paramref name="item" /> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1" />.
        /// </returns>
        public bool Remove(FolderImage item)
        {
            return this.selectedFolderImages.Remove(item);
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1" /> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        public void RemoveAt(int index)
        {
            this.selectedFolderImages.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.selectedFolderImages.GetEnumerator();
        }

        public async Task ResizeAllImages(int blockSize)
        {
            foreach (var currImage in this.selectedFolderImages)
            {
                await currImage.ResizeWritableBitmap(blockSize);
            }
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
                if (ColorsAreADirectMatch(color, currentImage))
                {
                    return currentImage;
                }

                var matchingValue = ColorDifference.GetColorDifference(currentImage.FindAverageColor(), color);

                if (matchingValue == 0)
                {
                    closestValue = matchingValue;
                    matchingImage = currentImage;
                }

                if (Math.Abs(matchingValue) < closestValue && closestValue != 0)
                {
                    closestValue = matchingValue;
                    matchingImage = currentImage;
                }
            }

            return matchingImage;
        }

        private static bool ColorsAreADirectMatch(Color color, FolderImage currentImage)
        {
            return currentImage.FindAverageColor().R == color.R && currentImage.FindAverageColor().B == color.B &&
                   currentImage.FindAverageColor().G == color.G;
        }

        #endregion
    }
}