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
        private decimal _QuantityInput;
        public decimal QuantityInput { get => _QuantityInput; set { _QuantityInput = value; OnPropertyChanged(nameof(QuantityInput)); } }

        private DateTime _ExpirationDateInput;
        private DateTime _ProductionDateInput;
        public DateTime ExpirationDateInput { get => _ExpirationDateInput; set {_ExpirationDateInput = value; OnPropertyChanged(nameof(ExpirationDateInput)); OnPropertyChanged(nameof(GetPercentColor)); OnPropertyChanged(nameof(GetColor)); } }
        public DateTime ProductionDateInput { get => _ProductionDateInput; set { _ProductionDateInput = value; OnPropertyChanged(nameof(ProductionDateInput)); } }
        public string NameWares { get; set; }
        public string BarCode { get; set; }
        public int CodeUnit { get; set; }
        public string NameUnit { get; set; }

        public int[] DaysRight { get { return DaysLeft?.Split(';')?.Select(e => e.ToInt()).ToArray() ?? new int[0]; } }

        public Color GetColor { get { return (ExpirationDateInput == default ? GetPercentColor?.ColorLight: GetPercentColor?.ColorNormal)??Color.LightGray ;  } }
        public PercentColor GetPercentColor { get { int i = GetColourIndex(); 
                return i < 0 || Connector.PercentColor == null || i >= Connector.PercentColor.Length ? null : Connector.PercentColor[i]; } }

        int GetColourIndex()
        {
            int i = 0;
            while (i < DaysRight.Length)
            {
                if (((ExpirationDateInput==default? ExpirationDate : ExpirationDateInput) -DateTime.Today).Days >= DaysRight[i]) break;
                i++;
            }
            return i - 1;
        }

               public DocWaresExpiration GetDocWaresExpiration()
        {
            return new DocWaresExpiration() { CodeWares = CodeWares, DocId = DocId, DateDoc = DateTime.Today, NumberDoc = NumberDoc, QuantityInput = QuantityInput, ExpirationDateInput = ExpirationDateInput };
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
