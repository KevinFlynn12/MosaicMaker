﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageSandbox
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        /// <summary>
        ///     The application height
        /// </summary>
        public const int ApplicationHeight = 1000;

        /// <summary>
        ///     The application width
        /// </summary>
        public const int ApplicationWidth = 1500;

        private StorageFile selectedImageFile;
        private readonly MosaicMakerPageViewModel viewModel;

        private StorageFolder selectedFolder;

        #endregion

        #region Constructors

        public MainPage()
        {
            this.InitializeComponent();
            this.NoGrid.IsChecked = true;
            this.btnPictureMosaic.IsEnabled = false;
            this.selectedFolder = null;
            this.viewModel = new MosaicMakerPageViewModel();
            DataContext = this.viewModel;

            ApplicationView.PreferredLaunchViewSize = new Size {Width = ApplicationWidth, Height = ApplicationHeight};
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(ApplicationWidth, ApplicationHeight));
        }

        #endregion

        #region Methods

        private async Task<StorageFile> selectSourceImageFile()
        {
            var openPicker = new FileOpenPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }

        private async Task<StorageFolder> selectImageFileFolder()
        {
            var folderPicker = new FolderPicker {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            folderPicker.FileTypeFilter.Add("*");
            var selectedFolder = await folderPicker.PickSingleFolderAsync();
            return selectedFolder;
        }

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var fileSavePicker = new FileSavePicker {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };

            var displayFileType = this.viewModel.LoadedFileType.Remove(0, 1);

            fileSavePicker.FileTypeChoices.Add(displayFileType + " files",
                new List<string> {this.viewModel.LoadedFileType});

            this.handleRemainingFileTypes(fileSavePicker);

            var savefile = await fileSavePicker.PickSaveFileAsync();

            await this.viewModel.SavePicture(savefile);
        }

        private void handleRemainingFileTypes(FileSavePicker fileSavePicker)
        {
            var fileTypes = new List<string>
            {
                ".png",
                ".jpg",
                ".jpeg",
                ".bmp"
            };

            foreach (var currType in fileTypes)
            {
                if (!currType.Equals(this.viewModel.LoadedFileType))
                {
                    var displayFileType = currType.Remove(0, 1);

                    fileSavePicker.FileTypeChoices.Add(displayFileType + " files", new List<string> {currType});
                }
            }
        }

        private async void LoadButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.selectedImageFile = await this.selectSourceImageFile();
            if (this.selectedImageFile != null)
            {
                await this.viewModel.LoadPicture(this.selectedImageFile);
            }
        }

        private void blockSizeTextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (!Regex.IsMatch(sender.Text, "^[1-9]?([0-9])*$") && sender.Text != "")
            {
                var pos = sender.SelectionStart - 1;
                sender.Text = sender.Text.Remove(pos, 1);
                sender.SelectionStart = pos;
            }
        }

        private async void BlackAndWhiteCheckbox_OnClick(object sender, RoutedEventArgs e)
        {
            if (this.blackAndWhiteCheckBox.IsChecked != null)
            {
                this.viewModel.IsBlackAndWhite = (bool)this.blackAndWhiteCheckBox.IsChecked;
                await this.viewModel.BlackAndWhiteCheckboxChanged();
            }
           
        }
        



        private void PictureMosaicButton_Click(object sender, RoutedEventArgs e)
        {
            this.viewModel.DisplayPictureMosaic(this.selectedFolder);
        }

        private async void AddImagePalette_OnClick(object sender, RoutedEventArgs e)
        {
            var selectedFolder = await this.selectImageFileFolder();
            if (selectedFolder != null)
            {
                await this.viewModel.LoadAllFolderImages(selectedFolder);

            }
        }

        #endregion

        private async void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            this.selectedImageFile = await this.selectSourceImageFile();
            if (this.selectedImageFile != null)
            {
                await this.viewModel.AddImage(this.selectedImageFile);
                
            }
        }

        private void RemoveSelectedImages_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0 ; i <= this.GridView.SelectedItems.Count; i++)
            {
                var selectedImage = this.GridView.SelectedItems[0];
               this.viewModel.RemoveSelectedItem((WriteableBitmap) selectedImage);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
           this.viewModel.clearImagePallette();
        }
    }
}