using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model.DB
{
    public class DocWaresExpirationSample 
    {        //NumberDoc, DocId, CodeWares, Quantity, ExpirationDate, Expiration, DaysLeft

        public string NumberDoc { get; set; }
        public string DocId { get; set; }
        public long CodeWares { get; set; }        
        public decimal Quantity { get; set; }
        public decimal QuantityInput { get; set; }
        public decimal Expiration { get; set; }
        public DateTime ExpirationDate { get; set; }        
        public string DaysLeft { get; set; }
        public int OrderDoc { get; set; }

    }
}
