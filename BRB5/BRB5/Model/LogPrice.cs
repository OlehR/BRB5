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
        public int ActionType;
        public int PackageNumber;
        public int CodeWares;
        public string Article;
        public int LineNumber;
        public double NumberOfReplenishment;
        //DateFormat format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");

        //public string GetJsonPSU() { return "[\"" + BarCode + "\"," + Status + ",\"" + format.format(DTInsert) + "\"," + PackageNumber + "," + CodeWares + "]"; }
     
        public LogPrice() { }

        public object[] GetPSU()
        {
            object[] arr = { BarCode, Status, DTInsert, PackageNumber, CodeWares };
            return arr;
        }

        Regex rg = new Regex("[0-9]+");
        public bool IsGoodBarCode { get { return BarCode != null && BarCode.Trim().Length > 2 && rg.IsMatch(BarCode.Trim().Replace("-", "")); } }
    }


}
