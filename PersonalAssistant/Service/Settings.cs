using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalAssistant
{
    class Settings
    {
        private static Settings _instance;

        public static Settings GetInstance()
        {
            if (_instance == null)
                _instance=new Settings();
            return _instance;
        }
        private int lastVersion = 1;
        private int Version;
        private Settings()
        {
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("Version", out Version);
            if (Version != lastVersion)
                fillDefaultValues();
            Load();
        }

        private void fillDefaultValues()
        {
            IsolatedStorageSettings.ApplicationSettings["MetricTemp"] = "Celsius";
            IsolatedStorageSettings.ApplicationSettings["APIKey"] = "";
            IsolatedStorageSettings.ApplicationSettings["Version"] = lastVersion;
        }

        private String _metricTemp;
        private string _apiKey;


        public string APIKey
        {
            get { return _apiKey; }
            set
            {
                _apiKey = value;
                IsolatedStorageSettings.ApplicationSettings["APIKey"] = value;
            }
        }

        public string MetricTemp
        {
            get { return _metricTemp; }
            set
            {
                _metricTemp = value;
                IsolatedStorageSettings.ApplicationSettings["MetricTemp"] = value;
            }
        }

        private void Load()
        {
            
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("MetricTemp", out _metricTemp);
            if (!_metricTemp.Any())
            {
                fillDefaultValues();
                Load();
            }
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("APIKey", out _apiKey);
        }



        static Settings()
        {
            ApplicationName = "Mosi";
        }

        public static string ApplicationName { get; set; }
        public static string UsersApplicationName { get; set; }
    }
}
