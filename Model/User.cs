using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class User
    {
        public eLoginServer LoginServer { get; set; }
        public int CodeUser { get; set; }
        public string NameUser { get; set; }
        public string BarCode { get; set; }
        public string Login { get; set; }
        public string PassWord { get; set; }
        public eRole Role { get; set; }
        public IEnumerable<TypeDoc> TypeDoc { get; set; }
        public eCompany LocalConnect { get; set; }
    }
}
