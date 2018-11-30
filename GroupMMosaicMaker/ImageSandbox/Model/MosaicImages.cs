using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace ImageSandbox.Model
{
    class MosaicImages : IList<BitmapImage>
    {
        private IList<BitmapImage>  SelectedFolderImages;

        public MosaicImages()
        {
            this.SelectedFolderImages = new List<BitmapImage>();
        }

        public BitmapImage this[int index] { get => SelectedFolderImages[index]; set => SelectedFolderImages[index] = value; }

        public int Count => SelectedFolderImages.Count;

        public bool IsReadOnly => SelectedFolderImages.IsReadOnly;

        public void Add(BitmapImage item)
        {
            SelectedFolderImages.Add(item);
        }

        public void Clear()
        {
            SelectedFolderImages.Clear();
        }

        public bool Contains(BitmapImage item)
        {
            return SelectedFolderImages.Contains(item);
        }

        public void CopyTo(BitmapImage[] array, int arrayIndex)
        {
            SelectedFolderImages.CopyTo(array, arrayIndex);
        }

        public IEnumerator<BitmapImage> GetEnumerator()
        {
            return SelectedFolderImages.GetEnumerator();
        }

        public int IndexOf(BitmapImage item)
        {
            return SelectedFolderImages.IndexOf(item);
        }

        public void Insert(int index, BitmapImage item)
        {
            SelectedFolderImages.Insert(index, item);
        }

        public bool Remove(BitmapImage item)
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
    }
}
