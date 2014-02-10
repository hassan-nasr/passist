﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PersonalAssistant.Resources;
using PersonalAssistant.Service;
using PersonalAssistant.ViewModels;

namespace PersonalAssistant
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the LongListSelector control to the sample data
            DataContext = App.ViewModel;
            this.onLoadedEventHandler = new RoutedEventHandler(OnLoaded);
            this.Loaded += onLoadedEventHandler;

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        private Boolean voiceCommandInitialized = false;
        private RoutedEventHandler onLoadedEventHandler;

        // Register the voice commands. Called when the app is first launched. 
        public async void OnLoaded(object sender, EventArgs e)
        {
            if (!this.voiceCommandInitialized)
            {
                try
                {
                    Uri uri = new Uri("ms-appx:///Resources/voicecommands.xml", UriKind.Absolute);
                    await Windows.Phone.Speech.VoiceCommands.VoiceCommandService.InstallCommandSetsFromFileAsync(uri);

                    this.voiceCommandInitialized = true;
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.ToString() + "\r\nVoice Commands failed to initialize.");

                }
            }

        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
            // Is this a new activation or a resurrection from tombstone?
            if (e.NavigationMode == System.Windows.Navigation.NavigationMode.New)
            {

                InitializeComponent();

                SystemTray.SetIsVisible(this, true);

                SystemTray.SetOpacity(this, 1);
                SystemTray.SetBackgroundColor(this, Colors.Magenta);
                SystemTray.SetForegroundColor(this, Colors.Black);

                progressIndicator = new ProgressIndicator();
                progressIndicator.IsVisible = true;
                progressIndicator.IsIndeterminate = true;

                // Was the app launched using a voice command?
                if (NavigationContext.QueryString.ContainsKey("voiceCommandName"))
                {

                    progressIndicator.Text = "Responding...";
                    SystemTray.SetProgressIndicator(this, progressIndicator);

                    RecognitionEngin engin = new RecognitionEngin();
                    engin.RespondToQuery(NavigationContext.QueryString, FinishResponseSimple, SentViewableResult);
                }
            }
        }
        private void SentViewableResult(IAsyncResult ar)
        {
            ResponseItem result = (ResponseItem)ar.AsyncState;
            if (result == null)
                return;
            System.Diagnostics.Debug.WriteLine("Recent List Size Befor: " + App.ViewModel.RecentItems.Count);
            Dispatcher.BeginInvoke(() =>
            {
                App.ViewModel.RecentItems.Insert(0, result);
                App.ViewModel.PersistData();

                System.Diagnostics.Debug.WriteLine("Recent List Size After: " + App.ViewModel.RecentItems.Count);
            });


        }

        public void FinishResponseSimple(IAsyncResult result)
        {
            String message = (string)result.AsyncState;
            System.Diagnostics.Debug.WriteLine(message);
            Dispatcher.BeginInvoke(() =>
            {
                progressIndicator.IsIndeterminate = false;
                progressIndicator.Text = message;
            });
        }


        // Handle selection changed on LongListSelector
        private void MainLongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            return;
            // If selected item is null (no selection) do nothing
            if (MainLongListSelector.SelectedItem == null )
                return;

            
            // Reset selected item to null (no selection)
            MainLongListSelector.SelectedItem = null;
        }

        // Sample code for building a localized ApplicationBar
        //private void BuildLocalizedApplicationBar()
        //{
        //    // Set the page's ApplicationBar to a new instance of ApplicationBar.
        //    ApplicationBar = new ApplicationBar();

        //    // Create a new button and set the text value to the localized string from AppResources.
        //    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
        //    appBarButton.Text = AppResources.AppBarButtonText;
        //    ApplicationBar.Buttons.Add(appBarButton);

        //    // Create a new menu item with the localized string from AppResources.
        //    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
        //    ApplicationBar.MenuItems.Add(appBarMenuItem);
        //}
        private void UpdateData(object sender, EventArgs e)
        {
            SystemTray.SetIsVisible(this, true);

            ProgressIndicator prog;
            SystemTray.SetOpacity(this, 1);
            SystemTray.SetBackgroundColor(this, Colors.Magenta);
            SystemTray.SetForegroundColor(this, Colors.Black);

            prog = new ProgressIndicator();
            prog.IsVisible = true;
            prog.IsIndeterminate = true;
            prog.Text = "Updating data...";
            SystemTray.SetProgressIndicator(this, prog);
            progressIndicator = prog;
            new BackGroundJob().doJobs(FinishUpdateData);
        }

        private ProgressIndicator progressIndicator;
        public void FinishUpdateData(IAsyncResult result)
        {
            String message = (string)result.AsyncState;
            System.Diagnostics.Debug.WriteLine(message);
            Application application = null;
            Dispatcher.BeginInvoke(() =>
            {
                progressIndicator.IsIndeterminate = false;
                progressIndicator.Text = message;
            });
        }
       
    }

//    class FinishUpdateDataCalss
//    {
//        private ProgressIndicator progressIndicator;
//        //        private Dispatcher dispatcher;
//        public FinishUpdateDataCalss(ProgressIndicator progressIndicator)
//        {
//            //            dispatcher = d;
//            this.progressIndicator = progressIndicator;
//        }
//        public void FinishUpdateData(IAsyncResult result)
//        {
//            String message = (string)result.AsyncState;
//            System.Diagnostics.Debug.WriteLine(message);
//            Application application = null;
//            Dispatcher.BeginInvoke(() =>
//            {
//                progressIndicator.IsIndeterminate = false;
//                progressIndicator.Text = message;
//            });
//        }
//
//    }
    
}