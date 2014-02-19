using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Phone.BackgroundTransfer;

namespace PersonalAssistant.Service
{
    class WebRequestManager
    {
        private static WebRequestManager webRequestManager;

        public static WebRequestManager GetInstance()
        {
            if(webRequestManager == null)
                webRequestManager = new WebRequestManager();
            return webRequestManager;
        }
        internal  HashSet<String> currentDownloadSet = new HashSet<string>();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void loadIfNotExist(string todl, string folder)
        {

            string key = todl + "#" + folder;
            if (currentDownloadSet.Contains(key))
                return;
            string name = todl.Substring(todl.LastIndexOf('/') + 1);
            if (!IsolatedStorageFile.GetUserStoreForApplication().DirectoryExists(folder))
                IsolatedStorageFile.GetUserStoreForApplication().CreateDirectory(folder);
            if (IsolatedStorageFile.GetUserStoreForApplication().FileExists(folder + "\\" + name))
                return;
            currentDownloadSet.Add(key);
            
            new WebRequest(key,this).downloadFile(todl, folder, name);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void RemoveKey(string key)
        {
            currentDownloadSet.Remove(key);
        }
    }
    class WebRequest
    {

     
        private String key;

        internal WebRequest(string key, WebRequestManager webRequestManager)
        {
            this.key = key;
            this.webRequestManager = webRequestManager;
        }
        String savePlace = null;
        public void downloadFile(String url, string place,String name)
        {
            if (BackgroundTransferService.Requests.Count() >= 25)
                throw new Exception("no more background task");
            BackgroundTransferRequest request = new BackgroundTransferRequest(new Uri(url,UriKind.Absolute));
            request.Method = "GET";
            if (name == null)
            {
                url.Substring(url.LastIndexOf("/") + 1);
            }
            savePlace = place;
            savePlace += "\\" + name;
            Uri downloadUri = new Uri("shared/transfers/" + name, UriKind.RelativeOrAbsolute);
            request.DownloadLocation = downloadUri;
            request.TransferStatusChanged += request_TransferStatusChanged;
            BackgroundTransferService.Add(request);
            System.Diagnostics.Debug.WriteLine("request created");
        }

        void request_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            BackgroundTransferRequest transfer = e.Request;
            if (transfer.StatusCode == 200 || transfer.StatusCode == 206)
            {
                using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                {

                    if (isoStore.FileExists(savePlace))
                    {
                        //    isoStore.DeleteFile(savePlace);
                        //    isoStore.Dispose();
                    }
                    else
                    {
                        isoStore.MoveFile(transfer.DownloadLocation.OriginalString, savePlace);
                        webRequestManager.RemoveKey(key);
                    }
                }
            }
        }

        private WebRequestManager webRequestManager;
    }
}
