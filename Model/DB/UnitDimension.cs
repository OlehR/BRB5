using System;
using System.Collections.Generic;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace BRB5.Model.DB
{
    public class UnitDimension
    {
        public int CodeUnit { get; set; }
        public string NameUnit { get; set; }
        public string AbrUnit { get; set; }
        public string Description { get; set; }
    }
}
