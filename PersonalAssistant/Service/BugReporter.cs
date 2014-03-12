//#define DEBUG
using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace PersonalAssistant.Service
{
    public class BugReporter
    {
        private static BugReporter _instance;

        public static BugReporter GetInstance()
        {
            if (_instance == null)
            {
                _instance = new BugReporter();
            }
            return _instance;
        }

        private string debugFileName = "error.log";

        private BugReporter()
        {
        }

        private IsolatedStorageFileStream debugFile = null;
        System.IO.IsolatedStorage.IsolatedStorageFile local = null;

        private IsolatedStorageFileStream openDebugFile()
        {
            local = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication();
            
            debugFile = new System.IO.IsolatedStorage.IsolatedStorageFileStream(debugFileName,
                System.IO.FileMode.Append,
                local);
            return debugFile;
        }

        public void report(Exception e)
        {
            var file = openDebugFile();
            var writer = new StreamWriter(file);
            writer.Write(DateTime.Now + " :\r\n");
            writer.Write(e.StackTrace);
            writer.Close();
#if DEBUG
            System.Diagnostics.Debug.WriteLine(e.StackTrace);
#endif
        }

        public void clear()
        {
            debugFile = new System.IO.IsolatedStorage.IsolatedStorageFileStream(debugFileName,
                System.IO.FileMode.Create,
                local);
            debugFile.Close();
        }


        
    }
}