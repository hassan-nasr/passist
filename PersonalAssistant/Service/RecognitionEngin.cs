﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Windows.Phone.Devices.Power;
using Windows.Phone.Speech.Synthesis;
using Microsoft.Phone.Maps.Services;
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
        public SpeechSynthesizer SpeechSynthesizer { get; set; }
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
                        SetAlarmIn(hourInt, minuteInt , notify);
                    }
                    break;
                case "actionReminder_contact_day_dayPart":
                    {
                        String reminderAction = queryString["reminderAction"];
                        String contactName = queryString["contactLoos"];
                        String day = queryString.ContainsKey("day")?queryString["day"]:"today";
                        String dayPart = queryString.ContainsKey("dayPart")?queryString["dayPart"]:"morning";
                        createReminder(reminderAction, contactName, day, dayPart);
                    }
                    break;
                case "actionReminder_contact_date":
                    {
                        String reminderAction = queryString["reminderAction"];
                        String contactName = queryString["contactLoos"];
                        String month = queryString.ContainsKey("month")?queryString["month"]:DateTime.Now.ToString("MMMM");
                        String day = queryString.ContainsKey("number") ? queryString["number"] : "1";
                        String dayPart = queryString.ContainsKey("dayPart") ? queryString["dayPart"] : "morning";
                        createReminderByMonth(reminderAction, contactName, month, day,dayPart);
                    }
                    break;
                default:
                    break;
            }

        }

        private async void createReminder(string reminderAction, string contactName, string day, string dayPart)
        {
            DateTime reminderTime = goToNextValidDate(DateTime.Now, day);
            reminderTime = reminderTime.Date;
            reminderTime = addNotAcurateDayTime(reminderTime, dayPart);
            await CreateReminderAndRespond(reminderAction, contactName, reminderTime);
        }
        private async void createReminderByMonth(string reminderAction, string contactName, string month, string day,string dayPart)
        {
            DateTime reminderTime = DateTime.Parse(month + " " + day + ", " + DateTime.Now.Year);
            reminderTime = addNotAcurateDayTime(reminderTime, dayPart);
            if (reminderTime.Ticks < DateTime.Now.AddMinutes(2).Ticks)
                reminderTime = reminderTime.AddYears(1);
            await CreateReminderAndRespond(reminderAction, contactName, reminderTime);
        }

        private async Task CreateReminderAndRespond(string reminderAction, string contactName, DateTime reminderTime)
        {
//            SpeechSynthesizer synth = new SpeechSynthesizer();
            string response;
            if (reminderTime.Ticks < DateTime.Now.AddMinutes(2).Ticks)
            {
                response = "sorry. the reminder should be at least in one minuets from now!";
                try
                {
                    await SpeechSynthesizer.SpeakTextAsync(response);
                }
                catch (TaskCanceledException) { }
                onFinish.Invoke(new Task(o => { }, "Have Fun"));
                return;
            }
            String reminderContent = reminderAction + " " + contactName;
            try
            {
                createReminder("reminder", reminderAction + " " + contactName, reminderTime);
            }
            catch (Exception e)
            {
                response = "sorry. couldn't create the reminder! please check if you have disabled background agenst or maybe I have reached the number of allowd reminder and or alerts!";
                try
                {
                    SpeechSynthesizer.SpeakTextAsync(response);
                }
                catch (TaskCanceledException) { }
                onFinish.Invoke(new Task(o => { }, "Have Fun"));
                return;
            }
            response = "reminder created for " + reminderTime.DayOfWeek + " " + SayTime(reminderTime);
            String detailsString = "remindier: " + reminderAction + " " + contactName + "\n\r" + "Date: " +
                                   reminderTime.ToLongDateString()
                                   + "\n\r" + "Time: " + reminderTime.ToLongTimeString();
            RecentItem = new ResponseItem(ReminderImageUri, detailsString, response);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            try
            {
                await SpeechSynthesizer.SpeakTextAsync(response);
            }
            catch (TaskCanceledException) { }
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }

        private String createReminder(string title, string content, DateTime reminderTime)
        {
            Reminder reminder = new Reminder(content + reminderTime.Ticks);
            reminder.BeginTime = reminderTime;
            reminder.Title = title;
            reminder.Content = content;
            ScheduledActionService.Add(reminder);
            return reminder.Name;
        }

        private DateTime addNotAcurateDayTime(DateTime reminderTime, string dayPart)
        {
            switch (dayPart)
            {
                case "morning":
                    return reminderTime.AddHours(8);
                case "noon":
                    return reminderTime.AddHours(12);
                case "afternoon":
                    return reminderTime.AddHours(16);
                case "evening":
                    return reminderTime.AddHours(20);
                case "midnight":
                    return reminderTime.AddHours(24);
                default:
                    return reminderTime.AddHours(12);
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
//            SpeechSynthesizer synth = new SpeechSynthesizer();
            int battery = Battery.GetDefault().RemainingChargePercent;
            String sentence = "alarm created for " + SayTime(dateTime);
            if (notification.Equals("wake me up"))
                sentence += ". Have a good sleep!";
            String detailsString = "alarm created for " + dateTime.ToString();
            RecentItem = new ResponseItem(AlarmImageUri, detailsString, sentence);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            await SpeechSynthesizer.SpeakTextAsync(sentence);
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
//                    SpeechSynthesizer synth = new SpeechSynthesizer();
                    try
                    {
                        await SpeechSynthesizer.SpeakTextAsync("please set your local location in settings page");
                    }
                    catch (TaskCanceledException e)
                    {
                        
                    }
                    return;
                }

                
            }
            String responseSentence = "";
            String detailsString = "";
            LocalWeather localWeather = weatherDataManager.getWeather(place);
            if (localWeather == null)
            {
                responseSentence = "sorry! but weather data for " + place + " is not available.";
                detailsString = "Sorry! No data :[\r\n" + "please add " + place +
                                   " to your weather checkout list and make sure you connect your phone to internet every few days you can Manually update weather data from App bar";
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

                String dateString = date.ToLongDateString();
                dateString = dateString.Substring(0, dateString.Length - 4);
                if (weatherToShow == null)
                {
                    responseSentence = "sorry! but weather data for " + place + " on " + dateString +
                                       " is not available";
                    detailsString = "Sorry! No data :[";
                    imageuri = WeatherImageUri + "/" + "sync.png";
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

                    imageuri = (WeatherImageUri + "/" + weatherToShow.weatherCode + ".png");

                }

            }
            RecentItem = new ResponseItem(imageuri,detailsString,responseSentence);
            sendViewableResult.Invoke(new Task(o => { },RecentItem));
//            SpeechSynthesizer synth = new SpeechSynthesizer();
            try
            {
                await SpeechSynthesizer.SpeakTextAsync(responseSentence);
            }
            catch (TaskCanceledException e) { }
            onFinish.Invoke(new Task(o=>{},"Have Fun"));

        }
        public String WeatherImageUri ="/Images/WeatherIcon";
        public String TimeImageUri ="/Images/feature.alarm.png";
        public String AlarmImageUri ="/Images/feature.alarm.png";
        public String ReminderImageUri = "/Images/feature.alarm.png";
        public String DateImageUri ="/Images/feature.calendar.png";
        public String AppointmentImageUri ="/Images/feature.calendar.png";
        public String BatteryImageUri = "/Images/feature.settings.png";

        public RecognitionEngin()
        {
            SpeechSynthesizer = new SpeechSynthesizer();
        }

        private async void Saytime(DateTime time)
        {
//            SpeechSynthesizer synth = new SpeechSynthesizer();
            String response = "it's "+SayTime(time);
            string detailsString = time.ToLongTimeString();
            RecentItem= new ResponseItem(TimeImageUri,detailsString,response);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            try
            {
                await SpeechSynthesizer.SpeakTextAsync(response);
            }
            catch (TaskCanceledException) { }
            onFinish.Invoke(new Task(o => { }, "Have Fun"));
        }

        private static string SayTime(DateTime time)
        {
            String timeToSay = "";
            if (time.Hour == 0 && time.Minute == 0)
                timeToSay = "exactly midnight";
            else if (time.Hour == 0 && time.Minute <= 30)
                timeToSay = "" + time.Minute + " minutes past Midnight";
            else if (time.Hour == 23 && time.Minute > 30)
                timeToSay = "" + (60 - time.Minute) + " minutes to Midnight";
            else if (time.Minute == 0)
                timeToSay = "" + time.Hour%12 + " o clock in the " + (time.Hour/12.0 > 1 ? "evening" : "morning");
            else
                timeToSay = "" + (time.Hour%12 + (time.Hour == 12 ? 1 : 0)*12) + " " + time.Minute + " in the " +
                            (time.Hour/12.0 >= 1 ? "evening" : "morning");
            return timeToSay;
        }

        private async void SayDate(String type, DateTime date)
        {
//            SpeechSynthesizer synth = new SpeechSynthesizer();
            String response = "";
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
            response = "it's " + dateString;
            detailsString = dateString;
            
            RecentItem = new ResponseItem(DateImageUri, detailsString, response);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            try
            {
                await SpeechSynthesizer.SpeakTextAsync(response);
            }
            catch (TaskCanceledException) { }
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
//            SpeechSynthesizer synth = new SpeechSynthesizer();
            int battery = Battery.GetDefault().RemainingChargePercent;
            String sentence = string.Format("your phone has {0}% of battery remaining.", battery);
            String detailsString = battery + "% remaining" ;
            RecentItem = new ResponseItem(BatteryImageUri, detailsString, sentence);
            sendViewableResult.Invoke(new Task(o => { }, RecentItem));
            try
            {
                await SpeechSynthesizer.SpeakTextAsync(sentence);
            }
            catch (TaskCanceledException){}
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
//            SpeechSynthesizer synth = new SpeechSynthesizer();
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
                try
                {
                    await SpeechSynthesizer.SpeakTextAsync(response);
                }
                catch (TaskCanceledException) { }
                onFinish.Invoke(new Task(o => { }, "Have Fun"));
            }
            if (onFinish != null)
            {
                onFinish.Invoke(new Task(o => { }, null));
            }
        }

        private async void SayFoundAppointment(IAsyncResult result)
        {
//            SpeechSynthesizer synth = new SpeechSynthesizer();
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
                try
                {
                    await SpeechSynthesizer.SpeakTextAsync(response);
                }
                catch (TaskCanceledException) { }
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
