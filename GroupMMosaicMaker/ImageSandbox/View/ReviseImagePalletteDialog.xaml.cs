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
        private List<WriteableBitmap> images;
        private int imageCount;
        public int ImageCount { 
            get=>this.imageCount;
            set
            {
                this.imageCount = value;
                this.OnPropertyChanged();
            }
        }

        public List<WriteableBitmap> Images
        {
            get => this.images;
            set
            {
                this.images = value;
                this.OnPropertyChanged();
            }
        }

        public ReviseImagePalletteDialog()
        {
            this.InitializeComponent();

        }

        /// <summary>
        /// Generates the images.
        /// </summary>
        /// <param name="imageFolder">The image folder.</param>
        public void GenerateImages(FolderImageRegistry imageFolder)
        {
            foreach (var folderImage in imageFolder)
            {
                images.Add(folderImage.ImageBitmap);
            }

            this.ImageCount = images.Count;
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
