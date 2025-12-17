using BRB5.Model.DB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;


namespace BRB5.Model
{
    public class ExpirationDateElementVM: DocWaresExpirationSample, INotifyPropertyChanged, ICloneable
    {
        private decimal? _QuantityInput;
        public decimal? QuantityInput { get => _QuantityInput; set { _QuantityInput = value; OnPropertyChanged(nameof(QuantityInput));
                OnPropertyChanged(nameof(GetNameWareColor)); OnPropertyChanged(nameof(DisplayedQuantity)); OnPropertyChanged(nameof(DisplayedExpirationDate)); } }
        public decimal DisplayedQuantity { get => QuantityInput ?? Quantity; }
        public DateTime DisplayedExpirationDate { get => QuantityInput == null ? ExpirationDate : ExpirationDateInput; }

        private DateTime _ExpirationDateInput;
        private DateTime _ProductionDateInput;
        public DateTime ExpirationDateInput { get => _ExpirationDateInput;
            set
            {
                _ExpirationDateInput = value;
                OnPropertyChanged(nameof(ExpirationDateInput)); 
                OnPropertyChanged(nameof(GetPercentColor)); 
                OnPropertyChanged(nameof(GetColor));
                //OnPropertyChanged(nameof(GetPercentColor));
                //OnPropertyChanged(nameof(GetColor));
            }
        }
        public DateTime ProductionDateInput { get => _ProductionDateInput; set { _ProductionDateInput = value; OnPropertyChanged(nameof(ProductionDateInput)); } }
        public string NameWares { get; set; }
        public string BarCode { get; set; }
        public int CodeUnit { get; set; }
        public string NameUnit { get; set; }

        public int[] DaysRight { get { return DaysLeft?.Split(';')?.Select(e => (int)e.ToDecimal()).ToArray() ?? new int[0]; } }

        public Color GetColor { get { return GetPercentColor?.ColorNormal??Color.LightGray ;  } }
        public PercentColor GetPercentColor { get { int i = GetColourIndex(); 
                return i < 0 || Connector.PercentColor == null || i >= Connector.PercentColor.Length ? null : Connector.PercentColor[i]; } }

        int GetColourIndex()
        {
            int Days = ((ExpirationDateInput == default ? ExpirationDate : ExpirationDateInput) - DateTime.Today).Days+1;
            if (Days <= 0) return Connector.PercentColor.Length - 1; //Якщо протерміновано
            int i = 0;
            while (i < DaysRight.Length && i< Connector.PercentColor.Length-2)
            {
                if (DaysRight[i] == -1) return i>0? i-1:0; //Connector.PercentColor.Length - 1
                if ( Days > DaysRight[i]) break;
                i++;
            }
            return i;
        }
        public Color GetNameWareColor { get { return QuantityInput== null? Color.White : Color.FromArgb(0xD4D8F2) ; } }


        public DocWaresExpiration GetDocWaresExpiration()
        {
            return new DocWaresExpiration() { CodeWares = CodeWares, DocId = DocId, DateDoc = DateTime.Today, NumberDoc = NumberDoc, QuantityInput = QuantityInput??0, ExpirationDateInput = ExpirationDateInput };
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
