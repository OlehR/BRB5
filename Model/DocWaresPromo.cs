using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRB5.Model
{
    public class DocWaresPromo : DocWaresId
    {
        public string WareName { get; set; }
        public int LineNumber { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal Price { get; set; } = 0;
        public int CodeReason { get; set; }
        public string Reason { get; set; }
        public string Article { get; set; }
    }
}
