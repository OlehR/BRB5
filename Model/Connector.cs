using BRB5;
using BRB5.Model.DB;
using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UtilNetwork;
using Utils;

namespace BRB5.Model
{
    public class ObservableInt { }
    public class Connector
    {
        protected static string TAG = "BRB6.Model/Connector";
        protected static Connector Instance = null;
        public Action<string> OnSave { get; set; }
        public volatile bool IsStopSave  = false;
        public volatile bool IsSaving = false;
        static protected IEnumerable<CustomerBarCode> CustomerBarCode { get; set; }

        public static void CleanConnector()
        { Instance = null; }

        //Логін
        public virtual Task<UtilNetwork.Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer, string pBarCode = null) { throw new NotImplementedException(); }
        //Завантаження довідників.
        public virtual async Task<UtilNetwork.Result> LoadGuidDataAsync(bool IsFull) => throw new NotImplementedException();

        //Робота з документами.
        /// <summary>
        /// Завантаження документів в ТЗД (HTTP)
        /// </summary>
        /// <param name="pTypeDoc">0-всі документи</param>
        /// <param name="pNumberDoc"></param>
        /// <param name="pProgress"></param>
        /// <param name="pIsClear"></param>
        /// <returns></returns>
        public virtual async Task<UtilNetwork.Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc, bool pIsClear) { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result<string>> GetNameWarehouseFromDoc(DocId pD) { throw new NotImplementedException(); }
        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public virtual async Task<UtilNetwork.Result> SendDocsDataAsync(DocVM pDoc, IEnumerable<DocWares> pWares) { throw new NotImplementedException(); }


        public virtual async Task<UtilNetwork.Result<Doc>> CreateDoc(Doc pDoc) { throw new NotImplementedException(); }
        //Збереження ПРосканованих товарів в 1С
        public virtual UtilNetwork.Result SendLogPrice(IEnumerable<LogPrice> pList) { throw new NotImplementedException(); }

        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public virtual async Task<UtilNetwork.Result> SendRatingAsync(IEnumerable<RaitingDocItem> pR, DocVM pDoc,bool pIsArchive=false)  { throw new NotImplementedException(); }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// </summary>
        /// <returns></returns>
        public virtual async Task<UtilNetwork.Result> SendRatingFilesAsync(string pNumberDoc, int pTry =2, int pMaxSecondSend = 0, int pSecondSkip = 0) { throw new NotImplementedException(); }


        /// <summary>
        /// Друк на стаціонарному термопринтері
        /// </summary>
        /// <param name="codeWares">Список товарів</param>
        /// <returns></returns>        
        public virtual string PrintHTTP(IEnumerable<long> pCodeWares, bool pIsOnlyRest = false) { throw new NotImplementedException(); }

        /// <summary>
        /// Розбір штрихкоду по правилам компанії
        /// </summary>
        /// <param name="pBarCode"></param>
        /// <param name="pIsOnlyBarCode"></param>
        /// <returns></returns>
        
        public virtual ParseBarCode ParsedBarCode(string pBarCode, bool pIsHandInput)
        {
            ParseBarCode Res = new() { BarCode = pBarCode };
            if (pBarCode.Length>25 && pBarCode.StartsWith("010")) //MatrixCode Упаковочний штрихкод.
            {
                Res.BarCode = pBarCode.Substring(3, 13);
            }
            /*if(pIsHandInput && pBarCode.Trim().Length<8)
            {
               return new ParseBarCode() { BarCode = pBarCode, IsHandInput = true, Article = pBarCode.ToInt(), TypeCode = eTypeCode.Article };
            }*/

           
            if(CustomerBarCode == null || CustomerBarCode.Count() == 0)
                return Res;
            long Code, Data, Data2;
            int Operator;
            bool IsFound;
            foreach (var el in CustomerBarCode.Where( el => el.TypeBarCode==eTypeBarCode.ManualInput || el.KindBarCode == eKindBarCode.EAN13 || el.KindBarCode == eKindBarCode.Code128 || el.KindBarCode == eKindBarCode.QR /*&& (el.TypeBarCode == eTypeBarCode.WaresWeight || el.TypeBarCode == eTypeBarCode.WaresUnit )*/))
            {
                try
                {
                    Code = 0; Data = 0; Data2 = 0; Operator = 0; 
                    IsFound = false;
                    if (el.TypeBarCode != eTypeBarCode.ManualInput && (string.IsNullOrEmpty(el.Separator) && el.TotalLenght != pBarCode.Length))
                        continue;
                   
                    if (!string.IsNullOrEmpty(el.Separator))
                    {
                        var D = pBarCode.Split(el.Separator);
                        if (D.Length > 1)
                        {
                            Code = D[0].ToLong();
                            Data = D.Length > 1 ? D[1].ToLong() : 0;
                            if( D.Length > 2) 
                                Data2 = D[2].ToLong();
                            IsFound = true;
                        }                        
                    }
                    if (pIsHandInput && el.TypeBarCode == eTypeBarCode.ManualInput && pBarCode.Trim().Length <= el.LenghtCode)
                    {
                        IsFound = true;
                        Code = pBarCode.ToLong();
                    }

                    if (Code == 0 && !string.IsNullOrEmpty(el.Prefix) && el.Prefix.Equals(pBarCode[..el.Prefix.Length]))
                    {
                        if (el.KindBarCode == eKindBarCode.EAN13 && pBarCode.Length != 13)
                            continue;

                        Code = Convert.ToInt32(pBarCode.Substring(el.Prefix.Length, el.LenghtCode));
                        if( el.LenghtOperator > 0 && el.TypeBarCode != eTypeBarCode.PriceTag )
                            Operator = Convert.ToInt32(pBarCode.Substring(el.Prefix.Length + el.LenghtCode, el.LenghtOperator));
                        if (el.TypeBarCode == eTypeBarCode.PriceTag)
                            Data = Convert.ToInt32(pBarCode.Substring(el.Prefix.Length + el.LenghtCode, el.LenghtOperator+el.LenghtPrice)); ///TMP!!! el.LenghtOperator
                        if (el.LenghtQuantity > 0)
                            Data = Convert.ToInt32(pBarCode.Substring(el.Prefix.Length + el.LenghtCode + el.LenghtOperator, el.LenghtQuantity));
                        IsFound = true;
                    }
                    if(!IsFound) continue;
                    if (Operator > 0 && el.LenghtOperator > 0) Res.CodeOperator = Operator;
                    if (Data > 0 && el.LenghtQuantity > 0) Res.Quantity = el.TypeBarCode== eTypeBarCode.WaresWeight? Data/1000m : Data;
                    if (Data > 0 && el.TypeBarCode == eTypeBarCode.PriceTag) Res.Price = (decimal)Data /100m;
                    if (Data2 > 0 && el.TypeBarCode == eTypeBarCode.PriceTag) Res.PriceOpt = (decimal)Data2/100m;

                    Res.TypeCode = el.TypeCode;
                    switch (el.TypeCode)
                    {
                        case eTypeCode.Article:
                            Res.Article = (int)Code;
                            break;
                        case eTypeCode.Code:
                            Res.CodeWares = Code;
                            break;
                        case eTypeCode.SKU:
                            Res.SKU = Code;
                            break;
                        case eTypeCode.PercentDiscount:
                            Res.PercentDiscount = Code;
                            break;
                        case eTypeCode.Coupon:
                        case eTypeCode.OneTimeCoupon:
                        case eTypeCode.OneTimeCouponGift:
                            Code = -1;
                            break;
                        default:
                            break;
                    }
                    if (Code != 0) break;
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, "ParsedBarCode", ex);
                }
            }
            return Res;
        }

        /// <summary>
        /// Ціна on-line
        /// </summary>
        /// <param name="pBC"></param>
        /// <returns></returns>
        public virtual UtilNetwork.Result<WaresPrice> GetPrice(ParseBarCode pBC, eTypePriceInfo pTP = eTypePriceInfo.Short) { throw new NotImplementedException(); }

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public virtual IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined) { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result<IEnumerable<Warehouse>>> LoadWarehouse() { throw new NotImplementedException(); }

        public virtual IEnumerable<LoginServer> LoginServer() { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result<int>> GetIdRaitingTemplate() { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result> GetNumberDocRaiting() { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result> SaveTemplate(RaitingTemplate pRT) { throw new NotImplementedException(); }        

        public virtual async Task<UtilNetwork.Result> SaveDocRaiting(DocVM pDoc) { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result<IEnumerable<RaitingTemplate>>> GetRaitingTemplateAsync() { throw new NotImplementedException(); }
        public virtual async Task<UtilNetwork.Result<IEnumerable<Doc>>> GetRaitingDocsAsync() { throw new NotImplementedException(); }
        public virtual async Task<UtilNetwork.Result<IEnumerable<DocVM>>> GetPromotion(int pCodeWarehouse) { throw new NotImplementedException(); }
        public virtual async Task<UtilNetwork.Result<IEnumerable<DocWares>>> GetPromotionData(string pNumberDoc) { throw new NotImplementedException(); }
        public virtual async Task<UtilNetwork.Result<IEnumerable<DocWaresExpirationSample>>> GetExpirationDateAsync(int pCodeWarehouse) { throw new NotImplementedException(); }

        //public virtual async Task<Result<IEnumerable<ExpirationWares>>> GetDaysLeft() { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result> SaveExpirationDate(DocWaresExpirationSave pED) { throw new NotImplementedException(); }

        static public PercentColor[] PercentColor = [];

        public virtual async Task<UtilNetwork.Result> GetInfo() { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result> UploadFile(string pFile, string pFileName = null) { throw new NotImplementedException(); }

        public virtual async Task<UtilNetwork.Result<IEnumerable<Client>>> GetClient(string  pBarCode) { throw new NotImplementedException(); }
    }

    public class LoginServer
    {
        public eLoginServer Code { get;set;}   
        public string Name { get;set;}
    }
    public class Guid
    {
        public string NameCompany { get; set; }
        public IEnumerable<UnitDimension> UnitDimension { get; set;}
        public IEnumerable<AdditionUnit> AdditionUnit { get; set; }
        public IEnumerable<Wares> Wares { get; set; }
        public IEnumerable<BARCode> BarCode { get; set; }
        public IEnumerable<Warehouse> Warehouse { get; set; }
        public IEnumerable<GroupWares> GroupWares { get; set; }
        public IEnumerable<Reason> Reason { get; set; }
        public IEnumerable<SKU> SKU { get; set; }
    }

    public class GetDocs
    {
        public int CodeWarehouse { get; set; }
        public int TypeDoc { get; set; }
        public string NumberDoc { get; set; }
        public int CodeUser { get; set; }
    }

    public class Docs
    {
        public IEnumerable<Doc> Doc {  get; set; }
        public IEnumerable<DocWaresSample> Wares { get; set; }
    }
    public class SaveDoc
    {
        public string NameDCT { get; set; }
        public Doc Doc { get; set; }
        public IEnumerable<DocWares> Wares { get; set; }
    }

}
