using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using Windows.Devices.Geolocation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using PersonalAssistant.Service;
using PersonalAssistant.Service.Weather;
using PersonalAssistant.ViewModels;

namespace PersonalAssistant
{
    public partial class PlacesPage : PhoneApplicationPage
    {
        // Constructor
        private Settings settings;
        private ProgressIndicator progressIndicator;
        public PlacesPage()
        {
            InitializeComponent();

            // Set the data context of the LongListSelector control to the sample data
            DataContext = App.ViewModel;
            this.onLoadedEventHandler = new RoutedEventHandler(OnLoaded);
            this.Loaded += onLoadedEventHandler;
            settings = Settings.GetInstance();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private RoutedEventHandler onLoadedEventHandler;

        public Boolean IsCelsius
        {
            get { return settings.MetricTemp == "Celsius"; }
            set
            {
                if (value == false)
                    return;
                settings.MetricTemp = "Celsius";
            }
        }
        public Boolean IsFahrenheit
        {
            get { return settings.MetricTemp == "Fahrenheit"; }
            set
            {
                if (value == false)
                    return;
                settings.MetricTemp = "Fahrenheit";
            }
        }

        public String apiKey
        {
            get { return settings.APIKey; }
            set
            {
                settings.APIKey = value;
            }
        }

        // Register the voice commands. Called when the app is first launched. 
        public async void OnLoaded(object sender, EventArgs e)
        {
            
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            SystemTray.SetBackgroundColor(this, Colors.Orange);
            SystemTray.SetForegroundColor(this, Colors.Black);
        }

        private void MyContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            Place selected = (sender as MenuItem).DataContext as Place;
            App.ViewModel.Places.Remove(selected);
            if (selected.IsLocal)
            {
                    var firstOrDefault = App.ViewModel.Places.FirstOrDefault();
                    if (firstOrDefault != null)
                        firstOrDefault.IsLocal = true;
            }
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            WeatherDataManager.GetInstance().SetPlaces(App.ViewModel.Places);
            WeatherDataManager.GetInstance().SavePlaces();
        }

        private void AddNewPlace(object sender, RoutedEventArgs e)
        {
            string newPlaceName = NewPlaceName.Text.Trim();
            if (newPlaceName.Length == 0)
            {
                MessageBox.Show("please enter a city name");
                return;
            }
            foreach (Place place in App.ViewModel.Places)
            {
                if (place.Name.Equals(newPlaceName))
                {
                    MessageBox.Show("please enter a different name");
                    return;
                }
            }
            Place p;
            if (!LongitudeBox.Text.Any() || !LatitudeBox.Text.Any())
                p = new Place(NewPlaceName.Text);
            else
            {
                p = new Place(NewPlaceName.Text, Double.Parse(LatitudeBox.Text), Double.Parse(LongitudeBox.Text));
            }
            App.ViewModel.Places.Add(p);
        }

        private async void FillLocalPlaceInfo(object sender, RoutedEventArgs e)
        {
            if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            {
                MessageBoxResult result =
                    MessageBox.Show("This app accesses your phone's location. Is that ok?",
                    "Location",
                    MessageBoxButton.OKCancel);

                if (result == MessageBoxResult.OK)
                {
                    IsolatedStorageSettings.ApplicationSettings["LocationConsent"] = true;
                }
                else
                {
                    return;
                }
                IsolatedStorageSettings.ApplicationSettings.Save();
            }

            Geolocator geolocator = new Geolocator();

            geolocator.DesiredAccuracyInMeters = 50;

            try
            {
                progressIndicator = new ProgressIndicator();
                progressIndicator.Text = "Looking for your position";
                progressIndicator.IsVisible = true;
                progressIndicator.IsIndeterminate = true;
                SystemTray.SetProgressIndicator(this,progressIndicator);
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );
                LatitudeBox.Text = geoposition.Coordinate.Latitude.ToString("0.000");
                LongitudeBox.Text = geoposition.Coordinate.Longitude.ToString("0.000");
                progressIndicator.IsVisible =false;
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("location is disabled in phone settings.");
                }
                //else
                {
                    MessageBox.Show("could not get your location please try again later");
                }
            }
        }

        private void SetAsDefault(object sender, RoutedEventArgs e)
        { 
            var RadioButton = (sender as RadioButton);
            Place selected = RadioButton.DataContext as Place;
            selected.IsLocal = true;
            foreach (Place place in App.ViewModel.Places)
            {
                if(place!=selected)
                    place.IsLocal = false;
            }
        }

        private void HelpAPIKey(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Actually Weather Data is not freely available in large scale! So I decided to " +
                "ask for your help to deliver this app to you full featured and free.\n\rThe only " +
                "thing you should do is to click on the link blow the text box and register " +
                "for a key from WorldWeatherOnline and after you activate your registration " +
                "go the other link the and fill the tiny form as you wish and make sure to uncheck the " +
                "\r\n'issue a new key from Premium Access'\r\nand check\r\n'I agree to the terms of service'\r\n and you’re done! You’ll see a key like 'pehqskn6dr4hx4chhtgmrn1k' just copy that in the box below");

        }

        private void GoToWeatherOnline(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("http://developer.worldweatheronline.com/member/register"));
        }

        private void GoToWeatherOnlineGetAPI(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("http://developer.worldweatheronline.com/apps/register"));
        }

        private void ShowAddHelp(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Please add new locations by its city level name or use boxes below to enter its geographical position latitude/longitude. You can use 'local' button to fill them by your current location");
        }

        private void SendFeedBack(object sender, RoutedEventArgs e)
        {
            EmailComposeTask emailcomposer = new EmailComposeTask();
            emailcomposer.To = "\"Hassan Nasr\"<hne1991@live.com>";
            emailcomposer.Subject = "FeedBack Mosi App";
            emailcomposer.Show();
        }

        private void GoToMosiPage(object sender , RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("https://www.facebook.com/pages/Salar/288872641128941"));
        }

        private void GoToWorldWeatherOnline(object sender, RoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(new Uri("http://www.worldweatheronline.com"));
        }
    }
}