using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace BRB5.Model
{
    public class LogPrice
    {
        //BarCode, Status,  ActionType, PackageNumber, CodeWares, LineNumber, Article
        public string BarCode { get; set; }
        /// <summary>
        /// 100 - Ок Подвійне скануванн,101-Відсутній цінник,102 -відсутній товар
        /// -999 -Ofline,-9 - Проблема,
        /// </summary>
        public int Status { get; set; }
        public DateTime DTInsert { get; set; }
        [Ignore]
        public int IsSend { get; set; }
        public int ActionType { get; set; }
        public int PackageNumber { get; set; }
        public long CodeWares { get; set; }
        public string Article { get; set; }
        public int LineNumber { get; set; }
        [Ignore]
        public double NumberOfReplenishment { get; set; }
        //DateFormat format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        //public string GetJsonPSU() { return "[\"" + BarCode + "\"," + Status + ",\"" + format.format(DTInsert) + "\"," + PackageNumber + "," + CodeWares + "]"; }
        public LogPrice() { }

        public LogPrice(WaresPrice pWP, bool pIsOnline = true, int pPackageNumber = 0, int pLineNumber = 0)
        {
            BarCode = pWP?.ParseBarCode?.StartString;
            Status = Config.Company == eCompany.Sim23 ?
                    (pIsOnline ? -999 : (pWP.CodeWares == 0 ? 1 : (BarCode.Substring(0, 2).Equals("29") ? (pWP.PriceOld == pWP.Price && pWP.PriceOpt == pWP.PriceOptOld ? -1 : 0) : (pWP?.ParseBarCode.IsHandInput ?? false ? 3 : 2)))) :
                    (/*isError*/false ? -9 : (pWP.Price > 0 && pWP.PriceOld == pWP.Price && pWP.PriceOpt == pWP.PriceOptOld ? 1 : (/*this.Printer.varPrinterError != ePrinterError.None */false ? -1 : 0)));
            PackageNumber = pPackageNumber;
            CodeWares = pWP.CodeWares;
            LineNumber = pLineNumber;
            Article = pWP.Article;
            ActionType = pWP.ActionType;
        }

        public LogPrice(int pStatus, WaresPrice pWP, int pPackageNumber = 0, int pLineNumber = 0)
        {
            BarCode = pWP?.ParseBarCode?.StartString;
            Status = pStatus;
            PackageNumber = pPackageNumber;
            CodeWares = pWP.CodeWares;
            LineNumber = pLineNumber;
            Article = pWP.Article;
            ActionType = pWP.ActionType;
        }
        public object[] GetPSU()
        {
            object[] arr = { BarCode, Status, DTInsert, PackageNumber, CodeWares };//DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") TMP!!!!
            return arr;
        }

        Regex rg = new Regex("[0-9]+");
        public bool IsGoodBarCode { get { return BarCode != null && BarCode.Trim().Length > 2 && rg.IsMatch(BarCode.Trim().Replace("-", "")); } }
    }

    public class LogPriceSave
    {
        public string SerialNumber { get; set; }
        public int CodeWarehouse { get; set; }
        public int CodeUser { get; set; }
        public IEnumerable<LogPrice> LogPrice { get; set; }
    }

}
