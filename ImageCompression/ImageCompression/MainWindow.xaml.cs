using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ImageCompression.Algorithms.DiscreteCosineTransform;
using ImageCompression.Algorithms.Wavelets;
using ImageCompression.Extensions;
using ImageCompression.Structures;
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
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
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
                PushLeft(image);
            }
        }

        private void PushLeft(BitmapSource image)
        {
            LeftImageBox.Source = image;
            ButtonApplyLeft.IsEnabled = true;
            leftHistory.Add(image);
            MenuUndoLeft.IsEnabled = leftHistory.Count > 1;
            MenuRedoLeft.IsEnabled = false;
        }

        private void MenuItem_Left_Save_OnClick(object sender, RoutedEventArgs e)
        {
            ((BitmapSource) LeftImageBox.Source).Save();
        }

        private void MenuItem_Right_Open_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadImage();
            if (image != null)
            {
                PushRight(image);
            }
        }

        private void PushRight(BitmapSource image)
        {
            RightImageBox.Source = image;
            ButtonApplyRight.IsEnabled = true;
            rightHistory.Add(image);
            MenuUndoRight.IsEnabled = rightHistory.Count > 1;
            MenuRedoRight.IsEnabled = false;
        }

        private void MenuItem_Right_Save_OnClick(object sender, RoutedEventArgs e)
        {
            ((BitmapSource) RightImageBox.Source).Save();
        }

        private bool ApplyEffect(BitmapSource bitmap, EffectType effectType, out BitmapSource result)
        {
            if (!Effects.CanApply(bitmap, effectType))
            {
                MessageBox.Show(string.Format("This effect is not supported for that image. Image format: {0}", bitmap.Format), "Error", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
                result = null;
                return false;
            }
            if (effectType.GetParameter() != null)
            {
                string errorMessage;
                object parameterValue;
                if (!ValidateParameter(effectType, EffectParameterComboBox.Text, out errorMessage, out parameterValue))
                {
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    result = null;
                    return false;
                }
                result = Effects.EffectByType[effectType](bitmap, parameterValue);
            }
            else if (effectType == EffectType.Dct)
            {
                string errorMessage;
                DctParameters dctParameters;
                if (!ValidateDct(out errorMessage, out dctParameters))
                {
                    
                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    result = null;
                    return false;
                }
                result = Effects.EffectByType[effectType](bitmap, dctParameters);
                return false;
            }
            else if (effectType == EffectType.Wavelet)
            {
                string errorMessage;
                WaveletParameters waveletParameters;
                if (!ValidateWavelet(out errorMessage, out waveletParameters))
                {

                    MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    result = null;
                    return false;
                }
                result = Effects.EffectByType[effectType](bitmap, waveletParameters);
                return true;
            }
            else
            {
                result = Effects.EffectByType[effectType](bitmap, null);
            }
            return true;
        }

        private bool ValidateWavelet(out string errorMessage, out WaveletParameters waveletParameters)
        {
            waveletParameters = null;
            int iterationsCount;
            if (!int.TryParse(WaveletIterationsComboBox.Text, out iterationsCount) || iterationsCount <= 0 ||
                iterationsCount > 7)
            {
                errorMessage = "Iterations count must be integer in [1..7]";
                return false;
            }
            double threshold;
            if (!double.TryParse(WaveletThresholdComboBox.Text, out threshold) || threshold < 0)
            {
                errorMessage = "Threshold must be real not less than 0";
                return false;
            }
            waveletParameters = new WaveletParameters
            {
                WaveletType = (WaveletType) WaveletTypeComboBox.SelectedIndex,
                SavePath = WaveletSavePath.Text,
                IterationsCount = iterationsCount,
                Threshold = threshold,
            };
            errorMessage = null;
            return true;
        }

        private bool ValidateDct(out string errorMessage, out DctParameters dctParameters)
        {
            dctParameters = new DctParameters
            {
                DecimationType = (DecimationType)ComboBox_Decimation.SelectedIndex,
                QuantizationType = (QuantizationType)ComboBox_Quantization.SelectedIndex,
                SavePath = JPG_SavePath.Text,
            };
            switch (dctParameters.QuantizationType)
            {
                case QuantizationType.LargestN:
                    int ny, nc;
                    if (!int.TryParse(JPG_FirstParameter.Text, out ny) || ny < 1 || ny > 64)
                    {
                        errorMessage = "N_Y must be integer in [1, 64]";
                        return false;
                    }
                    if (!int.TryParse(JPG_SecondParameter.Text, out nc) || nc < 1 || nc > 64)
                    {
                        errorMessage = "N_C must be integer in [1, 64]";
                        return false;
                    }
                    dctParameters.Ny = ny;
                    dctParameters.Nc = nc;
                    break;
                case QuantizationType.QuantizationMatrix:
                    int alphaY, alphaC, gammaY, gammaC;
                    if (!int.TryParse(JPG_FirstParameter.Text, out alphaY))
                    {
                        errorMessage = "\u03B1_Y must be integer";
                        return false;
                    }
                    if (!int.TryParse(JPG_SecondParameter.Text, out gammaY))
                    {
                        errorMessage = "\u0263_Y must be integer";
                        return false;
                    }
                    if (!int.TryParse(JPG_ThirdParameter.Text, out alphaC))
                    {
                        errorMessage = "\u03B1_C must be integer";
                        return false;
                    }
                    if (!int.TryParse(JPG_FourthParameter.Text, out gammaC))
                    {
                        errorMessage = "\u0263_C must be integer";
                        return false;
                    }
                    dctParameters.GeneratorsY = new MatrixGenerators
                    {
                        Alpha = alphaY,
                        Gamma = gammaY,
                    };
                    dctParameters.GeneratorsC = new MatrixGenerators
                    {
                        Alpha = alphaC,
                        Gamma = gammaC,
                    };
                    break;
                case QuantizationType.DefaultJpegMatrix:
                    break;
                default:
                    throw new Exception("Invalid program state");
            }
            errorMessage = string.Empty;
            return true;
        }

        private bool ValidateParameter(EffectType effectType, [CanBeNull] string text, out string errorMessage, out object result)
        {
            result = null;
            if (string.IsNullOrEmpty(text))
            {
                errorMessage = "Parameter value is empty";
                return false;
            }
            switch (effectType)
            {
                case EffectType.MedianCut:
                case EffectType.LindeBuzoGray:
                    errorMessage = "Parameter must be positive degree of 2";
                    int res;
                    if (int.TryParse(text, out res) && (res & (res - 1)) == 0)
                    {
                        result = res;
                        return true;
                    }
                    return false;
                case EffectType.QuantizationRgb:
                case EffectType.QuantizationYCrCb:
                    errorMessage = "Parameter must be in format 'BxBxB' where 1 <= B <= 8";
                    var bits = text.Split(new[] {'x'}, StringSplitOptions.RemoveEmptyEntries);
                    if (bits.Length != 3 || bits.Any(IsInvalid))
                        return false;
                    result = new Vector<byte>(bits.Select(byte.Parse));
                    return true;
                default:
                    errorMessage = string.Format("No validation for effect type: {0}", effectType);
                    return false;
            }
        }

        private bool IsInvalid(string token)
        {
            int r;
            if (!int.TryParse(token, out r))
                return false;
            return r < 1 || r > 8;
        }

        private void ButtonApplyLeft_Click(object sender, RoutedEventArgs e)
        {
            BitmapSource source;
            if (ApplyEffect((BitmapSource) LeftImageBox.Source, (EffectType) ComboBoxEffects.SelectedIndex, out source))
            {
                LeftImageBox.Source = source;
                leftHistory.Add(source);
                MenuRedoLeft.IsEnabled = false;
                MenuUndoLeft.IsEnabled = true;
            }
        }

        private void ButtonApplyRight_OnClick(object sender, RoutedEventArgs e)
        {
            BitmapSource source;
            if (ApplyEffect((BitmapSource) RightImageBox.Source, (EffectType) ComboBoxEffects.SelectedIndex, out source))
            {
                RightImageBox.Source = source;
                rightHistory.Add(source);
                MenuRedoRight.IsEnabled = false;
                MenuUndoRight.IsEnabled = true;
            }
        }

        private void MenuItem_LeftToRight_OnClick(object sender, RoutedEventArgs e)
        {
            var bitmap = LeftImageBox.Source as BitmapSource;
            if (bitmap == null)
                return;
            var newBitmap = bitmap.Create(bitmap.GetBytes());
            RightImageBox.Source = newBitmap;
            ButtonApplyRight.IsEnabled = true;
            rightHistory.Add(newBitmap);
            MenuRedoRight.IsEnabled = false;
            MenuUndoRight.IsEnabled = rightHistory.Count > 1;
        }

        private void MenuItem_RightToLeft_OnClick(object sender, RoutedEventArgs e)
        {
            var bitmap = RightImageBox.Source as BitmapSource;
            if (bitmap == null)
                return;
            var newBitmap = bitmap.Create(bitmap.GetBytes());
            LeftImageBox.Source = newBitmap;
            ButtonApplyLeft.IsEnabled = true;
            leftHistory.Add(newBitmap);
            MenuUndoLeft.IsEnabled = leftHistory.Count > 1;
            MenuRedoLeft.IsEnabled = false;
        }

        private void ButtonPSNR_OnClick(object sender, RoutedEventArgs e)
        {
            var leftImage = LeftImageBox.Source as BitmapSource;
            var rightImage = RightImageBox.Source as BitmapSource;
            if (leftImage == null || rightImage == null)
                return;
            var bytesLeft = leftImage.GetGoodBytes();
            var bytesRight = rightImage.GetGoodBytes();
            if (bytesLeft.Length != bytesRight.Length || leftImage.PixelWidth != rightImage.PixelWidth
                || leftImage.PixelHeight != rightImage.PixelHeight)
            {
                MessageBox.Show("Images have different format", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            var sum = 0L;
            for (var i = 0; i < bytesLeft.Length; ++i)
            {
                sum += Sqr(bytesLeft[i] - bytesRight[i]);
            }
            var psnr = sum == 0
                ? double.PositiveInfinity
                : 10.0*Math.Log10(Sqr(255)*bytesLeft.Length * 1.0/sum);
            MessageBox.Show(string.Format("PSNR: {0:0.00####}", psnr), "PSNR", MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private long Sqr(long x)
        {
            return x*x;
        }

        private void ButtonApply_OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ButtonApplyLeft.IsEnabled && ButtonApplyRight.IsEnabled)
                ButtonPSNR.IsEnabled = true;
        }

        private void MenuItem_UndoLeft_Click(object sender, RoutedEventArgs e)
        {
            leftHistory.Pop();
            if (leftHistory.Count == 1)
                MenuUndoLeft.IsEnabled = false;
            LeftImageBox.Source = leftHistory.Current();
            MenuRedoLeft.IsEnabled = true;
        }

        private void MenuItem_RedoLeft_Click(object sender, RoutedEventArgs e)
        {
            leftHistory.Unpop();
            if (leftHistory.IsAtHead())
                MenuRedoLeft.IsEnabled = false;
            LeftImageBox.Source = leftHistory.Current();
            MenuUndoLeft.IsEnabled = true;
        }

        private void MenuItem_UndoRight_Click(object sender, RoutedEventArgs e)
        {
            rightHistory.Pop();
            if (rightHistory.Count == 1)
                MenuUndoRight.IsEnabled = false;
            RightImageBox.Source = rightHistory.Current();
            MenuRedoRight.IsEnabled = true;
        }

        private void MenuItem_RedoRight_Click(object sender, RoutedEventArgs e)
        {
            rightHistory.Unpop();
            if (rightHistory.IsAtHead())
                MenuRedoRight.IsEnabled = false;
            RightImageBox.Source = rightHistory.Current();
            MenuUndoRight.IsEnabled = true;
        }

        private void ComboBoxEffects_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var effectType = (EffectType) ComboBoxEffects.SelectedIndex;
            var parameter = effectType.GetParameter();
            if (parameter != null)
            {
                EffectParameterTextBlock.Text = parameter.ParameterName;
                if (parameter.DefaultValue != null)
                    EffectParameterComboBox.Text = parameter.DefaultValue.ToString();
                EffectParameterTextBlock.Visibility = Visibility.Visible;
                EffectParameterComboBox.Visibility = Visibility.Visible;
            }
            else
            {
                EffectParameterTextBlock.Visibility = Visibility.Hidden;
                EffectParameterComboBox.Visibility = Visibility.Hidden;
            }
            WaveletBorder.Visibility = effectType == EffectType.Wavelet ? Visibility.Visible : Visibility.Hidden;
            DctBorder.Visibility = effectType == EffectType.Dct ? Visibility.Visible : Visibility.Hidden;
        }

        private void ComboBoxEffects_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBoxEffects.ItemsSource = typeof(EffectType).GetEnumValues<EffectType>().Select(el => el.GetText());
            ComboBoxEffects.SelectedIndex = 0;
        }

        private void ComboBox_Decimation_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox_Decimation.ItemsSource = typeof(DecimationType).GetEnumValues<DecimationType>().Select(el => el.GetText());
            ComboBox_Decimation.SelectedIndex = 0;
        }

        private void ComboBox_Quantization_OnLoaded(object sender, RoutedEventArgs e)
        {
            ComboBox_Quantization.ItemsSource = typeof(QuantizationType).GetEnumValues<QuantizationType>().Select(el => el.GetText());
            ComboBox_Quantization.SelectedIndex = 0;
        }

        private void WaveletComboBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            WaveletTypeComboBox.ItemsSource = typeof(WaveletType).GetEnumValues<WaveletType>().Select(el => el.GetText());
            WaveletTypeComboBox.SelectedIndex = 0;
        }

        private void ComboBox_Quantization_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var quantizationType = (QuantizationType) ComboBox_Quantization.SelectedIndex;
            switch (quantizationType)
            {
                case QuantizationType.LargestN:
                    JPG_FirstText.Text = "N_Y:";
                    JPG_FirstParameter.Text = "10";
                    JPG_SecondText.Text = "N_C:";
                    JPG_SecondParameter.Text = "10";
                    JPG_FirstText.Visibility = Visibility.Visible;
                    JPG_FirstParameter.Visibility = Visibility.Visible;
                    JPG_SecondText.Visibility = Visibility.Visible;
                    JPG_SecondParameter.Visibility = Visibility.Visible;
                    JPG_ThirdText.Visibility = Visibility.Hidden;
                    JPG_ThirdParameter.Visibility = Visibility.Hidden;
                    JPG_FourthText.Visibility = Visibility.Hidden;
                    JPG_FourthParameter.Visibility = Visibility.Hidden;
                    break;
                case QuantizationType.QuantizationMatrix:
                    JPG_FirstText.Text = "\u03B1_Y:";
                    JPG_FirstParameter.Text = "1";
                    JPG_SecondText.Text = "\u0263_Y:";
                    JPG_SecondParameter.Text = "2";
                    JPG_ThirdText.Text = "\u03B1_C:";
                    JPG_ThirdParameter.Text = "1";
                    JPG_FourthText.Text = "\u0263_C:";
                    JPG_FourthParameter.Text = "4";
                    JPG_FirstText.Visibility = Visibility.Visible;
                    JPG_FirstParameter.Visibility = Visibility.Visible;
                    JPG_SecondText.Visibility = Visibility.Visible;
                    JPG_SecondParameter.Visibility = Visibility.Visible;
                    JPG_ThirdText.Visibility = Visibility.Visible;
                    JPG_ThirdParameter.Visibility = Visibility.Visible;
                    JPG_FourthText.Visibility = Visibility.Visible;
                    JPG_FourthParameter.Visibility = Visibility.Visible;
                    break;
                case QuantizationType.DefaultJpegMatrix:
                    JPG_FirstText.Visibility = Visibility.Hidden;
                    JPG_FirstParameter.Visibility = Visibility.Hidden;
                    JPG_SecondText.Visibility = Visibility.Hidden;
                    JPG_SecondParameter.Visibility = Visibility.Hidden;
                    JPG_ThirdText.Visibility = Visibility.Hidden;
                    JPG_ThirdParameter.Visibility = Visibility.Hidden;
                    JPG_FourthText.Visibility = Visibility.Hidden;
                    JPG_FourthParameter.Visibility = Visibility.Hidden;
                    break;
                default:
                    throw new Exception("Invalid program state");
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "MyJPEG Image Files (*.myjpg)|*.myjpg",
                DefaultExt = "myjpg"
            };
            var showResult = saveFileDialog.ShowDialog();
            if (showResult.HasValue && showResult.Value)
            {
                JPG_SavePath.Text = saveFileDialog.FileName;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                Filter = "MyJPEG Image Files (*.wlt)|*.wlt",
                DefaultExt = "wlt"
            };
            var showResult = saveFileDialog.ShowDialog();
            if (showResult.HasValue && showResult.Value)
            {
                WaveletSavePath.Text = saveFileDialog.FileName;
            }
        }

        private void MenuItem_Left_OpenJPG_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadMyJpeg();
            if (image != null)
            {
                PushLeft(image);
            }
        }

        private void MenuItem_Right_OpenJPG_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadMyJpeg();
            if (image != null)
            {
                PushRight(image);
            }
        }

        private void MenuItem_Left_OpenWavelet_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadWavelet();
            if (image != null)
            {
                PushLeft(image);
            }
        }

        private void MenuItem_Right_OpenWavelet_OnClick(object sender, RoutedEventArgs e)
        {
            var image = LoadWavelet();
            if (image != null)
            {
                PushRight(image);
            }
        }

        private BitmapSource LoadWavelet()
        {
            var savePath = GetSavePath("Wavelet Image Files (*.wlt)|*.wlt");
            if (savePath == null)
                return null;
            return Wavelet.OpenWavelet(savePath);
        }

        private BitmapSource LoadMyJpeg()
        {
            var parameters = GetDctParameters();
            if (parameters.SavePath == null)
                return null;
            return DctAlgorithm.DecodeDct(parameters);
        }

        private DctParameters GetDctParameters()
        {
            return new DctParameters
            {
                SavePath = GetSavePath("MyJPEG Image Files (*.myjpg)|*.myjpg"),
            };
        }

        private string GetSavePath(string filter)
        {
            var openFile = new OpenFileDialog
            {
                Multiselect = false,
                Filter = filter,
            };
            var result = openFile.ShowDialog();
            if (result.HasValue && result.Value)
            {
                return openFile.FileName;
            }
            return null;
        }

        private readonly PersistentStack<BitmapSource> leftHistory = new PersistentStack<BitmapSource>();
        private readonly PersistentStack<BitmapSource> rightHistory = new PersistentStack<BitmapSource>();
    }
}