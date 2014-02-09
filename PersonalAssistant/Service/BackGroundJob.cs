using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PersonalAssistant.Service.Weather;

namespace Inform_Me.src
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
