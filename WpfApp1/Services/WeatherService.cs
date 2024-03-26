//using Newtonsoft.Json.Linq;
//using RestSharp;
//using System;
//using System.Threading.Tasks;

//namespace SpeechPhoto_WPF.Services
//{
//    public class WeatherService
//    {
//        private string apiKey = "5a614b117e4a38a762d900a7a5ef7ccf";

//        public async Task<string> GetWeatherAsync(string city)
//        {
//            var client = new RestClient($"http://api.openweathermap.org/data/2.5/weather?q={city}&appid={apiKey}&units=metric");
//            var request = new RestRequest(Method.GET);
//            IRestResponse response = await client.ExecuteAsync(request);

//            if (response.IsSuccessful)
//            {
//                JObject jsonResponse = JObject.Parse(response.Content);
//                string weatherDescription = jsonResponse["weather"][0]["description"].ToString();
//                string temperature = jsonResponse["main"]["temp"].ToString();
//                string result = $"Weather: {weatherDescription}, Temperature: {temperature}°C";
//                return result;
//            }
//            else
//            {
//                throw new Exception("Error occurred while fetching weather data.");
//            }
//        }
//    }
//}
