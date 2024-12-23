using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model.DB
{
    public class Wares
    {
        public int CodeWares { get; set; }
        public int CodeGroup { get; set; }
        public string NameWares { get; set; }
        public string Articl { get; set; }
        public int CodeBrand { get; set; }
        public string ArticlWaresBrand { get; set; }
        public int CodeUnit { get; set; }
        public string Description { get; set; }
        public decimal Vat { get; set; }
        public int VatOperation { get; set; }
        public string NameWaresReceipt { get; set; }
        public int CodeWaresRelative { get; set; }
        //public DateTime DateInsert
    }
}
