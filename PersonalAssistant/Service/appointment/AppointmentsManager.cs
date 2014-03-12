using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Phone.Speech.Synthesis;
using Windows.Phone.Speech.VoiceCommands;
using Microsoft.Phone.UserData;

namespace PersonalAssistant.Service.appointment
{
    class AppointmentsManager
    {
        public void FillSpeachDataWithAppointments()
        {
            Appointments appo = new Appointments();
            appo.SearchCompleted += finishSearchAndAddSpeach;
            appo.SearchAsync(DateTime.Now, DateTime.Now.AddYears(1), 1000, null);
        }

        private async void finishSearchAndAddSpeach(object sender, AppointmentsSearchEventArgs e)
        {
            List<Appointment> appointments = e.Results.ToList();
            HashSet<String> appointmentsName= new HashSet<string>();

            for (int i = 0; i < appointments.Count; i++)
            {
                var appointment = appointments[i];
                if(appointment.IsPrivate)
                    continue;
                appointmentsName.Add(appointment.Subject);
            }
            if (VoiceCommandService.InstalledCommandSets.ContainsKey("en-us-1"))
            {
                VoiceCommandSet widgetVcs = VoiceCommandService.InstalledCommandSets["en-us-1"];
                widgetVcs.UpdatePhraseListAsync("appointment", appointmentsName);
            }
        }

        public void FindAppointmentByName(String name,AsyncCallback callback)
        {

            Appointments appo = new Appointments();
            appo.SearchCompleted += new AppointmentFinder(callback,name).finishSearch;
            appo.SearchAsync(DateTime.Now, DateTime.Now.AddYears(1), 1000, null);
        }
    }
    class AppointmentFinder
    {
        private AsyncCallback callback;
        private String toFind;

        public AppointmentFinder(AsyncCallback callback, string toFind)
        {
            this.callback = callback;
            this.toFind = toFind;
        }

        public async void finishSearch(object sender, AppointmentsSearchEventArgs e)
        {
            List<Appointment> appointments = e.Results.ToList();
            HashSet<String> appointmentsName= new HashSet<string>();
            foreach (var appointment in appointments)
            {
                if(appointment.IsPrivate)
                    continue;
                if (appointment.Subject.Equals(toFind))
                {
                    callback.Invoke(new Task(o => { }, appointment));
                    return;
                }
            }
            callback.Invoke(new Task(o => { }, null));
        }

    }
}
