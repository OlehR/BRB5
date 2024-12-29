using BRB5;
using BRB5.Model.DB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace BRB5.Model
{
    public class ObservableInt { }
    public class Connector
    {
        protected static string TAG = "BRB5.Model/Connector";
        protected static Connector Instance = null;
        public Action<string> OnSave { get; set; }
        public volatile bool IsStopSave  = false;
        public volatile bool IsSaving = false;

        //Логін
        public virtual Task<Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer) { throw new NotImplementedException(); }
        //Завантаження довідників.
        public virtual async Task<Result> LoadGuidDataAsync(bool IsFull) => throw new NotImplementedException();

        //Робота з документами.
        /// <summary>
        /// Завантаження документів в ТЗД (HTTP)
        /// </summary>
        /// <param name="pTypeDoc">0-всі документи</param>
        /// <param name="pNumberDoc"></param>
        /// <param name="pProgress"></param>
        /// <param name="pIsClear"></param>
        /// <returns></returns>
        public virtual async Task<Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc, bool pIsClear) { throw new NotImplementedException(); }


        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public virtual Result SendDocsData(DocVM pDoc, IEnumerable<DocWares> pWares) { throw new NotImplementedException(); }

        //Збереження ПРосканованих товарів в 1С
        public virtual Result SendLogPrice(IEnumerable<LogPrice> pList) { throw new NotImplementedException(); }

        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public virtual async Task<Result> SendRaitingAsync(IEnumerable<RaitingDocItem> pR, DocVM pDoc)  { throw new NotImplementedException(); }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Result> SendRaitingFilesAsync(string pNumberDoc, int pTry =2, int pMaxSecondSend = 0, int pSecondSkip = 0) { throw new NotImplementedException(); }


        /// <summary>
        /// Друк на стаціонарному термопринтері
        /// </summary>
        /// <param name="codeWares">Список товарів</param>
        /// <returns></returns>        
        public virtual string PrintHTTP(IEnumerable<int> pCodeWares) { throw new NotImplementedException(); }

        /// <summary>
        /// Розбір штрихкоду по правилам компанії
        /// </summary>
        /// <param name="pBarCode"></param>
        /// <param name="pIsOnlyBarCode"></param>
        /// <returns></returns>
        public virtual ParseBarCode ParsedBarCode(string pBarCode, bool pIsHandInput) { throw new NotImplementedException(); }

        /// <summary>
        /// Ціна on-line
        /// </summary>
        /// <param name="pBC"></param>
        /// <returns></returns>
        public virtual WaresPrice GetPrice(ParseBarCode pBC, eTypePriceInfo pTP = eTypePriceInfo.Short) { throw new NotImplementedException(); }

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public virtual IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined) { throw new NotImplementedException(); }

        public virtual async Task<Result<IEnumerable<Warehouse>>> LoadWarehouse() { throw new NotImplementedException(); }

        public virtual IEnumerable<LoginServer> LoginServer() { throw new NotImplementedException(); }

        public virtual async Task<Result<int>> GetIdRaitingTemplate() { throw new NotImplementedException(); }

        public virtual async Task<Result> GetNumberDocRaiting() { throw new NotImplementedException(); }

        public virtual async Task<Result> SaveTemplate(RaitingTemplate pRT) { throw new NotImplementedException(); }        

        public virtual async Task<Result> SaveDocRaiting(DocVM pDoc) { throw new NotImplementedException(); }

        public virtual async Task<Result<IEnumerable<RaitingTemplate>>> GetRaitingTemplateAsync() { throw new NotImplementedException(); }
        public virtual async Task<Result<IEnumerable<Doc>>> GetRaitingDocsAsync() { throw new NotImplementedException(); }
        public virtual async Task<Result<IEnumerable<DocVM>>> GetPromotion(int pCodeWarehouse) { throw new NotImplementedException(); }
        public virtual async Task<Result<IEnumerable<DocWares>>> GetPromotionData(string pNumberDoc) { throw new NotImplementedException(); }
        public virtual async Task<Result<IEnumerable<DocWaresExpirationSample>>> GetExpirationDateAsync(int pCodeWarehouse) { throw new NotImplementedException(); }

        public virtual async Task<Result<IEnumerable<ExpirationWares>>> GetDaysLeft() { throw new NotImplementedException(); }

        public virtual async Task<Result> SaveExpirationDate(DocWaresExpirationSave pED) { throw new NotImplementedException(); }


        static public PercentColor[] PercentColor = new PercentColor[0];
        
    }

    public class LoginServer
    {
        public eLoginServer Code { get;set;}   
        public string Name { get;set;}
    }


}
