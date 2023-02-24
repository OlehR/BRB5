using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class DataStr
    {
        public DataStr() { }
        public DataStr(DateTime Date) {
            DateDoc=Date;
        }
        public DateTime DateDoc { get; set; }
        public string DateString { get { return DateDoc.ToString("dd.MM.yy"); } }
    }
}
