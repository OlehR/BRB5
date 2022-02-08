using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace BRB5
{
    public class Raiting : DocId, INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        public int Parent { get; set; }
        public bool IsHead { get; set; }
        public string Text { get; set; }

        public int RatingTemplate { get; set; }

        int _Rating;
        public int Rating { get { return _Rating; } set 
            {
                _Rating = value;
                OnPropertyChanged("Rating");
                OnPropertyChanged("OpacityOk");
                OnPropertyChanged("OpacitySoSo");
                OnPropertyChanged("OpacityBad");
                OnPropertyChanged("OpacityNotKnow");
                //OnPropertyChanged(nameof(Rating));
            } } 
        public string Note { get; set; }
        int _QuantityPhoto;
        public int QuantityPhoto { get { return _QuantityPhoto; } set { _QuantityPhoto = value; OnPropertyChanged("QuantityPhoto"); } }

        bool _IsVisible = true;
        public bool IsVisible { get { return _IsVisible; } set { _IsVisible = value; OnPropertyChanged("IsVisible"); OnPropertyChanged("HeightRequest"); } }
        public double HeightRequest { get { return _IsVisible ? -1d : 0d; } }
        public double OpacityOk { get { return Rating == 1 ? 1d : 0.4d; } }
        public double OpacitySoSo { get { return Rating == 2 ? 1d : 0.4d; } }
        public double OpacityBad { get { return Rating == 3 ? 1d : 0.4d; } }
        public double OpacityNotKnow { get { return Rating == 4 ? 1d : 0.4d; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
