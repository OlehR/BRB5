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
        //public  Warehouse[] LoadWarehouse();
        private static Connector Instance = null;

        protected DB db = DB.GetDB();
        protected GetDataHTTP Http = GetDataHTTP.GetInstance();

        public static Connector GetInstance()
        {
            if (Instance == null)
            {
                if (Config.Company == eCompany.Sim23)
                    Instance = new ConnectorSE();
                else
                    Instance = new ConnectorPSU();
            }
            return Instance;
        }

        //Логін
        public virtual Result Login(string pLogin, string pPassWord, bool pIsLoginCO=false) { throw new NotImplementedException(); }
        //Завантаження довідників.
        public virtual bool LoadGuidData(bool IsFull, ObservableInt pProgress) { throw new NotImplementedException(); }

        //Робота з документами.
        //Завантаження документів в ТЗД (HTTP)
        public virtual bool LoadDocsData(int pTypeDoc, string pNumberDoc, ObservableInt pProgress, bool pIsClear) { throw new NotImplementedException(); }
        //Вивантаження документів з ТЗД (HTTP)
        public virtual Result SendDocsData(Doc pDoc, IEnumerable<DocWares> pWares, int pIsClose) { throw new NotImplementedException(); }

        //Збереження ПРосканованих товарів в 1С
        public virtual Result SendLogPrice(List<LogPrice> pList) { throw new NotImplementedException(); }

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

    }
}
