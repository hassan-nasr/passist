using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalAssistant;
using PersonalAssistant.Service.Weather;
using PersonalAssistant.Service.Weather.LocalWeather;

/// <summary>
/// Summary description for FreeAPI
/// </summary>
public class FreeAPI
{
    public string ApiBaseURL = "http://api.worldweatheronline.com/free/v1/";
    public string FreeAPIKey = "pehqskn6dr4hx4chhtgmrn2k";

    public FreeAPI()
    {
        if (Settings.GetInstance().APIKey.Any())
            FreeAPIKey = Settings.GetInstance().APIKey;
    }

    private AsyncCallback successCallback = null;
    private AsyncCallback failCallback = null;
    public void GetLocalWeather(LocalWeatherInput input, AsyncCallback success, AsyncCallback fail)
    {
        successCallback = success;
        failCallback = fail;
        // create URL based on input paramters
        string apiURL = ApiBaseURL + "weather.ashx?q=" + input.query + "&format=" + "json" + "&extra=" + input.extra + "&num_of_days=" + input.num_of_days + "&date=" + input.date + "&fx=" + input.fx + "&cc=" + input.cc + "&includelocation=" + input.includelocation + "&show_comments=" + input.show_comments + "&callback=" + input.callback + "&key="+FreeAPIKey;
        System.Diagnostics.Debug.WriteLine(apiURL);
        // get the web response
        RequestHandler rh = new RequestHandler(); 
        rh.Process(apiURL,GetLoaclWeatherCallback,fail);

        // serialize the json output and parse in the helper class
//        LocalWeather lWeather = JsonConvert.DeserializeObject<LocalWeather>(result);
        

    }

    private void GetLoaclWeatherCallback(IAsyncResult asyncResult)
    {
        string result = (string) asyncResult.AsyncState;
        LocalWeather weatherData = JsonConvert.DeserializeObject<LocalWeather>(result);
        successCallback.Invoke(new Task((object obj) =>{}, weatherData));
    }
    /*public LocationSearch SearchLocation(LocationSearchInput input)
    {
        // create URL based on input paramters
        string apiURL = ApiBaseURL + "search.ashx?q=" + input.query + "&format=" + input.format + "&timezone=" + input.timezone + "&popular=" + input.popular + "&num_of_results=" + input.num_of_results + "&callback=" + input.callback + "&key=" + FreeAPIKey;

        // get the web response
        string result = RequestHandler.Process(apiURL);

        // serialize the json output and parse in the helper class
        LocationSearch locationSearch = null;//(LocationSearch)new JavaScriptSerializer().Deserialize(result, typeof(LocationSearch));

        return locationSearch;
    }

    public Timezone GetTimeZone(TimeZoneInput input)
    {
        // create URL based on input paramters
        string apiURL = ApiBaseURL + "tz.ashx?q=" + input.query + "&format=" + input.format + "&callback=" + input.callback + "&key=" + FreeAPIKey;

        // get the web response
        string result = RequestHandler.Process(apiURL);

        // serialize the json output and parse in the helper class
        Timezone timeZone = null;//(Timezone)new JavaScriptSerializer().Deserialize(result, typeof(Timezone));

        return timeZone;
    }

    public MarineWeather GetMarineWeather(MarineWeatherInput input)
    {
        // create URL based on input paramters
        string apiURL = ApiBaseURL + "marine.ashx?q=" + input.query + "&format=" + input.format + "&fx=" + input.fx + "&callback=" + input.callback + "&key=" + FreeAPIKey;

        // get the web response
        string result = RequestHandler.Process(apiURL);

        // serialize the json output and parse in the helper class
        MarineWeather mWeather = null;//(MarineWeather)new JavaScriptSerializer().Deserialize(result, typeof(MarineWeather));

        return mWeather;
    }

*/
}