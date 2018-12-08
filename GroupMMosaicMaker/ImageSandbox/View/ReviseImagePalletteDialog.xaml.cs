using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using ImageSandbox.Annotations;
using ImageSandbox.Model;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageSandbox.View
{
    public sealed partial class ReviseImagePalletteDialog : ContentDialog, INotifyPropertyChanged
    {
        private List<IAsyncOperation<StorageItemThumbnail>> thumbnails;
        private int imageCount;
        public int ImageCount { 
            get=>this.imageCount;
            set
            {
                this.imageCount = value;
                this.OnPropertyChanged();
            }
        }

        public List<IAsyncOperation<StorageItemThumbnail>> Images
        {
            get => this.thumbnails;
            set
            {
                this.thumbnails = value;
                this.OnPropertyChanged();
            }
        }

        public ReviseImagePalletteDialog()
        {
            this.thumbnails = new List<IAsyncOperation<StorageItemThumbnail>>();
            this.InitializeComponent();

        }

        /// <summary>
        /// Generates the images.
        /// </summary>
        /// <param name="imageFolder">The image folder.</param>
        public void GenerateImages(ImagePalette imageFolder)
        {
            foreach (var folderImage in imageFolder)
            {
                thumbnails.Add(folderImage.ThumbNail);
            }

            this.ImagePaletteView.ItemsSource = this.thumbnails;
            this.ImageCount = thumbnails.Count;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
