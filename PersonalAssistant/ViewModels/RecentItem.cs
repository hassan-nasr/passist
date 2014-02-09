using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PersonalAssistant.ViewModels
{
    public class RecentItem : INotifyPropertyChanged
    {
        private string _id;
        /// <summary>
        /// Sample ViewModel property; this property is used to identify the object.
        /// </summary>
        /// <returns></returns>
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("ID");
                }
            }
        }

        private string _imageUri;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string ImageUri
        {
            get
            {
                return _imageUri;
            }
            set
            {
                if (value != _imageUri)
                {
                    _imageUri = value;
                    NotifyPropertyChanged("ImageUri");
                }
            }
        }

        private string _detailString;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string DetailString
        {
            get
            {
                return _detailString;
            }
            set
            {
                if (value != _detailString)
                {
                    _detailString = value;
                    NotifyPropertyChanged("DetailString");
                }
            }
        }

        private string _responseString;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding.
        /// </summary>
        /// <returns></returns>
        public string ResponseString
        {
            get
            {
                return _responseString;
            }
            set
            {
                if (value != _responseString)
                {
                    _responseString = value;
                    NotifyPropertyChanged("ResponseString");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}