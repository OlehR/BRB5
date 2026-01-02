using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace BRB5.Model
{
    public class Api
    {
        public int CodeData { get; set; }
        public string Login { get; set; }
        public string PassWord { get; set; }
        public string SerialNumber { get; set; }
        public string BarCodeUser { get; set; }
        public string NameDCT { get; set; }
        public int Ver { get; set; }
        public long CodeWarehouse { get; set; }
        public Api(int pCodeData) : this()
        {
            CodeData = pCodeData;
        }
        public Api()
        {
            CodeWarehouse = Config.CodeWarehouse;
            Login = Config.Login;
            PassWord = Config.Password;
            Ver = Config.Ver;
            SerialNumber = Config.SN;
        }
    }

    public class ApiPrice : Api
    {
        public ApiPrice() : base() { }
        public ApiPrice(int pCodeData, ParseBarCode pPBC) : base(pCodeData)
        {
            BarCode = pPBC.BarCode;
            CodeWares = pPBC.CodeWares;
            Article = pPBC.Article;
        }
        public eTypePriceInfo TypePriceInfo { get; set; } = eTypePriceInfo.Short;
        public string BarCode { get; set; }
        public long CodeWares { get; set; }
        public long CodeShop { get; set; }
        public string StrCodeWares { get { return CodeWares > 0 ? CodeWares.ToString()  : null; } }

        public string StrCodeWarehouse { get { return CodeWarehouse.ToString(); } }
        public int Article { get; set; }
        IEnumerable<int> _WareHouses = null;
        public IEnumerable<int> WareHouses { get { return _WareHouses?? Config.CodesWarehouses; } set { _WareHouses=value;} }
        public string StrWareHouses { get { return WareHouses?.Any() == true ? "," + string.Join(",", WareHouses) + "," : null; } }
    }

    public class ApiDoc : Api
    {
        public ApiDoc() : base() { }
        public ApiDoc(int pCodeData, int pTypeDoc, string pNumberDoc = null) : base(pCodeData)
        {
            TypeDoc = pTypeDoc;
            NumberDoc = pNumberDoc;
        }
        public int TypeDoc { get; set; }
        public string NumberDoc { get; set; }
    }

    public class ApiSaveDoc : ApiDoc
    {
        public ApiSaveDoc() : base() { }
        public ApiSaveDoc(int pCodeData, int pTypeDoc, string pNumberDoc = null, IEnumerable<decimal[]> pWares = null) : base(pCodeData, pTypeDoc, pNumberDoc)
        {
            Wares = pWares;
        }
        public IEnumerable<decimal[]> Wares { get; set; }

    }
    public class ApiLogPrice : Api
    {
        public ApiLogPrice() : base() { }
        public ApiLogPrice(IEnumerable<object[]> pLogPrice) : base(141)
        {
            LogPrice = pLogPrice;
        }
        public IEnumerable<object[]> LogPrice { get; set; }
    }
    /*
    public class ApiPrintHTTP : Api
    {
        public ApiPrintHTTP() : base() { }
        public ApiPrintHTTP(string pWares = null) : base(999)
        {
            CodeWares = pWares;
        }
        public string CodeWares { get; set; }

    }*/
}
