using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Phone.Speech.VoiceCommands;
using Windows.Security.Authentication.OnlineId;
using Newtonsoft.Json;
using PersonalAssistant.Annotations;
using PersonalAssistant.Service.Weather.LocalWeather;

namespace PersonalAssistant.Service.Weather
{
    public class WeatherDataManager
    {
        private static WeatherDataManager _weatherDataManager;

        public static WeatherDataManager GetInstance()
        {
            if(_weatherDataManager == null)
                _weatherDataManager = new WeatherDataManager();
            return _weatherDataManager;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SavePlaces()
        {
            try
            {
                // Get the local folder.
                System.IO.IsolatedStorage.IsolatedStorageFile local =
                    System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

                // Create a new folder named DataFolder.
                if (!local.DirectoryExists("WeatherFolder"))
                    local.CreateDirectory("WeatherFolder");
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    "Places.txt",
                    System.IO.FileMode.Create,
                    local))
                {
                    using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
                    {
                        string serializedPlaces = JsonConvert.SerializeObject(places);
                        System.Diagnostics.Debug.WriteLine(serializedPlaces);
                        System.Diagnostics.Debug.WriteLine("CurrentCommandLangs: "+VoiceCommandService.InstalledCommandSets.ToString());
                      
                        VoiceCommandSet widgetVcs = VoiceCommandService.InstalledCommandSets["en-us-1"];
                        widgetVcs.UpdatePhraseListAsync("place",places.Keys);

                        isoFileWriter.Write(serializedPlaces);
                        isoFileWriter.Close();
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            
        }
        public void LoadPlaces()
        {
            System.Diagnostics.Debug.WriteLine("loading places ----------");
            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    "Places.txt",
                    System.IO.FileMode.Open,
                    local))
                {
                    // Write the data from the textbox.
                    using (var isoFileReader = new System.IO.StreamReader(isoFileStream))
                    {
                        places = JsonConvert.DeserializeObject<Dictionary<String, Place> >(isoFileReader.ReadToEnd());
                        isoFileReader.Close();
                        
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            if (places == null)
                places = new Dictionary<string, Place>();
            if (places.Count == 0)
            {
                places.Add("Tehran", new Place("Tehran"));
                places.Add("Paris", new Place("Paris"));
            }
        }

        private WeatherDataManager()
        {
            LoadPlaces();
        }

        private Dictionary<String,Place> places = new Dictionary<string, Place>(); 
        public void AddPlace(Place place)
        {
            places.Add(place.Name,place);
        }

        public Dictionary<String, Place> getPlaces()
        {
            return places;
        }

        public void SetPlaces(ICollection<Place> newPlaces)
        {
            places = new Dictionary<string, Place>();
            foreach (Place newPlace in newPlaces)
            {
                places.Add(newPlace.Name, newPlace);
            }
        }

        public async void updateRequierdData(int days, AsyncCallback callback, AsyncCallback failCallback)
        {
            
            DateTime today=  DateTime.Now;
            LocalWeatherInput weatherInput = new LocalWeatherInput();
            weatherInput.num_of_days = days.ToString();
            foreach (KeyValuePair<string, Place> dictionaryEntry in places)
            {
                Place place =  dictionaryEntry.Value;
                if (place.LastUpdateTime > DateTime.Now.AddHours(-2))
                    continue;
                weatherInput.query = place.Name;
                SaveWeatherDataClass saver = new SaveWeatherDataClass(place.Name,callback);
                new FreeAPI().GetLocalWeather(weatherInput,saver.SaveWeatherData, failCallback);
                Thread.Sleep(1000);
            }
            
        }
        

        private class SaveWeatherDataClass
        {
            private string placeName { get; set; }
            private AsyncCallback callback;
            public SaveWeatherDataClass(string placeName)
            {
                this.placeName = placeName;
                callback = null;
            }

            public SaveWeatherDataClass( string placeName, AsyncCallback callback)
            {
                this.callback = callback;
                this.placeName = placeName;
            }

            public void SaveWeatherData(IAsyncResult result)
            {
                String callBackMessage = "unknownResult";
                try
                {
                    LocalWeather.LocalWeather localWeather = result.AsyncState as LocalWeather.LocalWeather;
                    // Get the local folder.
                    System.IO.IsolatedStorage.IsolatedStorageFile local =
                        System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

                    // Create a new folder named DataFolder.
                    if (!local.DirectoryExists("WeatherFolder"))
                        local.CreateDirectory("WeatherFolder");
                    using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                        "WeatherFolder\\" + placeName + ".txt",
                        System.IO.FileMode.Create,
                        local))
                    {
                        // Write the data from the textbox.
                        using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
                        {
                            string serializedWeather = JsonConvert.SerializeObject(localWeather);
                            isoFileWriter.Write(serializedWeather);
                            isoFileWriter.Close();
                            GetInstance().getPlaces()[placeName].LastUpdateTime = DateTime.Now;
                            GetInstance().SavePlaces();
                            callBackMessage = "weather updated for " + placeName;
//                        MessageBox.Show(serializedWeather);
                        }
                    }
                }
                catch (Exception e)
                {
                    callBackMessage = "update Failed For " + placeName;
                }
                if(callback!=null)
                    callback.Invoke(new Task(o => { },callBackMessage));
            }
        }

        public LocalWeather.LocalWeather getWeather(String place)
        {
            LocalWeather.LocalWeather ret = null;
            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    "WeatherFolder\\" + place + ".txt",
                    System.IO.FileMode.Open,
                    local))
                {
                    // Write the data from the textbox.
                    using (var isoFileReader = new System.IO.StreamReader(isoFileStream))
                    {
                        ret =JsonConvert.DeserializeObject<LocalWeather.LocalWeather>(isoFileReader.ReadToEnd());
                        isoFileReader.Close();
                        return ret;
                        //                        MessageBox.Show(serializedWeather);
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return null;
            }
        }
    }


    public class Place :INotifyPropertyChanged
    {
        
        public Boolean _useName { get; set; }
        private DateTime _lastUpdateTime;
        private Boolean _isLocal = false;
        private Double _latitude=0.0;
        private Double _longitude=0.0;
        private String _name="";
        public Place()
        {
        }

        public bool IsLocal
        {
            get { return _isLocal; }
            set
            {
                if (value.Equals(_isLocal)) return;
                _isLocal = value;
                NotifyPropertyChanged();
            }
        }

        public double Longitude
        {
            get { return _longitude; }
            set
            {
                if (value.Equals(_longitude)) return;
                _longitude = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("Lonitude");
            }
        }

        public DateTime LastUpdateTime
        {
            get { return _lastUpdateTime; }
            set
            {
                if (value.Equals(_lastUpdateTime)) return;
                _lastUpdateTime = value;
                NotifyPropertyChanged();
            }
        }

        public Place(string name)
        {
            _name = name;
            _useName = true;
            _latitude = _longitude = 0.0;
        }

        public Place(string name, double latitude, double longitude)
        {
            _name = name;
            _useName = false;
            _latitude = latitude;
            _longitude = longitude;
        }

        public double Latitude
        {
            get { return _latitude; }
            set
            {
                if (value.Equals(_latitude)) return;
                _latitude = value;
                NotifyPropertyChanged();
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
