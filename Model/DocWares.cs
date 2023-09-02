using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class DocWaresId : DocId
    {
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
        public string QuantityOldStr { set { _QuantityOld = Convert.ToDecimal(value); } }

        private decimal _InputQuantity;
        [Ignore]
        public decimal InputQuantity { get { return _InputQuantity; } set { _InputQuantity = value; OnPropertyChanged("InputQuantity"); } }
        public string InputQuantityStr { set { _InputQuantity = Convert.ToDecimal(value); } }
        public int CodeReason { get; set; }
        public DateTime DTInsert { get; set; }
    }
}
