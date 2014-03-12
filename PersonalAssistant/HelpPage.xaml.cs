using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PersonalAssistant.Service;

namespace PersonalAssistant
{
    public partial class HelpPage : PhoneApplicationPage
    {
        public HelpPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }
    }

    public class ItemDefinitiion
    {
        public String Pattern, Sample, Note;

        public ItemDefinitiion(string pattern, string sample, string note)
        {
            this.Pattern = pattern;
            this.Sample = sample;
            this.Note = note;
        }
    }
}