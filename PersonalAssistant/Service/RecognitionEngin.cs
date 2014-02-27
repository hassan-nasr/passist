using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Phone.Devices.Power;
using Windows.Phone.Speech.Synthesis;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.UserData;
using PersonalAssistant.Service.appointment;
using PersonalAssistant.Service.Weather;
using PersonalAssistant.Service.Weather.LocalWeather;
using PersonalAssistant.ViewModels;

namespace PersonalAssistant.Service
{
    class RecognitionEngin
    {
        private AsyncCallback onFinish;
        private AsyncCallback sendViewableResult;
        ResponseItem RecentItem  = null;
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
                    SayDate(type,  DateTime.Now);
                    //       Exit();
                    break;
                case "date2":
                    type = "Gregorian";
                    SayDate(type,  DateTime.Now);
                    //       Exit();
                    break;
                case "battery":
                    SayBattery(null, null);
                    //       Exit();
                    break;
                case "appointments":
                    SayAppintments();
                    break;
                case "findAppointments":
                    String subject = queryString["appointment"];
                    new AppointmentsManager().FindAppointmentByName(subject, SayFoundAppointment);
                    ;
                    break;
                case "localWeather":
                    {
                        String day = queryString["day"];
                        String place = "[current]";
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
                case "currentWeather":
                    {
                        String day = "today";
                        String place = queryString["place"];
                        combineAndSayWeather(day, place);
                    }
                    break;
                case "currentLocalWeather":
                    {
                        String day = "today";
                        String place = "[current]";
                        combineAndSayWeather(day, place);
                    }
                    break;
                case "alarmTimeSet":
                    {
                        int hourInt,minuteInt;
                        String hour = queryString["hour"];
                        hourInt = int.Parse(hour);
                        String minute = queryString["minute"];
                        if (minute.Equals("o clock"))
                            minuteInt = 0;
                        else
                        {
                            minuteInt = int.Parse(minute);
                        }
                        string notify = queryString["alarmf"];
                        String ampm = queryString["ampm"];
                        if (ampm.Equals("p.m."))
                            hourInt += 12;
                        SetAlarmAt(hourInt, minuteInt , notify);
                    }
                    break;
                case "relativeAlarmTimeSetHourMinute":
                case "relativeAlarmTimeSetHour":
                case "relativeAlarmTimeSetMinute":
                {
                    int hourInt,minuteInt;
                        String hour = queryString.ContainsKey("number") ? queryString["number"] : "0";
                        String minute = queryString.ContainsKey("number2") ? queryString["number2"] : "0";
                        switch (minute)
                        {
                            case "a":
                            case "an":
                                minuteInt = 1;
                                break;
                            case"half an":
                            case"halfan":
                            case"halfa":
                            case"half a":
                            case"half":
                                minuteInt = 1;
                                break;
                            default:
                                minuteInt = int.Parse(minute);
                                break;
                        }
                        switch (hour)
                        {
                            case "a":
                            case "an":
                                hourInt = 1;
                                break;
                            case "half an":
                            case "halfan":
                            case "halfa":
                            case "half a":
                            case "half":
                                hourInt = 0;
                                minuteInt = 30;
                                break;
                            default:
                                hourInt = int.Parse(hour);
                                break;
                        }
                        
                        string notify = queryString["alarmf"];
                        System.Diagnostics.Debug.WriteLine(hourInt + " : " + minuteInt);
                        System.Diagnostics.Debug.WriteLine(hour + " : " + minute);
                        SetAlarmIn(hourInt, minuteInt , notify);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void SetAlarmAt(int hour, int minute , string notification)
        {
            DateTime dateTime = DateTime.Now;
            if (dateTime.Hour > hour || (dateTime.Hour == hour && dateTime.Minute > minute))
                dateTime = dateTime.AddDays(1);
            dateTime =new DateTime(dateTime.Year,dateTime.Month,dateTime.Day,hour,minute,0);
            await createAlarm(notification, dateTime);
        }
        private async void SetAlarmIn(int hour, int minute , string notification)
        {
            DateTime dateTime = DateTime.Now;
            dateTime = dateTime.AddHours(hour);
            dateTime = dateTime.AddMinutes(minute);
            await createAlarm(notification, dateTime);
        }

        private async Task createAlarm(string notification, DateTime dateTime)
        {
            Alarm alarm = new Alarm(Settings.ApplicationName+DateTime.Now);
            alarm.Content = notification;
            alarm.BeginTime = dateTime;
            alarm.ExpirationTime = alarm.BeginTime.AddSeconds(5.0);
            alarm.RecurrenceType = RecurrenceInterval.None;
            ScheduledActionService.Add(alarm);
            SpeechSynthesizer synth = new SpeechSynthesizer();
            int battery = Battery.GetDefault().RemainingChargePercent;
            String sentence = "alarm created for " + SayTime(dateTime);
            if (notification.Equals("wake me up"))
                sentence += ". Have a good sleep!";
            String detailsString = "alarm created for " + dateTime.ToString();
            RecentItem = new ResponseItem(AlarmImageUri, detailsString, sentence);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await synth.SpeakTextAsync(sentence);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
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
            WeatherDataManager weatherDataManager = WeatherDataManager.GetInstance();
            String imageuri = WeatherImageUri;
            if (place == "[current]")
            {
                Dictionary<string, Place> dictionaryEntries = WeatherDataManager.GetInstance().getPlaces();
                if (dictionaryEntries.Count != 0)
                {
                    place = dictionaryEntries.GetEnumerator().Current.Key;

                    foreach (KeyValuePair<string, Place> dictionaryEntry in dictionaryEntries)
                    {
                        string key = (string)dictionaryEntry.Key;
                        Place value = dictionaryEntry.Value;
                        if (value.IsLocal)
                            place = key;
                    }    
                }
                if (place == "[current]")
                {
                    SpeechSynthesizer synth2 = new SpeechSynthesizer();
                    await synth2.SpeakTextAsync("please set you local location in settings page");
                    return;
                }

                
            }
            String responseSentence = "";
            String detailsString = "";
            LocalWeather localWeather = weatherDataManager.getWeather(place);
            if (localWeather == null)
            {
                responseSentence = "sorry! but weather data for " + place + " is not available. please add " + place +
                                   " to your weather checkout list";
                detailsString = "Sorry! No data :[";
            }
            else
            {
                Weather.LocalWeather.Weather weatherToShow = null;
                for (int i = 0; i < localWeather.data.weather.Count; i++)
                {
                    Weather.LocalWeather.Weather weather = localWeather.data.weather[i];
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
                    detailsString = "Sorry! No data :[";
                }
                else
                {

                    responseSentence = "it's " + weatherToShow.weatherDesc[0].value + " at " + place + " on " +
                                       dateString + " the Minimum of  Temperature is " +
                                       GetTemp(weatherToShow.tempMinC, "none") + " and the Maximum is " +
                                       GetTemp(weatherToShow.tempMaxC, "long") +
                                       ".";
                    detailsString = "Location : " + place
                                    + "\r\nDescription: " + weatherToShow.weatherDesc[0].value
                                    + "\r\nMin Temp.: " + GetTemp(weatherToShow.tempMinC ,"short")
                                    + "\r\nMax Temp.: " + GetTemp(weatherToShow.tempMaxC, "short")
                                    + "\r\nWind Speed: " + weatherToShow.windspeedKmph + " Km/h"
                                    + "\r\nWind Degree: " + weatherToShow.winddirDegree;

                }

                imageuri =(WeatherImageUri+"/"+weatherToShow.weatherCode+".png");

            }
            RecentItem = new ResponseItem(imageuri,detailsString,responseSentence);
            sendViewableResult.Invoke(new Task(o => { },RecentItem));
            SpeechSynthesizer synth = new SpeechSynthesizer();
            await synth.SpeakTextAsync(responseSentence);
            onFinish.Invoke(new Task(o=>{},"Have Fun"));

        }
        public String WeatherImageUri ="/Images/WeatherIcon";
        public String TimeImageUri ="/Images/feature.alarm.png";
        public String AlarmImageUri ="/Images/feature.alarm.png";
        public String DateImageUri ="/Images/feature.calendar.png";
        public String AppointmentImageUri ="/Images/feature.calendar.png";
        public String BatteryImageUri = "/Images/feature.settings.png";

        private async void Saytime(DateTime time)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            String timeToSay = SayTime(time);
            string detailsString = time.ToLongTimeString();
            RecentItem= new ResponseItem(TimeImageUri,detailsString,timeToSay);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await synth.SpeakTextAsync(timeToSay);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }

        private static string SayTime(DateTime time)
        {
            String timeToSay = "";
            if (time.Hour == 0 && time.Minute == 0)
                timeToSay = "it's exactly midnight";
            else if (time.Hour == 0 && time.Minute <= 30)
                timeToSay = "it's " + time.Minute + " minutes past Midnight";
            else if (time.Hour == 23 && time.Minute > 30)
                timeToSay = "it's " + (60 - time.Minute) + " minutes to Midnight";
            else if (time.Minute == 0)
                timeToSay = "it's " + time.Hour%12 + " o clock in the " + (time.Hour/12 > 1 ? "evening" : "morning");
            else
                timeToSay = "it's " + (time.Hour%12 + (time.Hour == 12 ? 1 : 0)*12) + " " + time.Minute + " in the " +
                            (time.Hour/12.0 >= 1 ? "evening" : "morning");
            return timeToSay;
        }

        private async void SayDate(String type, DateTime date)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            String sentence = "";
            String detailsString = "";
            CultureInfo responseCultureInfo = new CultureInfo("fa-IR");
//            date.ToString("D",)
            String dateString = " an unsupported calendar type. sorry!";
            switch (type)
            {
                case "Gregorian":
                    dateString = date.ToLongDateString();
                    break;
                case "Hejri":

                    dateString = GetDateString(date, new CultureInfo("en-US"));
                    //                    System.Globalization.PersianCalendar p = new System.Globalization.PersianCalendar();
                    //                    string cal = hc.ToString();
//                    sentence = ("it's um please wait for next version!");
                    //                    MessageBox.Show(cal);
                    break;
                default:
                    break;

            }
            sentence = "it's " + dateString;
            detailsString = dateString;
            
            RecentItem = new ResponseItem(DateImageUri, detailsString, sentence);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await synth.SpeakTextAsync(sentence);
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }

        public String GetDateString(DateTime d, CultureInfo cultureInfo)
        {
            Calendar calendar = cultureInfo.Calendar;
            int year =calendar.GetYear(d);
            int month = calendar.GetMonth(d);
            int day = calendar.GetDayOfMonth(d);
            String s = "" + day + " " + cultureInfo.DateTimeFormat.GetMonthName(month) + " " + year;
            return s;
        }

        private async void SayBattery(object sender, RoutedEventArgs e)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            int battery = Battery.GetDefault().RemainingChargePercent;
            String sentence = "you phone has " + battery + "% of battery remaining.";
            String detailsString = battery + "% remaining" ;
            RecentItem = new ResponseItem(BatteryImageUri, detailsString, sentence);
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

            Appointment app=null;
            if(appointments.Count>0)
                app = appointments[0];

            await SayAppointment(app);
        }

        private async Task SayAppointment(Appointment app)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            String detailsString, response;
            if (app == null)
            {
                response = ("You have no appointments in next weak! have fun.");
                detailsString = "no appointments in next weak";
            }
            else
            {
                string TimeString;
                if (app.StartTime.Date.Equals(DateTime.Now.Date))
                    TimeString = "today";
                else if (app.StartTime.Date.Equals(DateTime.Now.Add(new TimeSpan(1, 0, 0, 0)).Date))
                    TimeString = "tomarow";
                else
                    TimeString = "on " + app.StartTime.DayOfWeek.ToString();
                if (!app.IsAllDayEvent)
                {
                    if (!app.StartTime.Equals(DateTime.Now.Date) || !app.EndTime.Equals(DateTime.Now.Date.AddDays(1)))
                        TimeString += " at " + app.StartTime.ToShortTimeString();
                }
                if (!app.IsPrivate)
                {
                    response = "You have an appointment about " + app.Subject + " " + TimeString;
                    if (app.Location != null)
                        response += " in " + app.Location;
                    detailsString = "Subject: " + app.Subject
                                    + "\r\nFrom: " + app.StartTime.ToLongDateString() + "," +
                                    app.StartTime.ToLongTimeString()
                                    + "\r\nTo: " + app.EndTime.ToLongDateString() + "," +
                                    app.EndTime.ToLongTimeString()
                                    + "\r\nLocation: " + app.Location
                                    + "\r\nDetails:" + app.Details;
                }
                else
                {
                    response = "you have a private appointment " + TimeString;
                    detailsString = "Subject: private"
                                    + "\r\nFrom: " + app.StartTime.ToLongDateString() + "," +
                                    app.StartTime.ToLongTimeString()
                                    + "\r\nTo: " + app.EndTime.ToLongDateString() + "," +
                                    app.EndTime.ToLongTimeString()
                                    + "\r\nLocation: private "
                                    + "\r\nDetails: private";
                }
                RecentItem = new ResponseItem(AppointmentImageUri, detailsString, response);
                sendViewableResult.Invoke(new Task(o => { }, RecentItem));
                await synth.SpeakTextAsync(response);
                onFinish.Invoke(new Task(o => { }, "Have Fun"));
            }
            if (onFinish != null)
            {
                onFinish.Invoke(new Task(o => { }, null));
            }
        }

        private async void SayFoundAppointment(IAsyncResult result)
        {
            SpeechSynthesizer synth = new SpeechSynthesizer();
            String detailsString = "", response="";
            Appointment app = result.AsyncState as Appointment;
            if (app == null)
            {
                response = ("You have no such appointment or event in next Year.");
                detailsString = "no appointment maches";
            }
            else
            {
                string TimeString;
                if (app.StartTime.Date.Equals(DateTime.Now.Date))
                    TimeString = "today";
                else if (app.StartTime.Date.Equals(DateTime.Now.Add(new TimeSpan(1, 0, 0, 0)).Date))
                    TimeString = "tomarow";
                else
                    TimeString = "on " + app.StartTime.ToShortDateString();
                if (!app.IsAllDayEvent)
                {
                    if (!app.StartTime.Equals(DateTime.Now.Date) || !app.EndTime.Equals(DateTime.Now.Date.AddDays(1)))
                        TimeString += " at " + app.StartTime.ToShortTimeString();
                }
                if (!app.IsPrivate)
                {
                    response = "You appointment about " + app.Subject + " is " + TimeString;
                    if (app.Location != null)
                        response += " in " + app.Location;
                    detailsString = "Subject: " + app.Subject
                                    + "\r\nFrom: " + app.StartTime.ToLongDateString() + "," +
                                    app.StartTime.ToLongTimeString()
                                    + "\r\nTo: " + app.EndTime.ToLongDateString() + "," +
                                    app.EndTime.ToLongTimeString()
                                    + "\r\nLocation: " + app.Location
                                    + "\r\nDetails:" + app.Details;
                }
                RecentItem = new ResponseItem(AppointmentImageUri, detailsString, response);
                sendViewableResult.Invoke(new Task(o => { }, RecentItem));
                await synth.SpeakTextAsync(response);
                onFinish.Invoke(new Task(o => { }, "Have Fun"));
            }
            if (onFinish != null)
            {
                onFinish.Invoke(new Task(o => { }, null));
            }
        }

        private static String GetTemp(int incels, String type)
        {
            if (Settings.GetInstance().MetricTemp == "Fahrenheit")
            {
                int inFaren = (int) (incels*9.0/5 + 32);

                if (type == "none")
                {
                    return "" + inFaren;
                }
                if (type == "short")
                {
                    return "" + inFaren + " °F";
                }
                return "" + inFaren + " degrees of Fahrenheit";
            }
            if (type == "none")
            {
                return "" + incels;
            }
            if (type == "short")
            {
                return "" + incels  +" °C";
            }
            return "" + incels + " degrees of Celsius";
        }

    }


}
