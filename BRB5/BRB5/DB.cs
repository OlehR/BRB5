//using BRB5.Model;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BRB5
{
    public class DB
    {
        static DB Db = null;
        public static DB GetDB()
        {
            if (Db == null)
                Db = new DB();
            return Db;
        }

        const string NameDB = "BRB5.db";
        const string SqlCreateDB = @"
CREATE TABLE AdditionUnit (
    CodeWares   INTEGER NOT NULL,
    CodeUnit    INTEGER NOT NULL,
    COEFFICIENT  NUMBER  NOT NULL,
    DefaultUnit INTEGER NOT NULL DEFAULT (0) );
CREATE UNIQUE INDEX AdditionUnitId ON AdditionUnit (CodeWares,CodeUnit);

CREATE TABLE BarCode (
    CodeWares INTEGER NOT NULL,
    CodeUnit  INTEGER NOT NULL,
    BarCode   TEXT    NOT NULL);
CREATE UNIQUE INDEX BarCodeId ON BarCode ( CodeWares, CodeUnit, BarCode);
CREATE UNIQUE INDEX BarCodeBC ON BarCode (BarCode);


CREATE TABLE Config (
    NameVar    TEXT      NOT NULL,
    DATAVar    TEXT      NOT NULL,
    TYPEVar    TEXT      NOT NULL DEFAULT 'string',
    DESCRIPTION TEXT,
    UserCreate TIMESTAMP NOT NULL  DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX ConfigId ON Config ( NameVar);

CREATE TABLE DOC (
    DateDoc           DATE      NOT NULL,
    TypeDoc           INTEGER   NOT NULL DEFAULT (0),
    NumberDoc         TEXT      NOT NULL,
    CodeWarehouse     INTEGER   NOT NULL DEFAULT (0),
    ExtInfo           TEXT,
    NameUser          TEXT,
    BarCode           TEXT,
    Description        TEXT,
    State              INTEGER   DEFAULT (0),
    IsControl         INTEGER   DEFAULT (0),
    NumberDoc1C      TEXT,
    DateOutInvoice   DATE,
    NumberOutInvoice TEXT,
    Color              INTEGER,
    DTInsert          TIMESTAMP DEFAULT (DATETIME('NOW', 'LOCALTIME') ));
CREATE UNIQUE INDEX DocId ON DOC (TypeDoc,NumberDoc);

CREATE TABLE DocWares (
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT            NOT NULL,
    OrderDoc    INTEGER         NOT NULL DEFAULT (0),
    CodeWares   INTEGER         NOT NULL,
    Quantity    NUMERIC (12, 3) NOT NULL,   
    QuantityOld NUMERIC (12, 3),   
    CodeReason  INTEGER,
    DTInsert    TIMESTAMP       DEFAULT (DATETIME('NOW', 'LOCALTIME') )
);
CREATE UNIQUE INDEX DocWaresTNO ON DocWares ( TypeDoc, NumberDoc, OrderDoc);
CREATE INDEX DocWaresTNW ON DocWares (TypeDoc ASC, NumberDoc ASC, CodeWares ASC);

CREATE TABLE DocWaresSample (
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    OrderDoc    INTEGER         NOT NULL,
    CodeWares   INTEGER         NOT NULL,
    Quantity     NUMERIC (12, 3),
    QuantityMin NUMERIC (12, 3),
    QuantityMax NUMERIC (12, 3),
    Name         TEXT,
    BarCode      TEXT);
CREATE INDEX DOCWaresSampleBC ON DocWaresSample (BarCode);

CREATE TABLE LogPrice (
    BarCode                TEXT,
    Status                  INTEGER,
    DTInsert               TIMESTAMP DEFAULT (DATETIME('NOW', 'LOCALTIME') ),
    IsSend                 INTEGER   DEFAULT 0,
    ActionType             INTEGER,
    PackageNumber          INTEGER,
    CodeWares              INTEGER,
    Article                 INTEGER,
    LineNumber             INTEGER,
    NumberOfReplenishment NUMERIC);

CREATE TABLE Reason (
    CodeReason INTEGER  NOT NULL,
    NameReason TEXT     NOT NULL,
    DateInsert DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX ReasonId ON Reason (CodeReason);

CREATE TABLE UnitDIMENSION (
    CodeUnit   INTEGER NOT NULL,
    NameUnit   TEXT    NOT NULL,
    AbrUnit    TEXT    NOT NULL,
    DESCRIPTION TEXT);
CREATE UNIQUE INDEX UnitDIMENSIONId ON UnitDIMENSION (CodeUnit);

CREATE TABLE Warehouse (
    Code       INT  PRIMARY KEY NOT NULL,
    Number     TEXT,
    Name       TEXT,
    Url        TEXT,
    InternalIP TEXT,
    ExternalIP TEXT,
    Location TEXT);
CREATE INDEX WarehouseId ON Warehouse (Code);

CREATE TABLE Wares (
    CodeWares          INTEGER  NOT NULL,
    CodeGroup          INTEGER  NOT NULL,
    NameWares          TEXT     NOT NULL,
    Articl              TEXT,
    CodeBRAND          INTEGER,
    ArticlWaresBRAND  TEXT,
    CodeUnit           INTEGER  NOT NULL,
    DESCRIPTION         TEXT,
    Vat                 NUMBER   NOT NULL DEFAULT 20,
    VatOPERATION       TEXT     NOT NULL DEFAULT 1,
    NameWaresRECEIPT  TEXT,
    CodeWaresRELATIVE INTEGER,
    DateINSERT         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX WaresId ON Wares (CodeWares);

CREATE TABLE RaitingSample(
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    Id INTEGER  NOT NULL,
    Parent INTEGER  NOT NULL,
    IsHead INTEGER  NOT NULL DEFAULT(0),
    Text TEXT,
    RatingTemplate INTEGER         NOT NULL DEFAULT (0),
    OrderRS INTEGER);
CREATE UNIQUE INDEX RaitingSampleId ON RaitingSample (TypeDoc,NumberDoc,Id);

CREATE TABLE Raiting(
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    Id           INTEGER  NOT NULL,   
    Rating       INTEGER NOT NULL DEFAULT (0),   
    QuantityPhoto INTEGER NOT NULL DEFAULT (0),
    Note TEXT);
CREATE UNIQUE INDEX RaitingId ON Raiting (TypeDoc,Id);

CREATE TABLE User (
    CodeUser   INTEGER NOT NULL,
    NameUser   TEXT NOT NULL,
    BarCode    TEXT NOT NULL,
    TypeUser  INTEGER NOT NULL,
    Login       TEXT NOT NULL,
    PassWord    TEXT NOT NULL
);
CREATE UNIQUE INDEX UserId ON User (CodeUser);
CREATE UNIQUE INDEX UserLogin ON User (Login);
";

        public SQLite db;
        public string PathNameDB;
        public DB()
        {
            string basedir = Path.GetTempPath();
            try
            {
                basedir = Device.RuntimePlatform == Device.Android ? FileSystem.AppDataDirectory : Path.GetTempPath();
            }
            catch (Exception e) { }

            var Dir = Path.Combine(basedir, "db");
            if (!Directory.Exists(Dir))
                Directory.CreateDirectory(Dir);
            
            PathNameDB=Path.Combine(Dir, NameDB);

       
            if (!File.Exists(PathNameDB))
            {
                //var receiptFilePath = Path.GetDirectoryName(ReceiptFile);
                //if (!Directory.Exists(receiptFilePath))
                //    Directory.CreateDirectory(receiptFilePath);
                //Створюємо базу
                db = new SQLite(PathNameDB);
                db.ExecuteNonQuery(SqlCreateDB);
               // db.Close();
               // db = null;
            }
            else
            {
                //var pst= Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                //string path1 = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath, Android.OS.Environment.DirectoryDownloads);
                db = new SQLite(PathNameDB);
            }
        }
        
        public bool SetConfig<T>(string pName, T pValue)
        {
            string SqlReplaceConfig  = "replace into CONFIG(NameVar, DataVar, TypeVar) values(@NameVar, @DataVar, @TypeVar)";            
            return db.ExecuteNonQuery<object>(SqlReplaceConfig, new { NameVar = pName, DataVar = pValue.ToString(), @TypeVar = pValue.GetType().ToString() })>0;
        }

        public T GetConfig<T>(string pStr)
        {
            T Res = default(T);
            string SqlConfig= "SELECT DataVar FROM CONFIG WHERE UPPER(NameVar) = UPPER(trim(@NameVar))";
            try
            {
                if (typeof(T).BaseType == typeof(Enum))
                { 
                    var r= db.ExecuteScalar<object, string>(SqlConfig, new { NameVar = pStr });
                    Res = (T)Enum.Parse(typeof(T), r, true);                   
                }
                else                       
                {
                    Res = db.ExecuteScalar<object, T>(SqlConfig, new { NameVar = pStr });
                }
            }
            catch (Exception e) 
            { 
            }
            return Res ;
        }

        public IEnumerable<DocWaresEx> GetDocWares(DocId pDocId, int pTypeResult, eTypeOrder pTypeOrder)
        {
            DocSetting DS = Config.GetDocSetting(pDocId.TypeDoc);
            string Sql = "";
            string OrderQuery = pTypeOrder == eTypeOrder.Name? "13 desc,3" :"11 desc,1";            

            string Color = " ,0 as Ord";
            if (DS.TypeColor == 1)
            {
                Color = ", case when dws.code_wares is null then 2 else 0 end as Ord\n";
            }
            else
            if (DS.TypeColor == 2)
            {
                Color = @", case when coalesce(dws.quantity,0) - coalesce(dw1.quantity_input,0) <0 then 3                                 when coalesce(dws.quantity,0) - coalesce(dw1.quantity_input,0) >0 then 2                                when coalesce(dws.quantity,0) - coalesce(dw1.quantity_input,0)=0 and quantity_reason>0 then 1                           else 0 end as Ord\n";            }            if (pTypeResult == 1)                Sql = $@"select d.Type_Doc as TypeDoc, d.number_doc as NumberDoc, dw1.order_doc as OrderDoc, dw1.CODE_WARES as CodeWares,coalesce(dws.name,w.NAME_WARES) as NameWares,                         coalesce(dws.quantity,0) as QuantityOrder,coalesce(dw1.quantity_input,0) as InputQuantity, coalesce(dws.quantity_min,0) as QuantityMin,                         coalesce(dws.quantity_max,0) as QuantityMax ,coalesce(d.Is_Control,0) as IsControl, coalesce(dw1.quantity_old,0) as QuantityOld                      ,dw1.quantity_reason as QuantityReason                        {Color}                        ,w.code_unit as CodeUnit                            from Doc d                            join (select dw.type_doc ,dw.number_doc, dw.code_wares, sum(dw.quantity) as quantity_input,max(dw.order_doc) as order_doc,sum(quantity_old) as quantity_old,  sum(case when dw.CODE_Reason>0 then  dw.quantity else 0 end) as quantity_reason  from doc_wares dw group by dw.type_doc ,dw.number_doc,code_wares) dw1 on (dw1.number_doc = d.number_doc and d.type_doc=dw1.type_doc)                          Left join wares w on dw1.code_wares = w.code_wares                           left join (                            select  dws.type_doc ,dws.number_doc, dws.code_wares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantity_min) as quantity_min, max(dws.quantity_max) as quantity_max  from   DOC_WARES_sample dws   group by dws.type_doc ,dws.number_doc,dws.code_wares,dws.name                            ) as dws on d.number_doc = dws.number_doc and d.type_doc=dws.type_doc and dws.code_wares = dw1.code_wares                          where d.type_doc=@TypeDoc and  d.number_doc = @NumberDoc                       union all                       select d.Type_Doc as TypeDoc, d.number_doc as NumberDoc, dws.order_doc+100000, dws.CODE_WARES,coalesce(dws.name,w.NAME_WARES) as NAME_WARES,coalesce(dws.quantity,0) as quantity_order,coalesce(dw1.quantity_input,0) as quantity_input, coalesce(dws.quantity_min,0) as quantity_min, coalesce(dws.quantity_max,0) as quantity_max ,coalesce(d.Is_Control,0) as Is_Control, coalesce(dw1.quantity_old,0) as quantity_old                           ,0 as  quantity_reason                      , 3 as Ord                      ,w.code_unit                          from Doc d                            join DOC_WARES_sample dws on d.number_doc = dws.number_doc and d.type_doc=dws.type_doc --and dws.code_wares = w.code_wares                          left join wares w on dws.code_wares = w.code_wares                           left join (select dw.type_doc ,dw.number_doc, dw.code_wares, sum(dw.quantity) as quantity_input,sum(dw.quantity_old) as quantity_old from doc_wares dw group by dw.type_doc ,dw.number_doc,code_wares) dw1 on (dw1.number_doc = d.number_doc and d.type_doc=dw1.type_doc and dw1.code_wares = dws.code_wares)                          where dw1.type_doc is null and d.type_doc=@TypeDoc and  d.number_doc = @NumberDoc                       order by {OrderQuery}"  ;            if (pTypeResult == 2)                Sql = $@"select d.Type_Doc as TypeDoc, d.number_doc as NumberDoc, dw1.order_doc as OrderDoc, dw1.CODE_WARES as CodeWares,coalesce(dws.name,w.NAME_WARES) as NameWares,                        coalesce(dws.quantity,0) as QuantityOrder,                        coalesce(dw1.quantity,0) as QuantityInput, coalesce(dws.quantity_min,0) as QuantityMin, coalesce(dws.quantity_max,0) as QuantityMax ,                        coalesce(d.Is_Control,0) as IsControl, coalesce(dw1.quantity_old,0) as QuantityOld,dw1.CODE_Reason as  CodeReason                        ,0 as Ord,w.code_unit                            from Doc d                             join doc_wares dw1 on (dw1.number_doc = d.number_doc and d.type_doc=dw1.type_doc)                            left join wares w on dw1.code_wares = w.code_wares                             left join (                            select  dws.type_doc ,dws.number_doc, dws.code_wares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantity_min) as quantity_min, max(dws.quantity_max) as quantity_max  from   DOC_WARES_sample dws   group by dws.type_doc ,dws.number_doc,dws.code_wares,dws.name                            ) as dws on d.number_doc = dws.number_doc and d.type_doc=dws.type_doc and dws.code_wares = dw1.code_wares                            where d.type_doc=@TypeDoc and  d.number_doc = @NumberDoc@                         order by 1,2";            try
            {
                return db.Execute<DocId,DocWaresEx>(Sql, pDocId);
            }
            catch (Exception e)
            {
                //Utils.WriteLog("e", TAG, "GetDocWares >>", e);
            }
            return null;
        }

        public bool UpdateDocState(int pState, int pTypeDoc, string pNumberDoc)
        {
            try
            {
                string Sql = "Update DOC state =@State  where Type_doc = @TypeDoc  and number_doc = @NumberDoc";
                return db.ExecuteNonQuery(Sql,new Doc(){TypeDoc=pTypeDoc,NumberDoc=pNumberDoc,State=pState } )>0;
            }
            catch (Exception e)
            {
               // Utils.WriteLog("e", TAG, "UpdateDocState >>", e);
            }
            return false;
        }

        public bool ReplaceDoc(IEnumerable<Doc> pDoc)
        {
            string Sql = @"replace into Doc ( DateDoc, TypeDoc, NumberDoc, CodeWarehouse, ExtInfo, NameUser, BarCode, Description, State, IsControl, NumberDoc1C, DateOutInvoice, NumberOutInvoice, Color) values 
                                            (@DateDoc,@TypeDoc,@NumberDoc,@CodeWarehouse,@ExtInfo,@NameUser,@BarCode,@Description,@State,@IsControl,@NumberDoc1C,@DateOutInvoice,@NumberOutInvoice,@Color)";
            return db.BulkExecuteNonQuery<Doc>(Sql, pDoc) >= 0;
        }

        public IEnumerable<Doc> GetDoc(int pTypeDoc)
        {
            string Sql = "select * from Doc where TypeDoc= @TypeDoc and DateDoc >= date(datetime(CURRENT_TIMESTAMP,'-5 day')) order by DateDoc DESC";
            return db.Execute<object, Doc>(Sql, new  { TypeDoc = pTypeDoc });
        }

        public IEnumerable<Raiting> GetRating(DocId pDoc)
        {
            string sql = @"select Rs.TypeDoc,Rs.NumberDoc,Rs.Id,Rs.Parent as Parent,Rs.IsHead,Rs.Text,Rs.RatingTemplate,R.Rating,R.QuantityPhoto,R.Note,Rs.OrderRS
        from RaitingSample as Rs
        left join Raiting R on Rs.TypeDoc=R.TypeDoc and  Rs.NumberDoc=R.NumberDoc and Rs.Id=R.id
        where Rs.TypeDoc=@TypeDoc and  Rs.NumberDoc=@NumberDoc order by case when Rs.Id<0 then Rs.Id else Rs.Parent end ,  case when Rs.Id<0 then 0 else Rs.Id end
        --order by Rs.OrderRS,Rs.Id";
            return db.Execute<DocId, Raiting>(sql, pDoc);
            /*var l = db.Execute<DocId, Raiting>(sql, pDoc). ToList(); ///.OrderBy(x=>(-x.Parent)*1000000+x.Id)
            var r = l.Where(el => el.Parent != 0);
            foreach(var el in l.Where(el=>el.Parent==0))
            {
                var index = l.FindIndex(x => x.Parent == el.Id);
                if (index > 0) index--;
                l.Insert(index, el);
            }
            return r;*/
        }

        public bool ReplaceRaiting(Raiting pR)
        {
            string Sql = @"replace into Raiting ( TypeDoc, NumberDoc, Id, Rating, QuantityPhoto, Note) values 
                                                (@TypeDoc,@NumberDoc,@Id,@Rating,@QuantityPhoto,@Note)";
            return db.ExecuteNonQuery<Raiting>(Sql, pR) >= 0;
        }

        public bool ReplaceRaitingSample(IEnumerable<Raiting> pR)
        {
            string Sql = @"replace into RaitingSample ( TypeDoc, NumberDoc, Id, Parent, IsHead, Text, RatingTemplate, OrderRS ) values 
                                                      (@TypeDoc,@NumberDoc,@Id,@Parent,@IsHead,@Text,@RatingTemplate,@OrderRS )";
            return db.BulkExecuteNonQuery<Raiting>(Sql, pR) >= 0;
        }

        public bool ReplaceDocWaresSample(IEnumerable<DocWaresSample> pDWS)
        {
            string Sql = @"replace into DocWaresSample ( TypeDoc, NumberDoc, OrderDoc, CodeWares, Quantity, QuantityMin, QuantityMax, Name, BarCode) values 
                                                       (@TypeDoc,@NumberDoc,@OrderDoc,@CodeWares,@Quantity,@QuantityMin,@QuantityMax,@Name,@BarCode)";
            return db.BulkExecuteNonQuery<DocWaresSample>(Sql, pDWS) >= 0;
        }

        public bool ReplaceDocWares(DocWares pDW)
        {
            string Sql = @"replace into DocWares ( TypeDoc, NumberDoc, OrderDoc, CodeWares, Quantity, QuantityOld, CodeReason) values 
                                                 (@TypeDoc,@NumberDoc,@OrderDoc,@CodeWares,@Quantity,@QuantityOld,@CodeReason)";
            return db.ExecuteNonQuery<DocWares>(Sql, pDW) >= 0;
        }

        public IEnumerable<Warehouse> GetWarehouse()
        {
            string Sql = "select * from Warehouse";
            return db.Execute<Warehouse>(Sql);
        }

        public bool SetStateDoc(Doc pDoc)
        {
            string Sql = @"Update Doc set State=@State  where NumberDoc= @NumberDoc";
            return db.ExecuteNonQuery<Doc>(Sql, pDoc) >= 0;
        }

        public bool ReplaceWarehouse(IEnumerable<Warehouse> pWh)
        {
            string Sql = @"replace into Warehouse ( Code, Number, Name, Url, InternalIP, ExternalIP, Location ) values 
                                                  (@Code,@Number,@Name,@Url,@InternalIP,@ExternalIP,@Location )";
            return db.BulkExecuteNonQuery<Warehouse>(Sql, pWh) >= 0;
        }

        public bool ReplaceUser(IEnumerable<User> pUser)
        {
            string Sql = @"replace into User ( CodeUser, NameUser, BarCode, Login, PassWord, TypeUser) values 
                                             (@CodeUser,@NameUser,@BarCode,@Login,@PassWord,@TypeUser)";
            return db.BulkExecuteNonQuery<User>(Sql, pUser) >= 0;
        }

        public User GetUserLogin(User pUser)
        {
            string sql = @"select CodeUser, NameUser, BarCode, Login, PassWord, TypeUser from User where Login=@Login and PassWord=@PassWord";
            return db.Execute<User, User>(sql, pUser)?.First();
        }
        public IEnumerable<LogPrice> GetSendData(int pLimit)
        {
            int varN;
            try
            {
                string sql = "select count(*) from   LogPrice where is_send=-1";
                varN = db.ExecuteScalar<int>(sql);

                if (varN < pLimit)
                {
                    sql = $"Update LogPrice set is_send=-1 where rowid  IN (SELECT rowid FROM LogPrice WHERE is_send=0 LIMIT {pLimit - varN})";
                    db.ExecuteNonQuery(sql);
                }

                sql = "select bar_code,Status,DT_insert,package_number,is_send,action_type,code_wares,article,Line_Number,Number_Of_Replenishment from LogPrice where is_send=-1";
                return db.Execute<LogPrice>(sql);
            }
            catch (Exception e) { }
            return null;
        }
        public void AfterSendData()
        {
            db.ExecuteNonQuery("Update LogPrice set is_send=1 where is_send=-1");            
        }

        public int[] GetCountScanCode()
        {
            int[] varRes = { 0, 0 };
            try
            {
                string sql = "select count(*),sum(case when Status=0 then 1 else 0 end) from LogPrice where is_send=0";

                db.ExecuteScalar<int[]>(sql);              

            }
            catch (Exception e)
            {
               // Utils.WriteLog("e", TAG, "GetCountScanCode >>" + e.tostring());
                //throw mSQLException;
            }
            return varRes;
        }
    }
}