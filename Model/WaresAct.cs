using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BRB5.Model
{
    public class WaresAct
    {
        public string NameWares { get; set; }
        public decimal Plan { get; set; }
        public decimal Fact { get; set; }
        public decimal QuantityDifference { get { return Fact - Plan; } }
        public string GetColor
        {
            get
            {
                if (QuantityDifference > 0)
                    return "#c4ffc4"; // Green
                else if (QuantityDifference < 0)
                    return "#ff0000"; // Red  
                else
                    return "#adaea7"; // Gray
            }
        }

    }
}
