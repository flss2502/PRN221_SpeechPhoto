using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;

namespace Weather
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
        private string APIKey = "de654bfa63689994c449fc63b93ef46a";

       

        private void GetWeather()
        {
            using (WebClient web = new WebClient())
            {

               
                string url = $"https://api.openweathermap.org/data/2.5/weather?q={TbCity.Text}&appid={APIKey}";
                var json = web.DownloadString(url);
                WeatherInfo.root info = JsonConvert.DeserializeObject<WeatherInfo.root>(json); PicIcon.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri($"https://openweathermap.org/img/w/{info.weather[0].icon}.png"));
                LabCondition.Text = info.weather[0].main;
                LabDetail.Text = info.weather[0].description;
                LabSunset.Text = ConvertDateTime(info.sys.sunset).ToString();
                LabSunrise.Text = ConvertDateTime(info.sys.sunrise).ToString();
                LabWindspeed.Text = info.wind.speed.ToString();
                LabPressure.Text = info.main.pressure.ToString();
                double tempKelvin = info.main.temp;
                double tempCelsius = tempKelvin - 273.15;
                Temp.Text = $"{tempCelsius} °C";


            }
        }

        private DateTime ConvertDateTime(long millisec)
        {
            DateTime day = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            day = day.AddSeconds(millisec).ToLocalTime();
            return day;
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetWeather();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
