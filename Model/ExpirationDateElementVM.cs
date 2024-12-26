using BRB5.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;


namespace BRB5.Model
{
    public class ExpirationDateElementVM: DocWaresExpirationSample, INotifyPropertyChanged
    {

        public decimal QuantityInput { get; set; }

        private DateTime _ExpirationDateInput;
        private DateTime _ProductionDateInput;
        public DateTime ExpirationDateInput { get => _ExpirationDateInput; set {_ExpirationDateInput = value; OnPropertyChanged(nameof(ExpirationDateInput));  } }
        public DateTime ProductionDateInput { get => _ProductionDateInput; set { _ProductionDateInput = value; OnPropertyChanged(nameof(ProductionDateInput)); } }
        public string NameWares { get; set; }
        public string BarCode { get; set; }
        public int CodeUnit { get; set; }
        public string NameUnit { get; set; }

        public int[] DaysRight { get { return DaysLeft?.Split(';')?.Select(e => e.ToInt()).ToArray() ?? new int[0]; } }

        public PercentColor GetPercentColor { get { int i = GetColourIndex(); 
                return i < 0 || Connector.PercentColor == null || i >= Connector.PercentColor.Length ? null : Connector.PercentColor[i]; } }

        int GetColourIndex()
        {
            int i = 0;
            while (i < DaysRight.Length)
            {
                if ((ExpirationDateInput-DateTime.Today).Days >= DaysRight[i]) break;
                i++;
            }
            return i - 1;
        }

        public string GetBackgroundColor
        {
            get
            {
                /*  if(!DocSetting.IsViewPlan)
                      return "fff3cd";*/
                /*
                if (Even)
                    switch (Ord)
                    {
                        case 3:
                            return "#FFB0B0";
                        case 2:
                            return "#FFC050";
                        case 1:
                            return "#FFFF80";
                        case 0:
                            return "#80FF80";
                        default:
                            return "#fff3cd";
                    }
                else
                    switch (Ord)
                    {
                        case 3:
                            return "#FFD1D1";
                        case 2:
                            return "#ffdd8a";
                        case 1:
                            return "#ffffb7";
                        case 0:
                            return "#c4ffc4";
                        case -1:
                            return "#ffffff";
                        default:
                            return "#fff3cd";
                    }*/
                return "#fff3cd";
            }
        }
        //public string GetBackgroundColor { get { return "#fff3cd"; } }
        public DocWaresExpiration GetDocWaresExpiration()
        {
            return new DocWaresExpiration() { CodeWares = CodeWares, DocId = DocId, DateDoc = DateTime.Today, NumberDoc = NumberDoc, QuantityInput = QuantityInput, ExpirationDateInput = ExpirationDateInput };
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
