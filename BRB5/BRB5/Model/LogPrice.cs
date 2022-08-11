using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BRB5.Model
{
    public class LogPrice
    {
        public string BarCode;
        public int Status;
        public DateTime DTInsert;
        public int IsSend;
        //public int ActionType;
        public int PackageNumber;
        public int CodeWares;
        //public string Article;
        public int LineNumber;
        public double NumberOfReplenishment;
        //DateFormat format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        //public string GetJsonPSU() { return "[\"" + BarCode + "\"," + Status + ",\"" + format.format(DTInsert) + "\"," + PackageNumber + "," + CodeWares + "]"; }
     
        public LogPrice() { }

        public LogPrice(WaresPrice pWP, bool pIsOnline = true,int pPackageNumber=0,int pLineNumber = 0)
        {
            BarCode = pWP.StartString;
            Status = Config.Company == eCompany.Sim23 ?
                    (pIsOnline ? -999 : (pWP.Code == 0 ? 1 : (BarCode.Substring(0, 2).Equals("29") ? (pWP.PriceOld == pWP.Price && pWP.PriceOpt == pWP.PriceOptOld ? -1 : 0) : (pWP?.ParseBarCode.IsHandInput ?? false ? 3 : 2)))) :
                    (/*isError*/false ? -9 : (pWP.Price > 0 && pWP.PriceOld == pWP.Price && pWP.PriceOpt == pWP.PriceOptOld ? 1 : (/*this.Printer.varPrinterError != ePrinterError.None */false ? -1 : 0)));
            PackageNumber = pPackageNumber;
            CodeWares =pWP.Code;
            LineNumber = pLineNumber;
        }

        public object[] GetPSU()
        {
            object[] arr = { BarCode, Status, DTInsert, PackageNumber, CodeWares };
            return arr;
        }

        Regex rg = new Regex("[0-9]+");
        public bool IsGoodBarCode { get { return BarCode != null && BarCode.Trim().Length > 2 && rg.IsMatch(BarCode.Trim().Replace("-", "")); } }
    }


}
