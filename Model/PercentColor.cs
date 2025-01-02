using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BRB5.Model
{
    public class PercentColor
    {
        //public bool IsLight { get; set; } = false;
        public PercentColor() { }
        public PercentColor(int pPercent, Color pColorLight, Color pColorNormal, string pBarCode)
        {
            Percent = pPercent;
            ColorLight = pColorLight;
            ColorNormal = pColorNormal;
            BarCode = pBarCode;
        }
        public int Percent { get; set; }
       // public Color Color { get { return IsLight ? ColorLight : ColorNormal; } }
        public Color ColorLight { get; set; }
        public Color ColorNormal { get; set; }
        public string BarCode { get; set; }
    }
}
