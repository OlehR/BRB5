using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace BRB5.Model
{
    public class DocExpiration : INotifyPropertyChanged
    {    
        public string NumberDoc { get; set; }        
        public string Description { get; set; }
        public int Count { get; set; }
        private int _CountInput;
        public int CountInput { get { return _CountInput; } set { _CountInput = value; OnPropertyChanged(nameof(CountInput)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}