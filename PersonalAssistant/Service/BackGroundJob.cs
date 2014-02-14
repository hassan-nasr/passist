using System;
using Microsoft.Xna.Framework.Audio;
using PersonalAssistant.Service.Weather;

namespace PersonalAssistant.Service
{
    class BackGroundJob
    {
        private BackGroundJob instance = null;

        public BackGroundJob GetInstance()
        {
            if (instance == null)
                instance = new BackGroundJob();
            return instance;
        }
        public async void doJobs(AsyncCallback callback)
        {
            WeatherDataManager weatherDataManager =  WeatherDataManager.GetInstance();
//            weatherDataManager.AddPlace(new Place("Tehran"));
//            weatherDataManager.AddPlace(new Place("Paris"));
            weatherDataManager.updateRequierdData(5,callback, callback);
        }
    }
}
