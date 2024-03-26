using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
        private void mapView_Loaded(object sender, RoutedEventArgs e)
        {
            GMap.NET.GMaps.Instance.Mode = GMap.NET.AccessMode.ServerAndCache;
            // choose your provider here
            mapView.MapProvider = GMap.NET.MapProviders.OpenStreetMapProvider.Instance;
            mapView.MinZoom = 2;
            mapView.MaxZoom = 17;
            // whole world zoom
            mapView.Zoom = 2;
            // lets the map use the mousewheel to zoom
            mapView.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
            // lets the user drag the map
            mapView.CanDragMap = true;
            // lets the user drag the map with the left mouse button
            mapView.DragButton = MouseButton.Left;
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
                MessageBox.Show("Invalid Latitude or Longitude!");
            }
        }

        private async void MoveToAPILocation_Click(object sender, RoutedEventArgs e)
        {
            string apiKey = "b7774cc1e6cc457daa4bf1d296f5adcd";
            string apiUrl = "https://ipgeolocation.abstractapi.com/v1/?api_key=" + apiKey + "&ip_address=113.23.109.239";

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
                        flagMarker.Shape = new Image
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
                        MessageBox.Show("Failed to retrieve location from API.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
