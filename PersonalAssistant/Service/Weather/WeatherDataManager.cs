//#define DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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

        private static string weatherfolderPath = "WeatherFolder";
        private static string placesFilePath = "Places.txt";
        private static string weatherIconPath = weatherfolderPath + "\\" + "Icons";

        public static String getLocalWeatherIconUri(String url)
        {
            String ret=  weatherIconPath + "\\" + (url.Substring(url.LastIndexOf("/") + 1));
            ret = ret.Replace('\\','/');
            return ret;
        }
        public static WeatherDataManager GetInstance()
        {
            if(_weatherDataManager == null)
                _weatherDataManager = new WeatherDataManager();
            return _weatherDataManager;
        }

        public void UpdatePlaces()
        {
            SavePlaces();
            updateRequierdData(5, dummyCallBack, dummyCallBack);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SavePlaces()
        {

//            MessageBox.Show("start saving places");
            try
            {
                // Get the local folder.
                System.IO.IsolatedStorage.IsolatedStorageFile local =
                    System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

                // Create a new folder named DataFolder.
                if (!local.DirectoryExists(weatherfolderPath))
                    local.CreateDirectory(weatherfolderPath);
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    placesFilePath,
                    System.IO.FileMode.Create,
                    local))
                {
                    using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
                    {
                        string serializedPlaces = JsonConvert.SerializeObject(places);
#if DEBUG
                        System.Diagnostics.Debug.WriteLine(serializedPlaces);
#endif
                        isoFileWriter.Write(serializedPlaces);
                        isoFileWriter.Close();
//                        MessageBox.Show("done saving places");
                    }
                }
                try
                {
                    if (VoiceCommandService.InstalledCommandSets.ContainsKey("en-us-1"))
                    {
                        VoiceCommandSet widgetVcs = VoiceCommandService.InstalledCommandSets["en-us-1"];
                        widgetVcs.UpdatePhraseListAsync("place", places.Keys);
                    }
                }
                catch (Exception e)
                {
                    BugReporter.GetInstance().report(e);
                }
            }
            catch (Exception e)
            {
                BugReporter.GetInstance().report(e);
//                throw e;
//                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
            
            
        }

        private void dummyCallBack(IAsyncResult ar)
        {
        }

        public void LoadPlaces()
        {
            System.IO.IsolatedStorage.IsolatedStorageFile local =
                System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                if (!IsolatedStorageFile.GetUserStoreForApplication().FileExists(placesFilePath))
                    DoFirstTimeJobs();
                using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                    placesFilePath,
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
                BugReporter.GetInstance().report(e);
            }
            if (places == null)
                places = new Dictionary<string, Place>();
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
            callback.Invoke(new Task((object obj) => { },
                        string.Format(" {0} place will be updated!", places.Count)));
            try
            {
                if (VoiceCommandService.InstalledCommandSets.ContainsKey("en-us-1"))
                {
                    VoiceCommandSet widgetVcs = VoiceCommandService.InstalledCommandSets["en-us-1"];
                    widgetVcs.UpdatePhraseListAsync("place", places.Keys);
                }
            }
            catch (Exception e)
            {
                BugReporter.GetInstance().report(e);
            }
            foreach (KeyValuePair<string, Place> dictionaryEntry in places)
            {
                Place place =  dictionaryEntry.Value;
                if (place.LastUpdateTime > DateTime.Now.AddHours(-2) && !Settings.GetInstance().APIKey.Any())
                {
                    callback.Invoke(new Task((object obj) => { },
                        "current weather data for " + place.Name + " is not outdated yet!"));
                    continue;
                }
                if (place._useName)
                    weatherInput.query = place.Name;
                else
                    weatherInput.query = place.Latitude + "," + place.Longitude;
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
                    if (localWeather.data == null || localWeather.data.request == null)
                        callBackMessage = "" + placeName + " - location not found.";
                    else
                    {
                        // Get the local folder.
                        System.IO.IsolatedStorage.IsolatedStorageFile local =
                            System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();

                        // Create a new folder named DataFolder.
                        if (!local.DirectoryExists(weatherfolderPath))
                            local.CreateDirectory(weatherfolderPath);
                        using (var isoFileStream = new System.IO.IsolatedStorage.IsolatedStorageFileStream(
                            weatherfolderPath+"\\" + placeName + ".txt",
                            System.IO.FileMode.Create,
                            local))
                        {
                            // Write the data from the textbox.
                            using (var isoFileWriter = new System.IO.StreamWriter(isoFileStream))
                            {
                                string serializedWeather = JsonConvert.SerializeObject(localWeather);
                                isoFileWriter.Write(serializedWeather);
                                isoFileWriter.Close();
                                Place place = GetInstance().getPlaces()[placeName];
                                place.enableNotify = false;
                                place.LastUpdateTime = DateTime.Now;
                                place.enableNotify = true;
//                                loadRequiredWeatherImages(localWeather);
                                GetInstance().SavePlaces();
                                callBackMessage = "weather updated for " + placeName;
//                        MessageBox.Show(serializedWeather);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    BugReporter.GetInstance().report(e);
                    callBackMessage = "update Failed For " + placeName;
                }
                if(callback!=null)
                    callback.Invoke(new Task(o => { },callBackMessage));
            }

            private void loadRequiredWeatherImages(LocalWeather.LocalWeather localWeather)
            {
                for (int i = 0; i < localWeather.data.current_Condition.Count; i++)
                {
                    WebRequestManager.GetInstance().loadIfNotExist(localWeather.data.current_Condition[i].weatherIconUrl[0].value, weatherIconPath);
                }
                for (int i = 0; i < localWeather.data.weather.Count; i++)
                {
                    WebRequestManager.GetInstance().loadIfNotExist(localWeather.data.weather[i].weatherIconUrl[0].value, weatherIconPath);
                }
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
                BugReporter.GetInstance().report(e);
                return null;
            }
        }

        public void DoFirstTimeJobs()
        {
            if (places.Count == 0)
            {
                places.Add("New York", new Place("New York"));
                places.Add("Paris", new Place("Paris"));
                places.Add("Tehran", new Place("Tehran"));
                places["New York"].IsLocal = true;
            }
            UpdatePlaces();
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
            if (enableNotify == false)
                return;
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Boolean enableNotify = true;
    }
}
