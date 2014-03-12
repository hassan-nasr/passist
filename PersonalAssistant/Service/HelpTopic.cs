using System;
using System.Collections.ObjectModel;

namespace PersonalAssistant.Service
{
    public class HelpTopic
    {
        public HelpTopic()
        {
        }

        public static ObservableCollection<HelpTopic> CreateHelpList()
        {
            ObservableCollection<HelpTopic> ret = new ObservableCollection<HelpTopic>();
            ret.Add(new HelpTopic("Weather", new[] {
                new ItemDefinitiion( "tell me the weather",null,"tells you current weather condition in your default place"), 
                new ItemDefinitiion( "tell me the weather in [place]","tell me the weather in Paris","you sould add the place in setting page"),
                new ItemDefinitiion( "tell me the weather in [city] on [day]","tell me the weather in Paris on monday","[day] can weekdays and today/tomarow you can also change the order of [day] and [city]")
            }));
            ret.Add(new HelpTopic("Time", new[] {
                new ItemDefinitiion( "tell me the weather",null,"tells you current weather condition in your default place"), 
                new ItemDefinitiion( "tell me the weather in [place]","tell me the weather in Paris","you sould add the place in setting page"),
                new ItemDefinitiion( "tell me the weather in [city] on [day]","tell me the weather in Paris on monday","[day] can weekdays and today/tomarow you can also change the order of [day] and [city]")
            }));
            return ret;
        }
        public String Category;
        public ItemDefinitiion[] Items;

        public HelpTopic(string category, ItemDefinitiion[] samples)
        {
            Items = samples;
            Category = category;
        }
    }
}