using GMap.NET;
using GMap.NET.WindowsPresentation;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace SpeechPhoto_WPF
{
    /// <summary>
    /// Interaction logic for GGMap.xaml
    /// </summary>
    public partial class GGMap : Window
    {
        public GGMap()
        {
            InitializeComponent();
        }
        private async void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string apiKey = "b7774cc1e6cc457daa4bf1d296f5adcd";
                string apiUrl = $"https://ipgeolocation.abstractapi.com/v1/?api_key={apiKey}&ip_address={await GetIPAddressAsync()}";

                // Thiết lập cài đặt cho bản đồ
                GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
                mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
                mapView.MinZoom = 2;
                mapView.MaxZoom = 17;
                mapView.Zoom = 2;
                mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
                mapView.CanDragMap = true;
                mapView.DragButton = MouseButton.Left;

                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        dynamic locationData = JObject.Parse(jsonString);

                        double latitude = locationData.latitude;
                        double longitude = locationData.longitude;

                        mapView.Position = new PointLatLng(latitude, longitude);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Failed to retrieve location data from API.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private async Task<string> GetIPAddressAsync()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("https://api.ipify.org?format=json");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    dynamic ipData = JObject.Parse(jsonString);
                    return ipData.ip;
                }
                else
                {
                    return null;
                }
            }
        }

        private void MoveButton_Click(object sender, RoutedEventArgs e)
        {
            double latitude, longitude;
            if (double.TryParse(latitudeTextBox.Text, out latitude) && double.TryParse(longitudeTextBox.Text, out longitude))
            {
                mapView.Position = new PointLatLng(latitude, longitude);
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid Latitude or Longitude!");
            }
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

        private async void MoveToAPILocation_Click(object sender, RoutedEventArgs e)
        {
            string ipAddress = await GetLocalIPAddressAsync();

            string apiKey = "b7774cc1e6cc457daa4bf1d296f5adcd";
            string apiUrl = "https://ipgeolocation.abstractapi.com/v1/?api_key=" + apiKey + "&ip_address=" + ipAddress;

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        dynamic apiData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

                        double latitude = apiData.latitude;
                        double longitude = apiData.longitude;

                        mapView.Position = new PointLatLng(latitude, longitude);

                        // Tạo một GMapMarker để hiển thị cờ
                        GMapMarker flagMarker = new GMapMarker(new PointLatLng(latitude, longitude));

                        // Tạo hình ảnh cho cờ từ tệp hình ảnh bạn đã thêm vào dự án
                        //string imagePath = @"C:\Users\DucAnh\Downloads\hinh\hinh1.png";
                        //BitmapImage flagImage = new BitmapImage(new Uri("https://res.cloudinary.com/dadxpzage/image/upload/v1711451547/point_qstesw.jpg", UriKind.Relative));
                        BitmapImage flagImage = new BitmapImage(new Uri("https://res.cloudinary.com/dadxpzage/image/upload/v1711451547/point_qstesw.jpg"));

                        // Đặt hình ảnh cho GMapMarker
                        flagMarker.Shape = new System.Windows.Controls.Image
                        {
                            Source = flagImage,
                            Width = 30,
                            Height = 30
                        };

                        // Thêm GMapMarker vào bản đồ
                        mapView.Markers.Add(flagMarker);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Failed to retrieve location from API.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
