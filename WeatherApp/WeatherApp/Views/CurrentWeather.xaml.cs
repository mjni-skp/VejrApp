using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WeatherApp.Helper;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CompleteWeatherApp.Models;
using Xamarin.Essentials;

namespace WeatherApp.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CurrentWeather : ContentPage
    {
        public CurrentWeather()
        {
            InitializeComponent();
            GetCoordinates();
        }

        private string Location { get; set; } = "Ireland";
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        //Get locations "current location" in terms of latitude and longitude coordinates
        private async void GetCoordinates()
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best);
                var location = await Geolocation.GetLocationAsync(request);

                if(location != null)
                {
                    Latitude = location.Latitude;
                    Longitude = location.Longitude;
                    Location = await GetCity(location);

                    GetWeatherInfo();
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.StackTrace);
            }
        }
        
       
        private async Task<string> GetCity(Location location)
        {
            var places = await Geocoding.GetPlacemarksAsync(location);
            var currentPlace = places?.FirstOrDefault();

            //returns city and country (Locality and CountryName)
            if (currentPlace != null)
            {
                return $"{currentPlace.Locality},{currentPlace.CountryName}";
            }
            return null;
        }

        private async void GetWeatherInfo()
        {
            var url = $"http://api.openweathermap.org/data/2.5/weather?q={Location}&appid=8110f218daad768d9ce7a741f6580aab&units=metric";

            var result = await ApiCaller.Get(url);

            if (result.Successful)
            {
                try
                {
                    var weatherInfo = JsonConvert.DeserializeObject<WeatherInfo>(result.Response);
                    descriptionTxt.Text = weatherInfo.weather[0].description.ToUpper();
                    iconImg.Source = $"w{weatherInfo.weather[0].icon}";
                    cityTxt.Text = weatherInfo.name.ToUpper();
                    temperatureTxt.Text = weatherInfo.main.temp.ToString("0");

                    var dt = new DateTime().ToUniversalTime().AddSeconds(weatherInfo.dt);
                    dateTxt.Text = dt.ToString("dddd, MMM dd").ToUpper();
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Weather Info", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("weather Info", "No weather information found", "OK");
            }
        }
    }
}