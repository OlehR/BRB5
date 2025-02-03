using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model.DB
{
    public class AdditionUnit
    {
        public long CodeWares { get; set; }
        public int CodeUnit { get; set; }
        public decimal Coefficient { get; set; }
        public bool DefaultUnit { get; set; }
    }
}
