using System;
using System.Collections.Generic;

namespace PersonalAssistant.Service.voice
{
    class VoiceCommandInfo
    {
        public static List<String> AvailableLanguages;

        static VoiceCommandInfo()
        {
            AvailableLanguages .Add("en-us-1");
            AvailableLanguages .Add("en-uk-1");
            AvailableLanguages .Add("en-in-1");
            AvailableLanguages .Add("fr-fr-1");
        }
    }
}
