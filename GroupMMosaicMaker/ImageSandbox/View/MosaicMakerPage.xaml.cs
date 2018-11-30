﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.Chat;
using Windows.ApplicationModel.Store;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using ImageSandbox.ViewModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ImageSandbox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region Data members

        private StorageFile selectedImageFile;
        private MosaicMakerPageViewModel viewModel;

        #endregion

        #region Constructors

        public MainPage()
        {
            
            this.InitializeComponent();
            this.ModifyMoasicButton.IsEnabled = false;
            this.RefreshMosaicButton.IsEnabled = false;
            this.viewModel = new MosaicMakerPageViewModel();
            this.DataContext = this.viewModel;
        }

        #endregion

      

        private async Task<StorageFile> selectSourceImageFile()
        {
            var openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".png");
            openPicker.FileTypeFilter.Add(".bmp");

            var file = await openPicker.PickSingleFileAsync();

            return file;
        }
        private async Task<StorageFolder> selectImageFileFolder()
        {
            var picker = new FolderPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add("*");
            var selectedFolder = await picker.PickSingleFolderAsync();
            return selectedFolder;
        }

        private async void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            var fileSavePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                SuggestedFileName = "image"
            };
            fileSavePicker.FileTypeChoices.Add("PNG files", new List<string> {".png"});
            var savefile = await fileSavePicker.PickSaveFileAsync();

            await this.viewModel.SavePicture(savefile);
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

        private async void GirdCheckbox_OnClick(object sender, RoutedEventArgs e)
        {
            this.viewModel.HasGrid = (bool)this.gridCheckbox.IsChecked;
            await this.viewModel.GridCheckboxChanged();
        }

        private async void PictureMosaicButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedFolder = await this.selectImageFileFolder();
        }
    }
}
