using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Phone.Speech.VoiceCommands;
using Microsoft.Phone.UserData;
using PersonalAssistant.Service.voice;

namespace PersonalAssistant.Service.contact
{
    class ContactManager
    {
        public void FillNames()
        {
            Contacts contacts = new Contacts();
            contacts.SearchCompleted += contacts_SearchCompleted;
            contacts.SearchAsync(String.Empty,FilterKind.None, "toGetAll");
        }

        void contacts_SearchCompleted(object sender, ContactsSearchEventArgs e)
        {
            var resultList =  e.Results;
            HashSet<String> allFoundNames = new HashSet<string>();
            foreach (Contact contact in resultList)
            {
                String name = contact.DisplayName;
                String[] parts = name.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < parts.Length; j++)
                {
                    string s = parts[j].ToLower();
                    if(!allFoundNames.Contains(s) && allFoundNames.Count<1000)
                        allFoundNames.Add(s);
                }
            }
            foreach (string langName in VoiceCommandInfo.AvailableLanguages)
            {
                if (VoiceCommandService.InstalledCommandSets.ContainsKey(langName))
                {
                    VoiceCommandSet widgetVcs = VoiceCommandService.InstalledCommandSets[langName];
                    widgetVcs.UpdatePhraseListAsync("contactLoos", allFoundNames);
                }
            }
#if DEBUG            
            System.Diagnostics.Debug.WriteLine("unique found names: "+ allFoundNames.Count);
#endif
        }
    }
}
