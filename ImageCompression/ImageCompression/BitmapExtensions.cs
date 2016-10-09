using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using ImageCompression.Exceptions;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace ImageCompression
{
    public static class BitmapExtensions
    {
        public static void Save([CanBeNull] this BitmapImage image)
        {
            if (image == null)
            {
                MessageBox.Show("No picture to save", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                var saveFile = new SaveFileDialog
                {
                    AddExtension = true,
                    Filter = "Image Files (*png, *.bmp, *.jpg)|*.png;*.bmp;*.jpg",
                    DefaultExt = "png",
                };
                var result = saveFile.ShowDialog();
                if (result.HasValue && result.Value)
                {
                    var extension = GetExtension(saveFile.SafeFileName).ToLowerInvariant();
                    if (extension.Equals("png") || extension.Equals("bmp"))
                        image.SavePng(saveFile.FileName);
                    else if (extension.Equals("jpg") || extension.Equals("jpeg"))
                        image.SaveJpeg(saveFile.FileName);
                    else
                        throw new ExtensionNotSupportedException(string.Format("Extension '{0}' is not supported",
                            extension));
                }
            }
            catch (ExtensionNotSupportedException e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [NotNull]
        private static string GetExtension([NotNull] string fileName)
        {
            return fileName.Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries).Last();
        }

        private static void SaveJpeg([NotNull] this BitmapImage image, [NotNull] string filePath)
        {
            SaveWithEncoder(image, new JpegBitmapEncoder(), filePath);
        }

        private static void SavePng([NotNull] this BitmapImage image, [NotNull] string filePath)
        {
            SaveWithEncoder(image, new PngBitmapEncoder(), filePath);
        }

        private static void SaveWithEncoder([NotNull] BitmapImage image, [NotNull] BitmapEncoder encoder, [NotNull] string filePath)
        {
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }
    }
}