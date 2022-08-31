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
            string Value = (typeof(T) == typeof(DateTime) ? ((DateTime)(object)pValue).ToString("yyyy-MM-dd HH:mm:ss") : pValue.ToString());
            string SqlReplaceConfig  = "replace into CONFIG(NameVar, DataVar, TypeVar) values(@NameVar, @DataVar, @TypeVar)";            
            return db.ExecuteNonQuery<object>(SqlReplaceConfig, new { NameVar = pName, DataVar = Value  , @TypeVar = pValue.GetType().ToString() })>0;
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
            var DS = Config.GetDocSetting(pDocId.TypeDoc);
            string Sql = "";
            string OrderQuery = pTypeOrder == eTypeOrder.Name? "13 desc,3" :"11 desc,1";            

            string Color = " ,0 as Ord";
            if (DS.TypeColor == 1)
            {
                Color = ", case when dws.codewares is null then 2 else 0 end as Ord\n";
            }
            else
            if (DS.TypeColor == 2)
            {
                Color = @", case when coalesce(dws.quantity,0) - coalesce(dw1.quantityinput,0) <0 then 3 
                                when coalesce(dws.quantity,0) - coalesce(dw1.quantityinput,0) >0 then 2
                                when coalesce(dws.quantity,0) - coalesce(dw1.quantityinput,0)=0 and quantityreason>0 then 1
                           else 0 end as Ord\n";
            }

            if (pTypeResult == 1)
                Sql = $@"select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dw1.orderdoc as OrderDoc, dw1.CODEWARES as CodeWares,coalesce(dws.name,w.NAMEWARES) as NameWares,
                         coalesce(dws.quantity,0) as QuantityOrder,coalesce(dw1.quantityinput,0) as InputQuantity, coalesce(dws.quantitymin,0) as QuantityMin, 
                        coalesce(dws.quantitymax,0) as QuantityMax ,coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as QuantityOld
                      ,dw1.quantityreason as QuantityReason
                        {Color}
                        ,w.codeunit as CodeUnit
                            from Doc d  
                          join (select dw.typedoc ,dw.numberdoc, dw.codewares, sum(dw.quantity) as quantityinput,max(dw.orderdoc) as orderdoc,sum(quantityold) as quantityold,  sum(case when dw.CODEReason>0 then  dw.quantity else 0 end) as quantityreason  
                                        from docwares dw group by dw.typedoc ,dw.numberdoc,codewares) dw1 
                            on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc)
                          Left join Wares w on dw1.codewares = w.codewares 
                          left join (
                            select  dws.typedoc ,dws.numberdoc, dws.codewares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantitymin) as quantitymin, max(dws.quantitymax) as quantitymax  
                                    from   DocWaresSample dws   group by dws.typedoc ,dws.numberdoc,dws.codewares,dws.name
                            ) as dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc and dws.codewares = dw1.codewares
                          where d.typedoc=@TypeDoc and  d.numberdoc = @NumberDoc
                       union all
                       select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dws.orderdoc+100000, dws.CODEWARES,coalesce(dws.name,w.NAMEWARES) as NAMEWARES,coalesce(dws.quantity,0) as quantityorder,coalesce(dw1.quantityinput,0) as quantityinput, coalesce(dws.quantitymin,0) as quantitymin, coalesce(dws.quantitymax,0) as quantitymax ,coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as quantityold
                           ,0 as  quantityreason
                      , 3 as Ord
                      ,w.codeunit
                          from Doc d  
                          join DocWaresSample dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc --and dws.codewares = w.codewares
                          left join Wares w on dws.codewares = w.codewares 
                          left join (select dw.typedoc ,dw.numberdoc, dw.codewares, sum(dw.quantity) as quantityinput,sum(dw.quantityold) as quantityold 
                                        from DocWares dw group by dw.typedoc ,dw.numberdoc,codewares) dw1 
                                 on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc and dw1.codewares = dws.codewares)
                          where dw1.TypeDoc is null and d.typedoc=@TypeDoc and  d.numberdoc = @NumberDoc
                       order by {OrderQuery}"  ;

            if (pTypeResult == 2)
                Sql = $@"select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dw1.orderdoc as OrderDoc, dw1.CODEWARES as CodeWares,coalesce(dws.name,w.NAMEWARES) as NameWares,
                        coalesce(dws.quantity,0) as QuantityOrder,
                        coalesce(dw1.quantity,0) as QuantityInput, coalesce(dws.quantitymin,0) as QuantityMin, coalesce(dws.quantitymax,0) as QuantityMax ,
                        coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as QuantityOld,dw1.CODEReason as  CodeReason
                        ,0 as Ord,w.codeunit
                            from Doc d 
                            join DocWares dw1 on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc)
                            left join Wares w on dw1.codewares = w.codewares 
                            left join (
                            select  dws.typedoc ,dws.numberdoc, dws.codewares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantitymin) as quantitymin, max(dws.quantitymax) as quantitymax  
                                    from   DocWaresSample dws   group by dws.typedoc ,dws.numberdoc,dws.codewares,dws.name
                            ) as dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc and dws.codewares = dw1.codewares
                            where d.typedoc=@TypeDoc and  d.numberdoc = @NumberDoc@
                         order by 1,2";

            try
            {
                var r= db.Execute<DocId, DocWaresEx>(Sql, pDocId);
                return r;
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

        public DocWaresEx GetScanData(DocId pDocId, ParseBarCode pParseBarCode)
        {
            DocWaresEx res = null;
            //Cursor mCur = null;
            string sql;
            
            if (pParseBarCode == null)
                return null;
            try
            {
                bool IsSimpleDoc = false;
                if (pDocId.TypeDoc > 0)
                    IsSimpleDoc = Config.GetDocSetting(pDocId.TypeDoc).IsSimpleDoc;
                if (IsSimpleDoc)
                {
                    sql = $@"select dws.CODEWARES as CodeWares,dws.NAME as NameWares,1 as Coefficient,{Config.GetCodeUnitPiece} as CodeUnit, 'шт' as NameUnit ,
                             dws.BarCode as BarCode  ,{Config.GetCodeUnitPiece} as BaseCodeUnit  
                            from DocWaresSample dws
                         where  dws.Typedoc=@TypeDoc  and dws.numberdoc=@DocNumber and " + (pParseBarCode.CodeWares > 0 ? $"dws.CODEWARES={pParseBarCode.CodeWares}" : $"dws.BarCode= {pParseBarCode.BarCode}");
                    var r = db.Execute<DocId, DocWaresEx>(sql, pDocId);
                    if (r != null && r.Count() == 1)
                        res = r.First();
                }
                else
                {

                    if (pParseBarCode.BarCode != null)
                    {
                        sql = $@"select w.CODEWARES as CodeWares,w.NAMEWARES as NameWares,au.COEFFICIENT as Coefficient,bc.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                                 bc.BARCODE as BarCode ,w.CODEUNIT as BaseCodeUnit 
                                from BARCODE bc 
                                join ADDITIONUNIT au on bc.CODEWARES=au.CODEWARES and au.CODEUNIT=bc.CODEUNIT 
                                join wares w on w.CODEWARES=bc.CODEWARES 
                                join UNITDIMENSION ud on bc.CODEUNIT=ud.CODEUNIT 
                                where bc.BARCODE=@BarCode";
                        var r = db.Execute<object, DocWaresEx>(sql, new { BarCode = pParseBarCode.BarCode });
                        if (r != null && r.Count() == 1)
                            res = r.First();
                        // Пошук по штрихкоду виробника
                        if (pParseBarCode.BarCode.Length == 13 && res == null)
                        {
                            sql = $@"select bc.codewares as CodeWares,bc.BARCODE as BarCode from BARCODE bc 
                                     join wares w on bc.codewares=w.codewares and w.codeunit={Config.GetCodeUnitWeight}
                                     where substr(bc.BARCODE,1,6)=@BarCode";
                            var rr = db.Execute<object, DocWaresEx>(sql, new { BarCode = pParseBarCode.BarCode.Substring(0, 6) });

                            foreach (var el in rr)
                            {
                                if (pParseBarCode.BarCode.Substring(0, el.BarCode.Length).Equals(el.BarCode))
                                {
                                    pParseBarCode.CodeWares = el.CodeWares;
                                    decimal Quantity = 0m;
                                    Decimal.TryParse(pParseBarCode.BarCode.Substring(8, 12), out Quantity);
                                    pParseBarCode.Quantity = Quantity;
                                    res = GetScanData(pDocId, pParseBarCode);//CodeWares, pIsOnlyBarCode,false);                                                                  
                                }
                            }
                        }
                        res.ParseBarCode = pParseBarCode;
                        return res;
                    }
                }
                // Пошук по коду
                if (res == null && (pParseBarCode.CodeWares > 0 || pParseBarCode.Article != null))
                {
                    String Find = pParseBarCode.CodeWares > 0 ? $"w.code_wares={pParseBarCode.CodeWares}" : @"w.ARTICL='{pParseBarCode.Article}'";
                    sql = @"select w.CODEWARES,w.NAMEWARES as NameWares, au.COEFFICIENT as Coefficient,w.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                            '' as BARCODE  ,w.CODEUNIT as BaseCodeUnit 
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT 
                                where " + Find;
                    var r = db.Execute<DocWaresEx>(sql);
                    if (r != null && r.Count() == 1)
                        res = r.First();
                }

            }
            catch (Exception e)
            {
                //Utils.WriteLog("e", TAG, "GetScanData >>", e);
            }
            if (res != null && pDocId.NumberDoc != null)
            {
                res.ParseBarCode=pParseBarCode;
                //res.QuantityBarCode = pParseBarCode.Quantity;
                sql = @"select coalesce(d.IsControl,0) as IsControl, coalesce(QuantityMax,0) as QuantityMax, coalesce(quantity,0) as QuantityOrder, 
                        case when dws.Typedoc is null then 0 else 1 end as IsRecord from DOC d
                         left join DOCWARESsample dws on d.Typedoc=dws.Typedoc and d.numberdoc=dws.numberdoc and dws.codewares=@CodeWares
                         where  d.Typedoc=@TypeDoc and d.numberdoc=@DocNumber";
                var r = db.Execute<DocWaresId, DocWaresEx>(sql, new DocWaresId() { TypeDoc=pDocId.TypeDoc,NumberDoc=pDocId.NumberDoc,CodeWares=res.CodeWares});
                if (r != null && r.Count() == 1)
                {
                    var el= r.First();
                    res.QuantityMax = el.QuantityMax;
                    res.IsControl=el.IsControl;
                    res.QuantityOrder = el.QuantityOrder;
                    res.IsRecord =el.IsRecord;
                }                    

            }
            //Log.d(TAG, "Found in DB  >>" + (model == null ? "Not Found" : model.NameWares));
            return res;
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
            db.ExecuteNonQuery("delete from warehouse");
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

        public void InsLogPrice(LogPrice pLP)// String pBarCode, Integer pStatus, Integer pActionType, Integer pPackageNumber, Integer pCodeWarees, String pArticle, Integer pLineNumber)
        {
            string Sql = @" insert into LogPrice ( BarCode, Status,  ActionType, PackageNumber, CodeWares, LineNumber, Article) 
                                          values (@BarCode,@Status, @ActionType,@PackageNumber,@CodeWares,@LineNumber,@Article)";
            try
            {
                db.ExecuteNonQuery(Sql, pLP);
            }
            catch (Exception e)
            {
                // Utils.WriteLog("e", TAG, "InsLogPrice >>" + e.toString());
            }
        }

        /// <summary>
        /// Пповнення стелажу СКЮ
        /// </summary>
        /// <param name="pLineNumber"></param>
        /// <param name="pNumberOfReplenishment"></param>
        public void UpdateReplenishment(int pLineNumber, decimal pNumberOfReplenishment)
        {
            try
            {
                string Sql = "Update LogPrice set NumberOfReplenishment=@NumberOfReplenishment where  date('now', '-1 day') and is_send = 0 and LineNumber =@LineNumber";
                db.ExecuteNonQuery<object>(Sql, new { NumberOfReplenishment = pNumberOfReplenishment, LineNumber = pLineNumber });
            }
            catch (Exception e)
            {
                //Utils.WriteLog("e", TAG, "UpdateReplenishment >>" + e.toString());
            }

        }

        public IEnumerable<LogPrice> GetSendData(int pLimit)
        {
            int varN;
            try
            {
                string sql = "select count(*) from   LogPrice where IsSend=-1";
                varN = db.ExecuteScalar<int>(sql);

                if (varN < pLimit)
                {
                    sql = $"Update LogPrice set IsSend=-1 where rowid  IN (SELECT rowid FROM LogPrice WHERE IsSend=0 LIMIT {pLimit - varN})";
                    db.ExecuteNonQuery(sql);
                }
                sql = "select * from LogPrice where IsSend=-1";
                return db.Execute<LogPrice>(sql);
            }
            catch (Exception e) { }
            return null;
        }

        public IEnumerable<int> GetPrintPackageCodeWares(int pActionType, int pPackageNumber, bool pIsMultyLabel)
        {            
            string ActionType = "";
            if (pActionType == 0)
                ActionType = " AND ActionType NOT IN (1,2)";
            else
                if (pActionType == 1)
                ActionType = " AND ActionType IN (1,2)";

            string Sql = $"SELECT {(pIsMultyLabel ? "" : "DISTINCT")} CodeWares FROM LogPrice WHERE CodeWares>0 and PackageNumber = {pPackageNumber } AND Status < 0 AND date(DTInsert) > date('now','-1 day') {ActionType}";
            return db.Execute<int>(Sql);            
        }

        public void AfterSendData()
        {
            db.ExecuteNonQuery("Update LogPrice set IsSend=1 where IsSend=-1");            
        }

        public InitDataPriceCheck GetCountScanCode()
        {            
            try
            {
                string sql = @"select sum(case when IsSend=0 then 1 else 0 end) as AllScan,
                                      sum(case when Status=0 and IsSend=0 then 1 else 0 end) BadScan, 
                                      max(case when date(DTInsert) > date('now','-1 day') then PackageNumber else 0 end) as PackageNumber,
                                      max(case when date(DTInsert) > date('now','-1 day') then LineNumber else 0 end) as LineNumber
                                from LogPrice";
                var res=db.Execute<InitDataPriceCheck>(sql);
                //return res;
                if (res != null && res.Count() > 0)
                    return res.First();
            }
            catch (Exception e)
            {
               // Utils.WriteLog("e", TAG, "GetCountScanCode >>" + e.tostring());
                //throw mSQLException;
            }
            return null;
        }

        public IEnumerable<PrintBlockItems> GetPrintBlockItemsCount()
        { 
            string Sql = @"select PackageNumber,count(DISTINCT case when ActionType in (1,2) then null else CodeWares end) as Normal,
                                  count(DISTINCT case when ActionType in (1,2) then CodeWares end) as Yellow 
                    from LogPrice WHERE  Status>= 0 AND date(DTInsert) > date('now','-1 day') 
                    GROUP BY PackageNumber";            
            return db.Execute<PrintBlockItems>(Sql);
        }
    }
}