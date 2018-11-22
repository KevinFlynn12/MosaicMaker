using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ImageSandbox.View
{
    public sealed partial class MosaicInformationDialog : ContentDialog
    {
        public int PixelArea { get; private set; }

        public bool OutlineChecked { get; private set; }

        public MosaicInformationDialog()
        {
            this.OutlineChecked = false;
            this.InitializeComponent();
            this.PixelAreaOf5.IsChecked = true;
            this.PixelArea = 0;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (this.OutlineCheckBox.IsPressed)
            {
                this.OutlineChecked = true;
            }

            if (this.PixelAreaOf5.IsPressed)
            {
                this.PixelArea = 5;
            }
            else if (this.PixelAreaOf15.IsPressed)
            {
                this.PixelArea = 15;
            }
            else if (this.PixelAreaOf55.IsPressed)
            {
                this.PixelArea = 55;
            }
            else if (this.PixelAreaOf150.IsPressed)
            {
                this.PixelArea = 150;
            }


        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (this.OutlineCheckBox.IsPressed)
            {
                this.OutlineChecked = true;
            }




        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (this.PixelAreaOf5.IsPressed)
            {
                this.PixelArea = 5;
            }
            else if (this.PixelAreaOf15.IsPressed)
            {
                this.PixelArea = 15;
            }
            else if (this.PixelAreaOf55.IsPressed)
            {
                this.PixelArea = 55;
            }
            else if (this.PixelAreaOf150.IsPressed)
            {
                this.PixelArea = 150;
            }


        }
    }
}
