using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using ImageCompression.Extensions;
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

        [CanBeNull]
        private static BitmapImage LoadImage()
        {
            var openFile = new OpenFileDialog
            {
                Multiselect = false,
                Filter = "Image Files (*png, *.bmp, *.tiff, *.jpg, *jpeg)|*.png;*.bmp;*.tiff;*.jpeg;*.jpg"
            };
            var result = openFile.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return new BitmapImage(new Uri(openFile.FileName));
            }
            return null;
        }

        private void MenuItem_Left_Open_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadImage();
            if (image != null)
            {
                LeftImageBox.Source = leftImage = image;
                ButtonApplyLeft.IsEnabled = true;
            }
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
                RightImageBox.Source = rightImage = image;
                ButtonApplyRight.IsEnabled = true;
            }
        }

        private void MenuItem_Right_Save_OnClick(object sender, RoutedEventArgs e)
        {
            rightImage.Save();
        }

        private BitmapSource leftImage;
        private BitmapSource rightImage;

        private void ComboBoxEffects_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBoxEffects.ItemsSource = typeof(EffectType).GetEnumValues<EffectType>().Select(el => el.GetText());
            ComboBoxEffects.SelectedIndex = 0;
        }

        private BitmapSource ApplyEffect(BitmapSource bitmap, EffectType effectType)
        {
            switch (effectType)
            {
                case EffectType.MonochromeBad:
                    return ApplyMonochromeBad(bitmap);
                case EffectType.MonochromeGood:
                    return ApplyMonochromeGood(bitmap);
                default:
                    throw new Exception(string.Format("Unknown effect type: {0}", effectType));
            }
        }

        private BitmapSource ApplyMonochromeGood(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            for (var i = 0; i < bytes.Length; i += 4)
            {
                var middle = (77.0 / 256) * bytes[i] + (150.0 / 256) * bytes[i + 1] + (29.0 / 256) * bytes[i + 2];
                bytes[i] = bytes[i + 1] = bytes[i + 2] = (byte)middle;
            }
            return BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth * 4);
        }

        private BitmapSource ApplyMonochromeBad(BitmapSource bitmap)
        {
            var bytes = bitmap.GetBytes();
            for (var i = 0; i < bytes.Length; i += 4)
            {
                var middle = (bytes[i] + bytes[i + 1] + bytes[i + 2]) / 3;
                bytes[i] = bytes[i + 1] = bytes[i + 2] = (byte)middle;
            }
            return BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth*4);
        }

        private void ButtonApplyLeft_Click(object sender, RoutedEventArgs e)
        {
            LeftImageBox.Source = leftImage = ApplyEffect(leftImage, (EffectType) ComboBoxEffects.SelectedIndex);
        }

        private void ButtonApplyRight_OnClick(object sender, RoutedEventArgs e)
        {
            RightImageBox.Source = rightImage = ApplyEffect(rightImage, (EffectType) ComboBoxEffects.SelectedIndex);
        }
    }
}
