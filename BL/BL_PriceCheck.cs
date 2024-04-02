using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BL
{
    public partial class BL
    {
        public (WaresPrice, ParseBarCode) FoundWares(string pBarCode, int PackageNumber, int LineNumber, bool pIsHandInput, bool IsOnline = true)
        {
            WaresPrice WP;
            ParseBarCode PB = null;
            if (IsOnline)
            {
                PB = c.ParsedBarCode(pBarCode, pIsHandInput);
                WP = c.GetPrice(PB);
            }
            else
            {
                var data = GetWaresFromBarcode(0, null, pBarCode, pIsHandInput);
                WP = new WaresPrice(data);
            }

            var l = new LogPrice(WP, IsOnline, PackageNumber, LineNumber);
            db.InsLogPrice(l);

            return (WP, PB);
        }
    }
}
