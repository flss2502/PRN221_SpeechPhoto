using Microsoft.CognitiveServices.Speech;
using System.IO;
using System.Windows;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;

namespace SpeechPhoto_WPF
{
    public partial class MainWindow : Window
    {
        private SpeechRecognizer recognizer;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap capturedBitmap;

        public MainWindow()
        {
            InitializeSpeechRecognizer();
            InitializeWebcam();
        }

        private void InitializeSpeechRecognizer()
        {
            var speechConfig = SpeechConfig.FromSubscription("2583945cfeae48e5bf5411b7debeb01d", "southeastasia");
            recognizer = new SpeechRecognizer(speechConfig);
            recognizer.Recognized += Recognizer_Recognized;
        }

        private async void Recognizer_Recognized(object sender, SpeechRecognitionEventArgs e)
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                string recognizedText = e.Result.Text;

                // Check if recognized text contains command to take a photo
                if (recognizedText.ToLower().Contains("take photo"))
                {
                    // Call your photo capture function here
                    await TakePhoto();
                }
            }
        }

        private async Task TakePhoto()
        {
            if (videoSource == null || !videoSource.IsRunning)
            {
                MessageBox.Show("Webcam is not available.");
                return;
            }

            Bitmap frame = null;
            videoSource.NewFrame += (s, e) =>
            {
                frame = (Bitmap)e.Frame.Clone();
            };

            while (frame == null)
            {
                await Task.Delay(100);
            }

            //string tempFilePath = Path.GetTempFileName();
            //frame.Save(tempFilePath, ImageFormat.Png);

            //ShowCapturedPhotoWindow(tempFilePath);

            string directoryPath = @"F:\CapturedPhotos"; // Change this to your desired directory path
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string fileName = $"CapturedPhoto_{DateTime.Now:yyyyMMddHHmmss}.png";

            string filePath = Path.Combine(directoryPath, fileName);
            frame.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);

            MessageBox.Show($"Photo saved to {filePath}");
        }

        private async void StartRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await recognizer.StartContinuousRecognitionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void StopRecognitionButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await recognizer.StopContinuousRecognitionAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void InitializeWebcam()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count > 0)
            {
                videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += VideoSource_NewFrame;

                videoSource.Start();
            }
            else
            {
                MessageBox.Show("No video devices found.");
            }
        }

        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            capturedBitmap = (Bitmap)eventArgs.Frame.Clone();
            BitmapImage bitmapImage = ConvertToBitmapImage(capturedBitmap);
            Dispatcher.Invoke(() => webcamImage.Source = bitmapImage);
        }

        private BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        //private void ShowCapturedPhotoWindow(string tempFilePath)
        //{
        //    CapturedPhotoWindow capturedPhotoWindow = new CapturedPhotoWindow(tempFilePath);
        //    capturedPhotoWindow.ShowDialog();
        //}

        //private async Task UpdateWeatherOverlay()
        //{
        //    if (capturedBitmap == null)
        //        return;

        //    WeatherService weatherService = new WeatherService();
        //    string city = "YOUR_CITY"; // Thay YOUR_CITY bằng tên thành phố của bạn
        //    try
        //    {
        //        string weatherInfo = await weatherService.GetWeatherAsync(city);
        //        using (Graphics graphics = Graphics.FromImage(capturedBitmap))
        //        {
        //            // Vẽ thông tin thời tiết lên ảnh
        //            graphics.DrawString(weatherInfo, new Font("Arial", 12), Brushes.White, new PointF(10, 10));
        //        }
        //        BitmapImage bitmapImage = ConvertToBitmapImage(capturedBitmap);
        //        webcamImage.Source = bitmapImage;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

    }
}
