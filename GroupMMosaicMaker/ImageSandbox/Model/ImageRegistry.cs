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
    class ImageRegistry : IList<WriteableBitmap>
    {
        private IList<WriteableBitmap>  SelectedFolderImages;

        public ImageRegistry()
        {
            this.SelectedFolderImages = new List<WriteableBitmap>();
        }

        public WriteableBitmap this[int index] { get => SelectedFolderImages[index]; set => SelectedFolderImages[index] = value; }

        public int Count => SelectedFolderImages.Count;

        public bool IsReadOnly => SelectedFolderImages.IsReadOnly;

        public void Add(WriteableBitmap item)
        {
            SelectedFolderImages.Add(item);
        }

        public void Clear()
        {
            SelectedFolderImages.Clear();
        }

        public bool Contains(WriteableBitmap item)
        {
            return SelectedFolderImages.Contains(item);
        }

        public void CopyTo(WriteableBitmap[] array, int arrayIndex)
        {
            SelectedFolderImages.CopyTo(array, arrayIndex);
        }

        public IEnumerator<WriteableBitmap> GetEnumerator()
        {
            return SelectedFolderImages.GetEnumerator();
        }

        public int IndexOf(WriteableBitmap item)
        {
            return SelectedFolderImages.IndexOf(item);
        }

        public void Insert(int index, WriteableBitmap item)
        {
            SelectedFolderImages.Insert(index, item);
        }

        public bool Remove(WriteableBitmap item)
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
