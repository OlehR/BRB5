using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Utils;

namespace BRB5.Model
{
    public class Warehouse : INotifyPropertyChanged
    {
        /// <summary>
        /// Унікальний Код може бути такий же як і Number
        /// </summary>
        public int Code {get;set;}
        /// <summary>
        /// Номер в випадку Sim23 4 значний код 1104
        /// </summary>
        public string Number {get;set;}
        public int CodeWarehouse { get { try { return string.IsNullOrEmpty(Number)? Code: Convert.ToInt32(Number); } catch { return 0; } }}
        /// <summary>
        /// Назва складу
        /// </summary>
        public string Name {get;set;}

        public string Address { get;set;}

        public string ShortAddress
        {
            get
            {
                var temp = Address.Split('-')[1];
                if (temp.Length < 2) temp = Address;
                return temp;
            }
        } // 

        public string Url {get;set;}
        public string InternalIP {get;set;}
        public string ExternalIP {get;set;}
        /// <summary>
        /// gps координати магазину 23.3451,45.2344
        /// </summary>
        public string Location { get;set;}

        public double GPSX { get { return Location.Split(',').Length == 2 ? Location.Split(',')[0].ToDouble():0d; } }
        public double GPSY { get { return Location.Split(',').Length == 2 ? Location.Split(',')[1].ToDouble():0d; } }
        /// <summary>
        /// Дистанція
        /// </summary>
        [Ignore]
        public double Distance { get; set; }


        private bool _IsChecked;
        [Ignore]
        public bool IsChecked { get { return _IsChecked; } set { _IsChecked = value; OnPropertyChanged(nameof(IsChecked)); } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}