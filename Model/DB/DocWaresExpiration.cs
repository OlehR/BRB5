using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model.DB
{
    public class DocWaresExpiration
    {
        public DateTime DateDoc { get; set; } = DateTime.Now.Date;
        public string NumberDoc { get; set; }
        public string DocId { get; set; }
        public long CodeWares { get; set; }
        public decimal QuantityInput { get; set; }
        public DateTime ExpirationDateInput { get; set; }
    }

    public class DocWaresExpirationSave
    {
        public string NumberDoc { get; set; }
        public int CodeWarehouse { get; set; }
        public IEnumerable<DocWaresExpiration> Wares { get; set; }
    }
}
