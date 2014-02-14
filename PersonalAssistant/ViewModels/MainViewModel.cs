using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Threading;
using Newtonsoft.Json;
using PersonalAssistant.Resources;
using PersonalAssistant.Service.Weather;
using PersonalAssistant.Service.Weather.LocalWeather;

namespace PersonalAssistant.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private WeatherDataManager weatherDataManager;
        public MainViewModel()
        {
            weatherDataManager = WeatherDataManager.GetInstance();
            this.RecentItems = new ObservableCollection<ResponseItem>();
            this.Places = new ObservableCollection<Place>();
        }

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<ResponseItem> RecentItems { get; set; }
        public ObservableCollection<Place> Places { get; set; } 
        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // Sample data; replace with real data
            LoadRecentItems();
            foreach (KeyValuePair<string, Place> dictionaryEntry in weatherDataManager.getPlaces())
            {
                String key = (String) dictionaryEntry.Key;
                Place value = (Place) dictionaryEntry.Value;
                Places.Add(value);
            }
//            RecentItems = new ObservableCollection<ResponseItem>
//            {
//                new ResponseItem()
//                {
//                    ImageUri = "/Images/safoora.JPG",
//                    DetailString = "SampleDS",
//                    DateTime = DateTime.Now,
//                    ID = "1",
//                    ResponseString = "SampleRS"
//                }
//            };
            this.IsDataLoaded = true;
        }
        public void PersistData()
        {
            // Sample data; replace with real data
            PersistRecentItems();
            PersistPlaces();
        }

        private void PersistPlaces()
        {
            weatherDataManager.SetPlaces(Places);
            weatherDataManager.SavePlaces();
        }


        private void LoadRecentItems()
        {
            ObservableCollection<ResponseItem> LoadedRecentItems;
            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    "RecentItem.txt",
                    System.IO.FileMode.Open,
                    local))
                {
                    // Write the data from the textbox.
                    using (var isoFileReader = new System.IO.StreamReader(isoFileStream))
                    {
                        LoadedRecentItems = JsonConvert.DeserializeObject<ObservableCollection<ResponseItem> >(isoFileReader.ReadToEnd());

                        System.Diagnostics.Debug.WriteLine("data loaded with " + LoadedRecentItems.Count + " items ");
                        isoFileReader.Close();
                        //                        MessageBox.Show(serializedWeather);
                    }
                }
            }
            catch (Exception e)
            {
                LoadedRecentItems = new ObservableCollection<ResponseItem>();
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            
            foreach (ResponseItem loadedRecentItem in LoadedRecentItems)
            {
                RecentItems.Add(loadedRecentItem);
            }
        }
        private void PersistRecentItems()
        {

            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    "RecentItem.txt",
                    System.IO.FileMode.Create,
                    local))
                {
                    // Write the data from the textbox.
                    using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
                    {
                        string serialaized =JsonConvert.SerializeObject(RecentItems);

                        isoFileWriter.Write(serialaized);
                        isoFileWriter.Close();

                        System.Diagnostics.Debug.WriteLine("data persisted with " + RecentItems.Count + " items ");
                        //                        MessageBox.Show(serializedWeather);
                    }
                }
            }
            catch (Exception e)
            {

                System.Diagnostics.Debug.WriteLine(e.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}