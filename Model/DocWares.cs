using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class DocWaresId : DocId
    {
        public DocWaresId() { }
        public DocWaresId(DocWaresId pDW)
        {
            TypeDoc = pDW.TypeDoc;
            NumberDoc = pDW.NumberDoc;
            OrderDoc = pDW.OrderDoc;
            CodeWares = pDW.CodeWares;
            Quantity = pDW.Quantity;
        }
        /// <summary>
        /// порядок по порядку в документі
        /// </summary>
        public int OrderDoc { get; set; }
        public int CodeWares { get; set; }
        public string QuantityStr { set { Quantity = Convert.ToDecimal(value); } }
        public decimal Quantity { get; set; }
    }
    public class DocWares: DocWaresId
    {
        public DocWares() { }
        public DocWares(DocWares pDW) {
            TypeDoc=pDW.TypeDoc;
            NumberDoc = pDW.NumberDoc;
            OrderDoc = pDW.OrderDoc;
            CodeWares = pDW.CodeWares;
            Quantity = pDW.Quantity;
            QuantityOld = pDW.QuantityOld;
            CodeReason = pDW.CodeReason;
                }
        private decimal _QuantityOld;
        public decimal QuantityOld { get { return _QuantityOld; } set { _QuantityOld = value; OnPropertyChanged("QuantityOld"); } }
        //public string QuantityOldStr { set { _QuantityOld = Convert.ToDecimal(value); } }

        private decimal _InputQuantity;       
        public decimal InputQuantity { get { return _InputQuantity; } set { _InputQuantity = value; OnPropertyChanged("InputQuantity"); OnPropertyChanged(nameof(Scaned)); OnPropertyChanged(nameof(GetBackgroundColorDocWares)); } }
        //public string InputQuantityStr { private get { return _InputQuantity.ToString(); } set { 
        //        _InputQuantity = Convert.ToDecimal(value); } }
        public int CodeReason { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime DTInsert { get; set; }
        public int Scaned { get { return InputQuantity > 0 ? 2 : 0; } set { OnPropertyChanged(nameof(GetBackgroundColorDocWares)); } }
        public string GetBackgroundColorDocWares
        {
            get
            {
                switch (Scaned)
                {
                    case 2:
                        return "#c4ffc4";
                    case 1:
                        return "#ffdd8a";
                    default:
                        return "#ffffff";
                }
            }
        }
    }
}
