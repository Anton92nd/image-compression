using System;
using System.Windows;
using System.Windows.Media.Imaging;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace ImageCompression
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Left_Open_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadImage();
            if (image != null)
            {
                LeftImage.Source = leftImage = image;
            }
        }

        [CanBeNull]
        private static BitmapImage LoadImage()
        {
            var openFile = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image Files (*png, *.bmp, *.tiff, *.jpeg)|*.png;*.bmp;*.tiff;*.jpeg"
            };
            var result = openFile.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return new BitmapImage(new Uri(openFile.FileName));
            }
            return null;
        }

        private void MenuItem_Left_Save_OnClick(object sender, RoutedEventArgs e)
        {
            leftImage.Save();
        }

        private void MenuItem_Right_Open_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadImage();
            if (image != null)
            {
                RightImage.Source = rightImage = image;
            }
        }

        private void MenuItem_Right_Save_OnClick(object sender, RoutedEventArgs e)
        {
            rightImage.Save();
        }

        private BitmapImage leftImage, rightImage;
    }
}
