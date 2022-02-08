using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5
{
    public enum eKindDoc {Normal,Simple, PriceCheck, Raiting }
    public class TypeDoc
    {
        public int CodeDoc { get; set; }
        public eKindDoc KindDoc { get; set; } = eKindDoc.Normal;
        public string NameDoc { get; set; }
        //public string NameClass { get { return $"{}_{CodeDoc}"}; }
    }
}
