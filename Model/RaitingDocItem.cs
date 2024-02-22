using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BRB5.Model
{
    public class RaitingDocItem : DocId
    {
        public int Id { get; set; }
        public int Parent { get; set; }
        public RaitingDocItem ParrentRDI { get; set; }
        public decimal ValueRating { get; set; }

        decimal _SumValueRating=0;
        public decimal SumValueRating {
            get {
                decimal res;
                if (Parent == 0 || Id==-1)
                   return  _SumValueRating;
                else
                {
                    switch (Rating)
                    {
                        case 1:
                            res = ValueRating;
                            break;
                        case 2:
                            res = ValueRating / 2;
                            break;

                        default:
                            res = 0;
                            break;
                    }                    
                }
                return res;
            } 
            set { _SumValueRating= value; } 
        }
        // заголовок групи
        [JsonIgnore]
        public bool IsHead { get { return Parent == 0; } }
        [JsonIgnore]
        public bool IsItem { get { return !IsHead; } }
        public string Text { get; set; }
        [JsonIgnore]
        public Color BackgroundColor { get { return IsHead ? Color.FromArgb(200, 200, 200)  : Color.FromArgb(230, 230, 230); } }
        /// <summary>
        /// Доступні варіанти відповіді 1 -погано + 2-так собі + 4 - добре + 8 - відсутня відповідь
        /// </summary>
        public int RatingTemplate { get; set; }
        /// <summary>
        /// Порядок сортування
        /// </summary>
        public int OrderRS { get; set; }


        int _Rating;
        public int Rating { get { return _Rating; } set 
            {
                _Rating = value;
                OnPropertyChanged("Rating");
                OnPropertyChanged("OpacityOk");
                OnPropertyChanged("OpacitySoSo");
                OnPropertyChanged("OpacityBad");
                OnPropertyChanged("OpacityNotKnow");
                OnPropertyChanged(nameof(SumValueRating));
                //OnPropertyChanged(nameof(Rating));
            } } 
        public string Note { get; set; }        
        public DateTime DTDelete { get; set; }
        int _QuantityPhoto;
       
        public int QuantityPhoto { get { return _QuantityPhoto; } set { _QuantityPhoto = value; OnPropertyChanged("QuantityPhoto"); } }

        [JsonIgnore]
        public bool IsVisPSU { get { return Config.Company != eCompany.Sim23; } }
        public bool IsVisTotal { get { return Id == -1 ; } }

        bool _IsVisible = false;
        [JsonIgnore]
        public bool IsVisible { get { return _IsVisible; } set { _IsVisible = value; OnPropertyChanged("IsVisible"); OnPropertyChanged("HeightRequest"); } }
        [JsonIgnore]
        public double HeightRequest { get { return _IsVisible ? -1d : 0d; } }
        [JsonIgnore]
        public bool IsDelete { get { return DTDelete != default; } set {  OnPropertyChanged(nameof(OpacityDelete)); } }
        public double OpacityDelete { get { return IsDelete ? 0.3d : 1d; } }
        [JsonIgnore]
        public double OpacityOk { get { return Rating == 1 ? 1d : 0.4d; } }
        [JsonIgnore]
        public double OpacitySoSo { get { return Rating == 2 ? 1d : 0.4d; } }
        [JsonIgnore]
        public double OpacityBad { get { return Rating == 3 ? 1d : 0.4d; } }
        [JsonIgnore]
        public double OpacityNotKnow { get { return Rating == 4 ? 1d : 0.4d; } }

        [JsonIgnore]
        public bool IsEnableOk { get { return (RatingTemplate & 1) == 1; } set { RatingTemplate = value ? RatingTemplate | 1 : RatingTemplate & (8 + 4 + 2); OnPropertyChanged(nameof(OpacityOk)); } }
        [JsonIgnore]
        public bool IsEnableSoSo { get { return (RatingTemplate & 2) == 2; } set { RatingTemplate = value ? RatingTemplate | 2 : RatingTemplate & (8 + 4 + 1); OnPropertyChanged(nameof(OpacitySoSo)); } }
            [JsonIgnore]
        public bool IsEnableBad { get { return (RatingTemplate & 4) == 4; } set { RatingTemplate = value ? RatingTemplate | 4 : RatingTemplate & (8 + 2 + 1); OnPropertyChanged(nameof(OpacityBad)); } }
        [JsonIgnore]
        public bool IsEnableNotKnow { get { return (RatingTemplate & 8) ==8; } set { RatingTemplate = value ? RatingTemplate | 8: RatingTemplate & ( 4 + 2+ 1); OnPropertyChanged(nameof(OpacityNotKnow)); } }    

    }
}
