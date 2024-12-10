using BRB5.Model;
using BRB5.Model.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;


namespace BRB5.Model
{
    public class ExpirationDateElementVM: DocWaresExpirationSample
    {

        public decimal QuantityInput { get; set; }
        public DateTime ExpirationDateInput { get; set; }
        public string NameWares { get; set; }
        public string BarCode { get; set; }
        public int CodeUnit { get; set; }
        public string NameUnit { get; set; }

        public int[] DaysRight { get { return DaysLeft?.Split(';')?.Select(e => e.ToInt()).ToArray() ?? new int[0]; } }


        public PercentColor GetPercentColor { get { int i = GetColourIndex(); return i < 0 || Connector.PercentColor == null || i >= Connector.PercentColor.Length ? null : Connector.PercentColor[i]; } }

        int GetColourIndex()
        {
            int i = 0;
            while (i < DaysRight.Length)
            {
                if (QuantityInput <= DaysRight[i]) break;
                i++;
            }
            return i - 1;
        }
        
    }
}
