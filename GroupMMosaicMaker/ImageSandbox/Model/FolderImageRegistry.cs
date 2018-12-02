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
    }
}
