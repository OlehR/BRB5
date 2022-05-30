using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Connector
{
    public class ObservableInt { }
    public class Connector
    {
        protected static string TAG = "BRB5/Connector";
        
        private static Connector Instance = null;
        public Action<string> OnSave { get; set; }
        public bool StopSave { get; set; } = false;
        protected DB db = DB.GetDB();
        protected GetDataHTTP Http = GetDataHTTP.GetInstance();

        public static Connector GetInstance()
        {
            if (Instance == null)
            {
                switch (Config.Company)
                {
                    case eCompany.Sim23:
                        Instance = new ConnectorSE();
                        break;
                    case eCompany.Sim23FTP:
                        Instance = new ConnectorSE_FTP();
                        break;
                    case eCompany.SparPSU:
                    case eCompany.VPSU:
                        Instance = new ConnectorPSU();
                        break;
                }
            }
            return Instance;
        }

        //Логін
        public virtual Result Login(string pLogin, string pPassWord, eLoginServer pLoginServer) { throw new NotImplementedException(); }
        //Завантаження довідників.
        public virtual Result LoadGuidData(bool IsFull, ObservableInt pProgress) { throw new NotImplementedException(); }

        //Робота з документами.
        /// <summary>
        /// Завантаження документів в ТЗД (HTTP)
        /// </summary>
        /// <param name="pTypeDoc">0-всі документи</param>
        /// <param name="pNumberDoc"></param>
        /// <param name="pProgress"></param>
        /// <param name="pIsClear"></param>
        /// <returns></returns>
        public virtual Result LoadDocsData(int pTypeDoc, string pNumberDoc, ObservableInt pProgress, bool pIsClear) { throw new NotImplementedException(); }


        /// <summary>
        /// Вивантаження документів з ТЗД (HTTP)
        /// </summary>
        /// <param name="pDoc"></param>
        /// <param name="pWares"></param>
        /// <param name="pIsClose"></param>
        /// <returns></returns>
        public virtual Result SendDocsData(Doc pDoc, IEnumerable<DocWares> pWares, int pIsClose) { throw new NotImplementedException(); }

        //Збереження ПРосканованих товарів в 1С
        public virtual Result SendLogPrice(List<LogPrice> pList) { throw new NotImplementedException(); }

        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public virtual Result SendRaiting(IEnumerable< Raiting> pR, Doc pDoc)  { throw new NotImplementedException(); }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// </summary>
        /// <returns></returns>
        public virtual Result SendRaitingFiles(string NumberDoc) { throw new NotImplementedException(); }


        // Друк на стаціонарному термопринтері
        public virtual string printHTTP(List<string> codeWares) { throw new NotImplementedException(); }

        /// <summary>
        /// Розбір штрихкоду по правилам компанії
        /// </summary>
        /// <param name="pBarCode"></param>
        /// <param name="pIsOnlyBarCode"></param>
        /// <returns></returns>
        public virtual ParseBarCode ParsedBarCode(string pBarCode, bool pIsOnlyBarCode) { throw new NotImplementedException(); }

        /// <summary>
        /// Ціна on-line
        /// </summary>
        /// <param name="pBC"></param>
        /// <returns></returns>
        public virtual WaresPrice GetPrice(ParseBarCode pBC) { throw new NotImplementedException(); }

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public virtual IEnumerable< TypeDoc> GetTypeDoc(eRole pRole) { throw new NotImplementedException(); }

        public virtual IEnumerable<Warehouse> LoadWarehouse() { throw new NotImplementedException(); }

        
    }
}
