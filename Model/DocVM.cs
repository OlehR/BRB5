using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
//using System.Drawing;
using System.Text;
//using Xamarin.Forms;

namespace BRB5.Model
{
    public class DocId:  INotifyPropertyChanged
    {

        /// <summary>
        /// Тип документа (1-ревізія 2-приходи тощо,11-опитування)
        /// </summary>
        public int TypeDoc { get; set; }
        /// <summary>
        /// (Номер документа в 1С)
        /// </summary>
        public string NumberDoc { get; set; }
        public DocId() { }

        public DocId(DocId pDocId)
        { 
            TypeDoc = pDocId.TypeDoc;
            NumberDoc = pDocId.NumberDoc;
        }

        public event PropertyChangedEventHandler PropertyChanged;        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    /// <summary>
    /// Відповідає структурі БД.
    /// </summary>
    public class Doc : DocId, ICloneable
    {
        #region ICloneable Members
        public object Clone()
        {
            return this.MemberwiseClone();
        }
        #endregion
        public Doc() : base() { }
        public Doc(DocId pDocId) : base(pDocId) { }
        /// <summary>
        /// Стан 0 - готується, 1 - збережено ...
        /// </summary>
        public int State { get; set; }

        /// <summary>
        /// Дата документа
        /// </summary>
        public DateTime DateDoc { get; set; }
        public string DateString { get { return DateDoc.ToString("dd.MM.yy"); } }

        /// <summary>
        /// 1- Якщо треба контролювати асортимент та кількість, для замовлень та можливо інших документів.
        /// </summary>
        public int IsControl { get; set; } // 

        /// <summary>
        /// Шаблон
        /// </summary>
        public int IdTemplate { get; set; }

        /// <summary>
        /// Код складу
        /// </summary>
        public int CodeWarehouse { get; set; }

        public string NameUser { get; set; } // 
        /// <summary>
        /// штрихкод документа, якщо є для швидкого пошуку
        /// </summary>
        public string BarCode { get; set; } //
        /// <summary>
        ///  (опис для ревізій коментар, якщо використовують для приходів назва контрагента)
        /// </summary>
        public string Description { get; set; } //
        /// <summary>
        /// Для замовлень номер прихідної, якщо створено.
        /// </summary>
        public string NumberDoc1C { get; set; } // 
        /// <summary>
        /// Дата розхідної накладної для приходу
        /// </summary>
        public string DateOutInvoice { get; set; } // 
        /// <summary>
        /// Номер розхідної накладної для приходу
        /// </summary>
        public string NumberOutInvoice { get; set; } // 

        public DateTime DTStart { get; set; }

        public DateTime DTEnd { get; set; }

        /// <summary>
        /// Колір відображення документа
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Додаткова інформація, яка може вплинути на обробку документа напиклад ЗКПО постачальника
        /// Для Анкет код складу магазина.
        /// </summary>
        public string ExtInfo { get; set; }
    }

    public class DocVM:Doc
    {        
        [Ignore]
        public string RaitingTemplateName { get; set; } = string.Empty;
        
        [Ignore]
        public string CodeWarehouseName { get; set; } = string.Empty;        

        /// <summary>
        /// Адреса
        /// </summary>
        public string Address { get; set; } // 
        [JsonIgnore]
        [Ignore]
        public string ShortAddress { 
            get {
                if (Address == null) return null;
                var temp = Address.Split('-')[1];
                if (temp.Length < 2) temp= Address;
                return temp;
            }  } // 

        

        private bool _SelectedColor = false;
        [JsonIgnore]
        [Ignore]
        public bool SelectedColor { get { return _SelectedColor; } set { _SelectedColor = value;   OnPropertyChanged(nameof(GetColor)); OnPropertyChanged(nameof(GetLightColor)); } } 

        // 9 - червоний, 2- оранжевий, 1 - жовтий, 0 - зелений, інше грязно жовтий-ранжевий.
        [JsonIgnore]
        public string GetColor
        {
            get
            {
                if (SelectedColor) return "#50c878";
                switch (Color)
                {
                    case 9:
                        return "#FFB0B0"; //Червоний
                    case 2:
                        return "#FFC050"; //Оранжевий
                    case 0:
                        return "#FFFF80"; //Жовтий
                    case 1:
                        return "#80FF80"; //Зелений
                    default:
                        return "#fff3cd";
                }
            }
        }
        [JsonIgnore]
        public string GetLightColor
        {
            get
            {
                if (SelectedColor) return "#50c878";
                switch (Color)
                {
                    case 9:
                        return "#FFD1D1"; //Червоний
                    case 2:
                        return "#ffdd8a"; //Оранжевий
                    case 0:
                        return "#ffffb7"; //Жовтий
                    case 1:
                        return "#c4ffc4"; //Зелений
                    default:
                        return "#fff3cd";
                }
            }
        }
        //public Color GetColor { get { return Color == 0?new Color(0xdcdcdc) : new Color(Color); } }

        //public int isClose; //0- не закривати, 1 - закривати.
        public DocVM() { }
        
        public DocVM(DocId pDocId):base(pDocId) { }        


    }
}
