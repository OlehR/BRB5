using System;
using System.Collections.Generic;
using System.Text;

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
        //public string GetJsonSE() { return "{\"Barcode\":\"" + BarCode + "\",\"Code\":\"" + CodeWares + "\",\"Status\":" + Status + ",\"LineNumber\":" + LineNumber + ",\"NumberOfReplenishment\":" + Double.tostring(NumberOfReplenishment) + "}"; }

        public LogPrice() { }
        
        //string regex = "[0-9]+";
        /*public boolean IsGoodBarCode()
        {
            if (BarCode != null && BarCode.trim().length() > 2 && BarCode.trim().replace("-", "").matches(regex))
                return true;
            return false;
        }*/
    }
}
