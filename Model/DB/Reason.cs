using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace BRB5.Model.DB
{
    public class Reason
    {
        public int Level { get; set; }
        public int CodeReason { get; set; }
        public string NameReason { get; set; }
    }
}
