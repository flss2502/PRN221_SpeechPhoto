using Microsoft.CognitiveServices.Speech;
using System.IO;
using System.Windows;
using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Drawing.Imaging;
using System.Windows.Input;
using System.Collections.ObjectModel;
using WinForms = System.Windows.Forms;
using Path = System.IO.Path;
using System.Net.Http;
using System.Net;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Weather;
using static SpeechPhoto_WPF.Image;

namespace SpeechPhoto_WPF
{
    public partial class MainWindow : Window
    {


        private SpeechRecognizer recognizer;
        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoSource;
        private Bitmap capturedBitmap;
        private ObservableCollection<ImageViewModel> imageList;
        private string selectedFolderPath;


        public MainWindow()
        {
            InitializeComponent();
            InitializeSpeechRecognizerAsync();
            InitializeWebcam();
            imageList = new ObservableCollection<ImageViewModel>();
            imageListView.ItemsSource = imageList;
            GetWeatherApi();
            StartRecognition();


            foreach (Window window in System.Windows.Application.Current.Windows)
            {
                if (window is Image imageWindow)
                {
                    imageWindow.ImageDeleted += ImageWindow_ImageDeleted;
                }
            }
        }

        private Task InitializeSpeechRecognizerAsync()
        {
            var speechConfig = SpeechConfig.FromSubscription("2583945cfeae48e5bf5411b7debeb01d", "southeastasia");
            recognizer = new SpeechRecognizer(speechConfig);
            recognizer.Recognized += Recognizer_Recognized;

            return Task.CompletedTask;
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

        private async void StartRecognition()
        {
            try
            {
                await recognizer.StartContinuousRecognitionAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private async Task TakePhoto()
        {
            if (videoSource == null || !videoSource.IsRunning)
            {
                System.Windows.MessageBox.Show("Webcam is not available.");
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

            Dispatcher.BeginInvoke(new Action(() =>
            {
                CapturedPhotoWindow capturedPhotoWindow = new CapturedPhotoWindow(frame);
                capturedPhotoWindow.ShowDialog();
            }));
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
                System.Windows.MessageBox.Show("No video devices found.");
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

        private void imageListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (imageListView.SelectedItem != null)
            {
                ImageViewModel selectedImage = (ImageViewModel)imageListView.SelectedItem;
                ShowImageViewer(selectedImage);

            }
        }

        private void btnBrowseFile_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog dialog = new WinForms.FolderBrowserDialog();
            dialog.InitialDirectory = "";
            WinForms.DialogResult result = dialog.ShowDialog();

            if (dialog != null && result == WinForms.DialogResult.OK)
            {
                selectedFolderPath = dialog.SelectedPath;
                txbFolderName.Text = selectedFolderPath;
                LoadFolder(selectedFolderPath);
            }
        }

        public void LoadFolder(string folderPath)
        {
            imageList.Clear();
            LoadDirectory(folderPath);
        }

        private void LoadDirectory(string path)
        {
            var directoryInfo = new DirectoryInfo(path);

            foreach (var subDirectory in directoryInfo.GetDirectories())
            {
                AddImageToList(subDirectory.FullName);
            }

            foreach (var file in directoryInfo.GetFiles())
            {
                AddImageToList(file.FullName);
            }
        }

        private void AddImageToList(string imagePath)
        {
            try
            {
                BitmapImage image = new BitmapImage(new Uri(imagePath));
                string imageName = Path.GetFileName(imagePath);

                imageList.Add(new ImageViewModel { Thumbnail = image, Name = imageName });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image: {ex.Message}");
            }
        }

        private void ShowImageViewer(ImageViewModel selectedImage)
        {
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                try
                {
                    string imagePath = Path.Combine(selectedFolderPath, selectedImage.Name);
                    Image viewerWindow = new Image(imageList, selectedFolderPath, imagePath);
                    viewerWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error opening image viewer: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Please select a folder before viewing images.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void locate_Click(object sender, RoutedEventArgs e)
        {
            GGMap ggMapWindow = new GGMap();
            ggMapWindow.Show();
        }

        private async Task<string> GetLocalIPAddressAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://api.ipify.org");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return "Failed to retrieve local IP address.";
                }
            }
        }

        private async Task<string> GetLocationFromApiAsync()
        {
            string ipAddress = await GetLocalIPAddressAsync();

            string apiKey = "b7774cc1e6cc457daa4bf1d296f5adcd";
            string apiUrl = "https://ipgeolocation.abstractapi.com/v1/?api_key=" + apiKey + "&ip_address=" + ipAddress;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        dynamic apiData = JObject.Parse(jsonString);
                        locationTextBlock.Text = $"{apiData.region}, {apiData.country}, {apiData.continent}";
                        string location = $"{apiData.region}";
                        return location;
                    }
                    else
                    {
                        return "Failed to retrieve location from API.";
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

 

        private string APIKey = "de654bfa63689994c449fc63b93ef46a";

        private async void GetWeatherApi()
        {
            string location = await GetLocationFromApiAsync();

            using (WebClient web = new WebClient())
            {

                string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={APIKey}";
                var json = web.DownloadString(url);
                WeatherInfo.root info = JsonConvert.DeserializeObject<WeatherInfo.root>(json); weatherTextBlock.Source = new BitmapImage(new Uri($"https://openweathermap.org/img/w/{info.weather[0].icon}.png"));
                conditionTextBlock.Text = info.weather[0].main;
                double tempKelvin = info.main.temp;
                double tempCelsius = tempKelvin - 273.15;
                double roundedTempCelsius = Math.Round(tempCelsius, 1); 
                temperatureTextBlock.Text = $"{roundedTempCelsius} °C";

            }
        }

        private void weather_Click(object sender, RoutedEventArgs e)
        {
            Weather weather = new Weather();
            weather.Show();
        }

        private void ImageWindow_ImageDeleted(object sender, Image.ImageEventArgs e)
        {
            string deletedImagePath = e.ImagePath;
            // Xóa ảnh khỏi danh sách
            DeleteImage(deletedImagePath);
        }

        void DeleteImage(string imagePath)
        {
            
            if (File.Exists(imagePath))
            {
                try
                {
                    File.Delete(imagePath);
                    System.Windows.MessageBox.Show("Image deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    LoadFolder(selectedFolderPath);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Error deleting image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Error: Image file not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}