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
using PersonalAssistant.Service;
using PersonalAssistant.Service.Weather;
using PersonalAssistant.ViewModels;

namespace PersonalAssistant
{
    public partial class PlacesPage : PhoneApplicationPage
    {
        // Constructor
        public PlacesPage()
        {
            InitializeComponent();

            // Set the data context of the LongListSelector control to the sample data
            DataContext = App.ViewModel;
            this.onLoadedEventHandler = new RoutedEventHandler(OnLoaded);
            this.Loaded += onLoadedEventHandler;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private RoutedEventHandler onLoadedEventHandler;

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
            System.Diagnostics.Debug.WriteLine(App.ViewModel.Places.Count + " is places count");
            
        }

        private void MyContextMenu_Opened(object sender, RoutedEventArgs e)
        {
            
        }

        private void DeleteItem(object sender, RoutedEventArgs e)
        {
            Place selected = (sender as MenuItem).DataContext as Place;
            App.ViewModel.Places.Remove(selected);
            System.Diagnostics.Debug.WriteLine(selected);
//            System.Diagnostics.Debug.WriteLine(selected.name);
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
                Geoposition geoposition = await geolocator.GetGeopositionAsync(
                    maximumAge: TimeSpan.FromMinutes(5),
                    timeout: TimeSpan.FromSeconds(10)
                    );

                LatitudeBox.Text = geoposition.Coordinate.Latitude.ToString("0.000");
                LongitudeBox.Text = geoposition.Coordinate.Longitude.ToString("0.000");
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    MessageBox.Show("location  is disabled in phone settings.");
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
//            foreach (Place place in App.ViewModel.Places)
//            {
//                if(place!=selected)
//                    place.IsLocal = false;
//            }
//            System.Diagnostics.Debug.WriteLine(selected);
        }
    }
}