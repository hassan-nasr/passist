using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalAssistant.Service.Weather.LocalWeather;

namespace PersonalAssistant.Service.Weather
{
    public class WeatherDataManager
    {
        private Dictionary<String,Place> places = new Dictionary<string, Place>(); 
        public void AddPlace(Place place)
        {
            places.Add(place.name,place);
        }

        public Dictionary<String, Place> getPlaces()
        {
            return places;
        }

        public void updateRequierdData(int days, AsyncCallback callback, AsyncCallback failCallback)
        {
            DateTime today=  DateTime.Now;
            LocalWeatherInput weatherInput = new LocalWeatherInput();
            weatherInput.num_of_days = days.ToString();
            foreach (KeyValuePair<string, Place> dictionaryEntry in places)
            {
                string key = dictionaryEntry.Key;
                Place value =  dictionaryEntry.Value;
                weatherInput.query = value.name;
                SaveWeatherDataClass saver = new SaveWeatherDataClass(value.name,callback);
                new FreeAPI().GetLocalWeather(weatherInput,saver.SaveWeatherData, failCallback);
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


    public class Place
    {
        public Place(string name)
        {
            this.name = name;
        }

        public Place(string name, double latitude, double longitude)
        {
            this.name = name;
            this.latitude = latitude;
            this.longitude = longitude;
        }

        public String name { get; set; }
        public double latitude { get; set; }
        public double longitude { get; set; }
    }
}
