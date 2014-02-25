using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;
using PersonalAssistant.Service.appointment;
using PersonalAssistant.Service.Weather;

namespace PersonalAssistant.Service
{
    class BackGroundJob
    {
        private static BackGroundJob instance = null;

        private BackGroundJob() { }

        public static BackGroundJob GetInstance()
        {
            if (instance == null)
                instance = new BackGroundJob();
            return instance;
        }
        public async Task doJobs(AsyncCallback callback)
        {
            System.Diagnostics.Debug.WriteLine("inJobs");
            WeatherDataManager weatherDataManager =  WeatherDataManager.GetInstance();
//            weatherDataManager.AddPlace(new Place("Tehran"));
//            weatherDataManager.AddPlace(new Place("Paris"));
            weatherDataManager.updateRequierdData(5,callback, callback);

            new AppointmentsManager().FillSpeachDataWithAppointments();
        }
    }
}
