using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using Weather;

namespace SpeechPhoto_WPF
{
    /// <summary>
    /// Interaction logic for Weather.xaml
    /// </summary>
    public partial class Weather : Window
    {
        private string APIKey = "de654bfa63689994c449fc63b93ef46a";

        public Weather()
        {
            InitializeComponent();
            GetWeatherByIPAsync(); // Gọi phương thức để lấy thông tin thời tiết dựa trên địa chỉ IP khi khởi tạo
        }

        private void DisplayWeatherInfo(WeatherInfo.root info, string location)
        {
            // Hiển thị các thông tin thời tiết lên giao diện
            LabLocation.Text = location;
            PicIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"https://openweathermap.org/img/w/{info.weather[0].icon}.png"));
            LabCondition.Text = info.weather[0].main;
            LabDetail.Text = info.weather[0].description;
            LabSunset.Text = ConvertDateTime(info.sys.sunset).ToString();
            LabSunrise.Text = ConvertDateTime(info.sys.sunrise).ToString();
            LabWindspeed.Text = info.wind.speed.ToString();
            LabPressure.Text = info.main.pressure.ToString();
            Temp.Text = $"{info.main.temp} °C";
        }

        private DateTime ConvertDateTime(long millisec)
        {
            DateTime day = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            day = day.AddSeconds(millisec).ToLocalTime();
            return day;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string city = TbCity.Text.Trim();
            if (!string.IsNullOrEmpty(city))
            {
                GetWeather(city);
            }
            else
            {
                System.Windows.MessageBox.Show("Please enter a city name.");
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void GetWeather(string location)
        {
            location = FormatCityName(location);
            using (WebClient web = new WebClient())
            {
                try
                {
                    string url = $"https://api.openweathermap.org/data/2.5/weather?q={location}&appid={APIKey}&units=metric";
                    var json = web.DownloadString(url);
                    WeatherInfo.root info = JsonConvert.DeserializeObject<WeatherInfo.root>(json);
                    DisplayWeatherInfo(info, location); // Truyền giá trị của location vào phương thức DisplayWeatherInfo
                }
                catch (WebException ex)
                {
                    System.Windows.MessageBox.Show($"Error retrieving weather data: {ex.Message}");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"An error occurred: {ex.Message}");
                }
            }
        }

        private string FormatCityName(string cityName)
        {
            if (string.IsNullOrEmpty(cityName))
            {
                return string.Empty;
            }

            string[] words = cityName.ToLower().Split(' ');
            string formattedCityName = string.Join(" ", words.Select(word => char.ToUpper(word[0]) + word.Substring(1)));

            return formattedCityName;
        }

        private async Task<string> GetIPAddressAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync("https://api.ipify.org/?format=json");
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
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error getting IP address: {ex.Message}");
                return null;
            }
        }

        private async void GetWeatherByIPAsync()
        {
            string ipAddress = await GetIPAddressAsync();
            if (string.IsNullOrEmpty(ipAddress))
            {
                System.Windows.MessageBox.Show("Unable to retrieve IP address.");
                return;
            }

            // Lấy thông tin vị trí từ địa chỉ IP
            string location = await GetLocationFromIPAsync(ipAddress);
            if (string.IsNullOrEmpty(location))
            {
                System.Windows.MessageBox.Show("Unable to retrieve location from IP address.");
                return;
            }

            // Gửi yêu cầu thời tiết dựa trên vị trí
            GetWeather(location);
        }

        private async Task<string> GetLocationFromIPAsync(string ipAddress)
        {
            string apiKey = "b7774cc1e6cc457daa4bf1d296f5adcd";
            string apiUrl = $"https://ipgeolocation.abstractapi.com/v1/?api_key=" + apiKey + "&ip_address=" + ipAddress;

            try
            {
                using (var client = new HttpClient())
                {
                    var response = await client.GetAsync(apiUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var jsonString = await response.Content.ReadAsStringAsync();
                        dynamic apiData = JObject.Parse(jsonString);
                        return $"{apiData.city}, {apiData.country}";
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error getting location from IP: {ex.Message}");
                return null;
            }
        }
    }
}
