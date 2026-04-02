using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRB5.Model
{
    public class CreateDocData
    {
        public int TypeDoc { get; set; }
        public int CodeWarehouse { get { return CodeWarehouseFrom; } }
        public int CodeWarehouseFrom { get; set; }
        public int CodeWarehouseTo { get; set; }
        public int CodeReason { get; set; }
        public int CodeUser { get; set; }
        public string Description { get; set; }
        public string ExtInfo { get; set; }
    }
}
