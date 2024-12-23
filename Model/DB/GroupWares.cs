using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model.DB
{
    public class GroupWares
    {
        public int CodeGroup { get; set; }//int
        public string NameGroup { get; set; }
        public bool IsAlcohol { get; set; } = false;
    }
}
