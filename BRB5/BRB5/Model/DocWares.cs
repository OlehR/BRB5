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
        public decimal Quantity { get; set; }
    }
    public class DocWares: DocWaresId
    {
        public decimal QuantityOld { get; set; }
        public decimal InputQuantity { get; set; }
        public int CodeReason { get; set; }
        public DateTime DTInsert { get; set; }
    }
}
