using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model.DB
{
    public class Wares
    {
        public long CodeWares { get; set; }
        public long CodeGroup { get; set; }
        public string NameWares { get; set; }
        public string Article { get; set; }
        public int CodeBrand { get; set; }
        public string ArticlWaresBrand { get; set; }
        public int CodeUnit { get; set; }
        public string Description { get; set; }
        public decimal Vat { get; set; }
        public int VatOperation { get; set; }
        public string NameWaresReceipt { get; set; }
        public int CodeWaresRelative { get; set; }
        //public DateTime DateInsert
        public string DaysLeft { get; set; }
        public decimal Expiration { get; set; }
    }
}
