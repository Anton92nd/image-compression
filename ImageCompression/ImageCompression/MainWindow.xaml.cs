﻿using System;
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
                LeftImageBox.Source = image;
                ButtonApplyLeft.IsEnabled = true;
            }
        }

        private void MenuItem_Left_Save_OnClick(object sender, RoutedEventArgs e)
        {
            ((BitmapSource)LeftImageBox.Source).Save();
        }

        private void MenuItem_Right_Open_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadImage();
            if (image != null)
            {
                RightImageBox.Source = image;
                ButtonApplyRight.IsEnabled = true;
            }
        }

        private void MenuItem_Right_Save_OnClick(object sender, RoutedEventArgs e)
        {
            ((BitmapSource)RightImageBox.Source).Save();
        }

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
                var middle = (77 * bytes[i] + 150 * bytes[i + 1] + 29 * bytes[i + 2]) >> 8;
                bytes[i] = bytes[i + 1] = bytes[i + 2] = (byte)middle;
            }
            return BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8));
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
                null, bytes, bitmap.PixelWidth*(bitmap.Format.BitsPerPixel / 8));
        }

        private void ButtonApplyLeft_Click(object sender, RoutedEventArgs e)
        {
            LeftImageBox.Source = ApplyEffect((BitmapSource)LeftImageBox.Source, (EffectType) ComboBoxEffects.SelectedIndex);
        }

        private void ButtonApplyRight_OnClick(object sender, RoutedEventArgs e)
        {
            RightImageBox.Source = ApplyEffect((BitmapSource)RightImageBox.Source, (EffectType) ComboBoxEffects.SelectedIndex);
        }

        private void MenuItem_LeftToRight_OnClick(object sender, RoutedEventArgs e)
        {
            var bitmap = LeftImageBox.Source as BitmapSource;
            if (bitmap == null)
                return;
            var bytes = bitmap.GetBytes();
            RightImageBox.Source = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8));
            ButtonApplyRight.IsEnabled = true;
        }

        private void MenuItem_RightToLeft_OnClick(object sender, RoutedEventArgs e)
        {
            var bitmap = RightImageBox.Source as BitmapSource;
            if (bitmap == null)
                return;
            var bytes = bitmap.GetBytes();
            LeftImageBox.Source = BitmapSource.Create(bitmap.PixelWidth, bitmap.PixelHeight, bitmap.DpiX, bitmap.DpiY, bitmap.Format,
                null, bytes, bitmap.PixelWidth * (bitmap.Format.BitsPerPixel / 8));
            ButtonApplyLeft.IsEnabled = true;
        }
    }
}
