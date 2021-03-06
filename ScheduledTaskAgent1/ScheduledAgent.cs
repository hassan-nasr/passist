﻿//#define DEBUG_AGENT
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using PersonalAssistant;
using PersonalAssistant.Service;

namespace ScheduledTaskAgent1
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override async void OnInvoke(ScheduledTask task)
        {

            //TODO: Add code to perform your task in background
            string toastMessage = "";

            // If your application uses both PeriodicTask and ResourceIntensiveTask
            // you can branch your application code here. Otherwise, you don't need to.
            if (task is PeriodicTask)
            {
                // Execute periodic task actions here.
                toastMessage = "Periodic task running.";
            }
            else
            {
                // Execute resource-intensive task actions here.
                toastMessage = "Resource-intensive task running.";
            }

            // Launch a toast to show that the agent is running.
            // The toast will not be shown if the foreground application is running.
            ShellToast toast = new ShellToast();
            toast.Title = Settings.ApplicationName;
            toast.Content = toastMessage;
//            toast.Show();
            BackGroundJob backGroundJob =  BackGroundJob.GetInstance();

            await backGroundJob.doJobs(handelResult);

            Thread.Sleep(20000);
            toast = new ShellToast();
            toast.Title = "Msoi";
            toast.Content = toastMessage;
//            toast.Show();


            // If debugging is enabled, launch the agent again in one minute.
//#if DEBUG_AGENT
//  ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
//#endif

            // Call NotifyComplete to let the system know the agent is done working.
            NotifyComplete();
        }

        public void handelResult(IAsyncResult result)
        {
            String message = (string) result.AsyncState;
            ShellToast toast = new ShellToast();
            toast.Title = "Mosi";
            toast.Content = message;
//            toast.Show();
        }
    }
}