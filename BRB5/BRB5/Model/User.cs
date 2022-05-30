using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public enum eTypeUser
    { }

    public class User
    {
        public int CodeUser { get; set; }
        public string NameUser { get; set; }
        public string BarCode { get; set; }
        public string Login { get; set; }
        public string PassWord { get; set; }
        public eTypeUser TypeUser { get; set; }
    }
}
