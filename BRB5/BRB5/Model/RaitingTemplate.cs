using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace BRB5.Model
{
    public class RaitingTemplate : INotifyPropertyChanged
    {
        
        public int Id { get; set; }
        public string Text { get; set; }
        public bool IsActive { get; set; }

        bool? _IsHidden=null;
        public bool IsHidden { get { return _IsHidden == null?IsActive: (bool)_IsHidden; } set { _IsHidden = value; OnPropertyChanged(nameof(IsHidden));  } }

        public event PropertyChangedEventHandler PropertyChanged; 
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
