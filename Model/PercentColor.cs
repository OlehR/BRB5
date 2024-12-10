using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace BRB5.Model
{
    public class PercentColor
    {
        public PercentColor() { }
        public PercentColor(int pPercent, Color pColor, string pBarCode)
        {
            Percent = pPercent;
            Color = pColor;
            BarCode = pBarCode;
        }
        public int Percent { get; set; }
        public Color Color { get; set; }
        public string BarCode { get; set; }
    }
}
