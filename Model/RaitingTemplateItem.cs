﻿using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace BRB5.Model
{
    public  class RaitingTemplateItem: INotifyPropertyChanged
    {
        public int IdTemplate { get; set; }
        public int Id { get; set; }
        public int Parent { get; set; }
        
        public decimal ValueRating { get; set; }
        [JsonIgnore]
        // заголовок групи
        public bool IsHead { get { return Parent==0; } }
      
        public string Text { get; set; }

        public string Explanation { get; set; }
        
        /// <summary>
        /// Доступні варіанти відповіді 1 -погано + 2-так собі + 4 - добре + 8 - відсутня відповідь
        /// </summary>
        public int RatingTemplate { get; set; }
        /// <summary>
        /// Порядок сортування
        /// </summary>
        public int OrderRS { get; set; }
        public DateTime DTDelete { get; set; }

        public bool IsItem { get { return !IsHead; } }
        [Ignore]
        public bool IsTemplate { get; set; } = false;

        bool _IsVisible = true;       

        [JsonIgnore]
        [Ignore]
        public bool IsVisible { get { return _IsVisible; } set { _IsVisible = value; OnPropertyChanged("IsVisible"); OnPropertyChanged("HeightRequest"); } }

        [JsonIgnore]
        public bool IsDelete { get { return DTDelete != default; }  }
        [JsonIgnore]
        [Ignore]
        public double OpacityDelete { get { return IsDelete ? 0.4d : 1d; } }

        [JsonIgnore]
        [Ignore]
        public bool IsEnableOk { get { return (RatingTemplate & 1) == 1; } set { RatingTemplate = value ? RatingTemplate | 1 : RatingTemplate & (8 + 4 + 2); OnPropertyChanged(nameof(OpacityOk)); } }
        [JsonIgnore]
        [Ignore]
        public bool IsEnableSoSo { get { return (RatingTemplate & 2) == 2; } set { RatingTemplate = value ? RatingTemplate | 2 : RatingTemplate & (8 + 4 + 1); OnPropertyChanged(nameof(OpacitySoSo)); } }
        [JsonIgnore]
        [Ignore]
        public bool IsEnableBad { get { return (RatingTemplate & 4) == 4; } set { RatingTemplate = value ? RatingTemplate | 4 : RatingTemplate & (8 + 2 + 1); OnPropertyChanged(nameof(OpacityBad)); } }
        
        [JsonIgnore]
        [Ignore]
        public bool IsEnableNotKnow { get { return (RatingTemplate & 8) == 8; } set { RatingTemplate = value ? RatingTemplate | 8 : RatingTemplate & (4 + 2 + 1); OnPropertyChanged(nameof(OpacityNotKnow)); } }

        [JsonIgnore]
        public double OpacityOk { get { if (IsTemplate) return IsEnableOk ? 1d : 0.4d; return RatingTemplate == 1 ? 1d : 0.4d; } }
        [JsonIgnore]
        public double OpacitySoSo { get { if (IsTemplate) return IsEnableSoSo ? 1d : 0.4d; return RatingTemplate == 2 ? 1d : 0.4d; } }
        [JsonIgnore]
        public double OpacityBad { get { if (IsTemplate) return IsEnableBad ? 1d : 0.4d; return RatingTemplate == 3 ? 1d : 0.4d; } }
        [JsonIgnore]
        public double OpacityNotKnow { get { if (IsTemplate) return IsEnableNotKnow ? 1d : 0.4d; return RatingTemplate == 4 ? 1d : 0.4d; } }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
