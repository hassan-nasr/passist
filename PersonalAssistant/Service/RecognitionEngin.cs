using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using Windows.Foundation;
using Windows.Phone.Devices.Power;
using Windows.Phone.Speech.Synthesis;
using Windows.UI.Input;
using Microsoft.Phone.UserData;
using PersonalAssistant.Service.Weather;
using PersonalAssistant.Service.Weather.LocalWeather;
using PersonalAssistant.ViewModels;

namespace Inform_Me.src
{
    class RecognitionEngin
    {
        private AsyncCallback onFinish;
        private AsyncCallback sendViewableResult;
        RecentItem RecentItem = new RecentItem();
        public void RespondToQuery(IDictionary<string, string> queryString, AsyncCallback onFinishIn, AsyncCallback sendViewableResult)
        {
            this.onFinish = onFinishIn;
            this.sendViewableResult = sendViewableResult;
            // If so, get the name of the voice command.
            string voiceCommandName
                = queryString["voiceCommandName"];

            // Define app actions for each voice command name.
            switch (voiceCommandName)
            {
                case "time":
                    DateTime t = DateTime.Now;
                    Saytime(t);
                    //       Exit();
                    break;
                case "date":
                    String type = queryString["CalType"];
                    SayDate(type, new DateTime());
                    //       Exit();
                    break;
                case "battery":
                    SayBattery(null, null);
                    //       Exit();
                    break;
                case "appointments":
                    SayAppintments();
                    break;
                case "localWeather":
                    {
                        String day = queryString["day"];
                        String place = "Tehran";
                        combineAndSayWeather(day, place);
                    }
                    break;
                case "weather":
                    {
                        String day = queryString["day"];
                        String place = queryString["place"];
                        combineAndSayWeather(day, place);
                    }
                    break;
                default:
                    break;
            }

        }

        private void combineAndSayWeather(String day, String place)
        {
            DateTime now = DateTime.Now;
            now = goToNextValidDate(now, day);
            SayWeather(now, place, day);
        }

        private DateTime goToNextValidDate(DateTime now, string day)
        {
            if (day == "today")
                return now;
            if (day == "tomorrow")
                return now.AddDays(1);
            while (!now.DayOfWeek.ToString().Equals(day))
                now = now.AddDays(1);
            return now;

        }

        private async void SayWeather(DateTime date, string place, String orginalDateString)
        {
            WeatherDataManager weatherDataManager = new WeatherDataManager();

            String responseSentence = "";
            LocalWeather localWeather = weatherDataManager.getWeather(place);
            if (localWeather == null)
            {
                responseSentence = "sorry! but weather data for " + place + " is not available. please add " + place +
                                   " to your weather checkout list";
            }
            else
            {
                Weather weatherToShow = null;
                for (int i = 0; i < localWeather.data.weather.Count; i++)
                {
                    Weather weather = localWeather.data.weather[i];
                    if (weather.date.Date.Equals(date.Date))
                    {
                        weatherToShow = weather;
                        break;
                    }

                }
                String dateString = weatherToShow.date.Date.ToLongDateString();
                dateString = dateString.Substring(0, dateString.Length - 4);
                if (weatherToShow == null)
                {
                    responseSentence = "sorry! but weather data for " + place + " on " + dateString +
                                       " is not available";
                }
                else
                {

                    responseSentence = "it's " + weatherToShow.weatherDesc[0].value + " at " + place + " on " +
                                       dateString + " , the Minimum of  Temperature is " +
                                       weatherToShow.tempMinC + " and the Maximum is " + weatherToShow.tempMaxC +
                                       " degrees of Celsius.";
                }
            }
            RecentItem.ImageUri = WeatherImageUri;
            RecentItem.ResponseString = responseSentence;
            sendViewableResult.Invoke(new Task(o => { },RecentItem));
            SpeechSynthesizer synth = new SpeechSynthesizer();
            await synth.SpeakTextAsync(responseSentence);
            onFinish.Invoke(new Task(o=>{},"Have Fun"));

        }

        public String WeatherImageUri ="/Images/feature.search.png";
        public String TimeImageUri ="/Images/feature.alarm.png";
        public String DateImageUri ="/Images/feature.calendar.png";
        public String AppointmentImageUri ="/Images/feature.calendar.png";
        public String BatteryImageUri = "/Images/feature.settings.png";

        private async void Saytime(DateTime time)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            String timeToSay = "";
            if (time.Hour == 0 && time.Minute == 0)
                timeToSay = "it's exactly midnight";
            else if (time.Hour == 0 && time.Minute <= 30)
                timeToSay = "it's " + time.Minute + " minutes past Midnight";
            else if (time.Hour == 23 && time.Minute > 30)
                timeToSay = "it's " + (60 - time.Minute) + " minutes to Midnight";
            else if (time.Minute == 0)
                timeToSay = "it's " + time.Hour % 12 + " o clock in the " + (time.Hour / 12 > 1 ? "evening" : "morning");
            else
                timeToSay = "it's " + (time.Hour % 12 + (time.Hour == 12 ? 1 : 0) * 12) + " " + time.Minute + " in the " + (time.Hour / 12.0 >= 1 ? "evening" : "morning");
            RecentItem.ImageUri = TimeImageUri;
            RecentItem.ResponseString = timeToSay;
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await synth.SpeakTextAsync(timeToSay);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }
        private async void SayDate(String type, DateTime date)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            String sentence = "";
            switch (type)
            {
                case "Gregorian":
                    sentence = "it's " + date.ToShortDateString();
                    break;
                case "Hejri":
                    //                    System.Globalization.PersianCalendar p = new System.Globalization.PersianCalendar();
                    //                    string cal = hc.ToString();
                    sentence = ("it's um please wait for next version!");
                    //                    MessageBox.Show(cal);
                    break;
                default:
                    break;

            }
            RecentItem.ImageUri = DateImageUri;
            RecentItem.ResponseString = sentence;
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await synth.SpeakTextAsync(sentence);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }

        private async void SayBattery(object sender, RoutedEventArgs e)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            int battery = Battery.GetDefault().RemainingChargePercent;
            String sentence = "you phone has " + battery + "% of battery remaining.";
            RecentItem.ImageUri = BatteryImageUri;
            RecentItem.ResponseString = sentence;
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await synth.SpeakTextAsync(sentence);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }

        private async void SayAppintments()
        {
            Appointments appo = new Appointments();
            appo.SearchCompleted += finishSearch;
            appo.SearchAsync(DateTime.Now, DateTime.Now.Add(new TimeSpan(7, 0, 0, 0)), 1, null);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }
        private async void finishSearch(object sender, AppointmentsSearchEventArgs e)
        {
            List<Appointment> appointments = e.Results.ToList();
            SpeechSynthesizer synth = new SpeechSynthesizer();
            if (appointments.Count == 0)
            {
                await synth.SpeakTextAsync("you have no appointments in next weak! have fun.");
            }
            else
            {
                Appointment app = appointments[0];
                string response;
                string TimeString;
                if (app.StartTime.Date.Equals(DateTime.Now.Date))
                    TimeString = "today";
                else if (app.StartTime.Date.Equals(DateTime.Now.Add(new TimeSpan(1, 0, 0, 0)).Date))
                    TimeString = "tomarow";
                else
                    TimeString = "on " + app.StartTime.DayOfWeek.ToString();
                if (!app.IsAllDayEvent)
                {
                    TimeString += " at " + app.StartTime.ToShortTimeString();
                }
                if (!app.IsPrivate)
                {
                    response = "you have an appointment about " + app.Subject + " " + TimeString;
                    if (app.Location != null)
                        response += " in " + app.Location;
                }
                else
                    response = "you have a private appointment " + TimeString;
                RecentItem.ImageUri = BatteryImageUri;
                RecentItem.ResponseString = response;
                sendViewableResult.Invoke(new Task(o => { }, RecentItem));
                await synth.SpeakTextAsync(response);
                onFinish.Invoke(new Task(o => { }, "Have Fun"));
            }
            if (onFinish != null)
            {
                onFinish.Invoke(new Task(o => { },null));
            }
        }

    }
}
