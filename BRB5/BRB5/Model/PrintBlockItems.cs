using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class PrintBlockItems
    {
        public int PackageNumber { get; set; }
        public int Normal { get; set; }=0;
        public int Yellow { get; set; } = 0;
        public string stringPrintBlockItem { get { return "" + PackageNumber + "-" + Normal + "/" + Yellow; }  }
    }
}
