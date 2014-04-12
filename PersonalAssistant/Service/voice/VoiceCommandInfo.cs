using System;
using System.Collections.Generic;

namespace PersonalAssistant.Service.voice
{
    class VoiceCommandInfo
    {
        public static List<String> AvailableLanguages;

        static VoiceCommandInfo()
        {
            AvailableLanguages=new List<String>();
            AvailableLanguages .Add("en-us-1");
            AvailableLanguages .Add("en-gb-1");
            AvailableLanguages .Add("en-in-1");
//            AvailableLanguages .Add("fr-fr-1");
        }
    }
}
