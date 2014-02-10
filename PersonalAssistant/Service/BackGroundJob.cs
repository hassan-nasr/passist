using System;
using PersonalAssistant.Service.Weather;

namespace PersonalAssistant.Service
{
    class BackGroundJob
    {
        public void doJobs(AsyncCallback callback)
        {
            WeatherDataManager weatherDataManager = new WeatherDataManager();
            weatherDataManager.AddPlace(new Place("Tehran"));
            weatherDataManager.AddPlace(new Place("Paris"));
            weatherDataManager.updateRequierdData(5,callback, callback);
        }
    }
}
