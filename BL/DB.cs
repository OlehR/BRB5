using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Utils;
public static class ProtoDB
{ 
    public static int ReplaceAll(this SQLiteConnection SQL, IEnumerable objects)
    {
        int c = 0;

        SQL.RunInTransaction(delegate
            {
                foreach (object @object in objects)
                {
                    c += SQL.InsertOrReplace(@object);
                }
            });        
        return c;
    }
}

namespace BL
{
    public class DB
    {
        static DB Db = null;
        public static DB GetDB(string pPathDB = null)
        {
            if (!string.IsNullOrEmpty(pPathDB))
            { BaseDir = pPathDB;}
            if (!string.IsNullOrEmpty(BaseDir) && Db == null)
                Db = new DB();
            return Db;
        }

        public SQLiteConnection db;
        const string NameDB = "BRB6.db";
        public static string BaseDir = null;

        const string SqlCreateDB = @"
CREATE TABLE Wares (
    CodeWares          INTEGER  NOT NULL,
    CodeGroup          INTEGER  NOT NULL,
    NameWares          TEXT     NOT NULL,
    Article              TEXT,
    CodeBrand          INTEGER,
    ArticlWaresBrand  TEXT,
    CodeUnit           INTEGER  NOT NULL,
    Description         TEXT,
    Vat                 NUMBER   NOT NULL DEFAULT 20,
    VatOperation       TEXT     NOT NULL DEFAULT 1,
    NameWaresReceipt  TEXT,
    CodeWaresRelative INTEGER,
    DaysLeft        TEXT,
    Expiration NUMBER   NOT NULL DEFAULT 0,
    DateInsert         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX WaresId ON Wares (CodeWares);

CREATE TABLE UnitDimension (
    CodeUnit   INTEGER NOT NULL,
    NameUnit   TEXT    NOT NULL,
    AbrUnit    TEXT    NOT NULL,
    Description TEXT);
CREATE UNIQUE INDEX UnitDIMENSIONId ON UnitDIMENSION (CodeUnit);

CREATE TABLE AdditionUnit (
    CodeWares   INTEGER NOT NULL,
    CodeUnit    INTEGER NOT NULL,
    Coefficient  NUMBER  NOT NULL,
    DefaultUnit INTEGER NOT NULL DEFAULT (0) );
CREATE UNIQUE INDEX AdditionUnitId ON AdditionUnit (CodeWares,CodeUnit);

CREATE TABLE BarCode (
    CodeWares INTEGER NOT NULL,
    CodeUnit  INTEGER NOT NULL,
    BarCode   TEXT    NOT NULL);
CREATE UNIQUE INDEX BarCodeId ON BarCode ( CodeWares, CodeUnit, BarCode);
CREATE UNIQUE INDEX BarCodeBC ON BarCode (BarCode);

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

CREATE TABLE Config (
    NameVar    TEXT      NOT NULL,
    DATAVar    TEXT      NOT NULL,
    TYPEVar    TEXT      NOT NULL DEFAULT 'string',
    DESCRIPTION TEXT,
    UserCreate TIMESTAMP NOT NULL  DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX ConfigId ON Config ( NameVar);

CREATE TABLE Doc (
    DateDoc           DATE      NOT NULL,
    TypeDoc           INTEGER   NOT NULL DEFAULT (0),
    NumberDoc         TEXT      NOT NULL,
    CodeWarehouse     INTEGER   NOT NULL DEFAULT (0),
    CodeReason INTEGER         DEFAULT (0),
    IdTemplate INTEGER         NOT NULL DEFAULT (0),
    ExtInfo           TEXT,
    NameUser          TEXT,
    BarCode           TEXT,
    Description        TEXT,
    State              INTEGER   DEFAULT (0),
    IsControl         INTEGER   DEFAULT (0),
    NumberDoc1C      TEXT,
    DateOutInvoice   DATE,
    NumberOutInvoice TEXT,
    Color            INTEGER,
    DTStart          TIMESTAMP DEFAULT null,
    DTEnd            TIMESTAMP DEFAULT null,
    DTInsert         TIMESTAMP DEFAULT (DATETIME('NOW', 'LOCALTIME') )
);
CREATE UNIQUE INDEX DocId ON DOC (TypeDoc,NumberDoc);

CREATE TABLE DocWares (
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT            NOT NULL,
    OrderDoc    INTEGER         NOT NULL DEFAULT (0),
    CodeWares   INTEGER         NOT NULL,
    Quantity    NUMBER  NOT NULL,   
    QuantityOld NUMBER ,   
    CodeReason  INTEGER NOT NULL DEFAULT (0),
    ExpirationDate TIMESTAMP,
    DTInsert    TIMESTAMP       DEFAULT (DATETIME('NOW', 'LOCALTIME') )
);
--CREATE INDEX DocWaresTNO ON DocWares (TypeDoc, NumberDoc, OrderDoc, CodeReason);
CREATE INDEX DocWaresTNW ON DocWares (TypeDoc, NumberDoc, CodeWares );
CREATE UNIQUE INDEX DocWaresTNT ON DocWares (TypeDoc, NumberDoc, OrderDoc );

CREATE TABLE DocWaresSample (
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    OrderDoc    INTEGER         NOT NULL,
    CodeWares   INTEGER         NOT NULL,
    CodeReason  INTEGER NOT NULL DEFAULT (0),
    Quantity     NUMBER,
    QuantityMin NUMBER,
    QuantityMax NUMBER,
    Name         TEXT,
    BarCode      TEXT,
    ExpirationDate TIMESTAMP,
    Expiration NUMBER
);
CREATE INDEX DOCWaresSampleBC ON DocWaresSample (BarCode);
CREATE UNIQUE INDEX DOCWaresSampleTNC ON DocWaresSample (TypeDoc, NumberDoc, CodeWares);

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
    Level INTEGER  DEFAULT (0),
    CodeReason INTEGER  NOT NULL,
    NameReason TEXT     NOT NULL,
    DateInsert DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX ReasonId ON Reason (Level,CodeReason);

CREATE TABLE Warehouse (
    Code       INTEGER  PRIMARY KEY NOT NULL,
    Number     TEXT,
    Name       TEXT,
    Url        TEXT,
    Address    TEXT,
    InternalIP TEXT,
    ExternalIP TEXT,
    Location TEXT,
    CodeTM INTEGER DEFAULT (0) );
CREATE INDEX WarehouseId ON Warehouse (Code);

CREATE TABLE GroupWares (
    CodeGroup INTEGER PRIMARY KEY NOT NULL,
    NameGroup TEXT,
    IsAlcohol INTEGER DEFAULT (0) 
);
CREATE INDEX GroupWaresId ON GroupWares (CodeGroup);



CREATE TABLE RaitingTemplate(
 IdTemplate    INTEGER  NOT NULL, 
 Text TEXT,
 IsActive  INTEGER  NOT NULL DEFAULT (0) 
);
CREATE UNIQUE INDEX RaitingTemplateId ON RaitingTemplate (IdTemplate);

CREATE TABLE RaitingTemplateItem(    
    IdTemplate INTEGER         NOT NULL DEFAULT (0),
    Id INTEGER  NOT NULL,
    Parent INTEGER  NOT NULL,
    Text TEXT,
    Explanation TEXT,
    RatingTemplate INTEGER         NOT NULL DEFAULT (0),
    OrderRS INTEGER,
    ValueRating         NUMBER   NOT NULL DEFAULT (0),
    DTInsert    TIMESTAMP  DEFAULT (DATETIME('NOW', 'LOCALTIME')),
    DTDelete    TIMESTAMP
);
CREATE UNIQUE INDEX RaitingTemplateItemId ON RaitingTemplateItem (IdTemplate,Id);

CREATE TABLE RaitingDocItem(
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    Id           INTEGER  NOT NULL,   
    Rating       INTEGER NOT NULL DEFAULT (0),   
    QuantityPhoto INTEGER NOT NULL DEFAULT (0),
    Note TEXT,
    DTInsert    TIMESTAMP  DEFAULT (DATETIME('NOW', 'LOCALTIME'))
);
CREATE UNIQUE INDEX RaitingDocItemId ON RaitingDocItem (TypeDoc,NumberDoc,Id);

CREATE TABLE DocWaresExpirationSample ( 
    NumberDoc   TEXT,
    DocId      TEXT NOT NULL,    
    CodeWares   INTEGER         NOT NULL,
    Quantity     NUMBER, 
    ExpirationDate TIMESTAMP,
    Expiration NUMBER,
    DaysLeft TEXT,
    OrderDoc    INTEGER NOT NULL DEFAULT (0)
);
CREATE UNIQUE INDEX DocWaresExpirationSampleTNC ON DocWaresExpirationSample 
        (NumberDoc, DocId, CodeWares);

CREATE TABLE DocWaresExpiration(   
    DateDoc    DATE  NOT NULL,
    NumberDoc   TEXT,
    DocId      TEXT NOT NULL,   
    CodeWares   INTEGER         NOT NULL,    
    QuantityInput     NUMBER,  
    ExpirationDateInput TIMESTAMP
);
CREATE UNIQUE INDEX DocWaresExpirationTNC ON DocWaresExpiration (DateDoc, NumberDoc, DocId, CodeWares);
";
        readonly int Ver = 9;
        string SqlTo6 = @"alter TABLE Reason add  Level INTEGER  DEFAULT (0);
drop index ReasonId;
CREATE UNIQUE INDEX ReasonId ON Reason (Level,CodeReason);";

        string SqlTo7 = "alter TABLE Doc add CodeReason INTEGER DEFAULT (0)";
        string SqlTo8 = "alter TABLE Warehouse add CodeTM INTEGER DEFAULT (0)";
        string SqlTo9 = "alter TABLE DocWaresSample add CodeReason  INTEGER NOT NULL DEFAULT (0)";

        public static string PathNameDB { get { return Path.Combine(BaseDir, NameDB); } }

        public DB(string pBaseDir) : this() { BaseDir = pBaseDir; }
        public DB()
        {
            FileLogger.WriteLogMessage($"PathNameDB=>{PathNameDB}");

            if (!File.Exists(PathNameDB))
            {
                CreateDB();
            }
            else
                db = new SQLiteConnection(PathNameDB, false);

            if (GetVersion < 5)
                CreateDB();
            else
            {
                if (GetVersion < 6)
                    SetSQL(SqlTo6, 6);
                if (GetVersion < 7)
                    SetSQL(SqlTo7, 7);
                if (GetVersion < 8)
                    SetSQL(SqlTo8, 8);
                if (GetVersion < 9)
                    SetSQL(SqlTo9, 9);
            }            
        }

        public bool CreateDB()
        {
            string Sql = null ;
            try
            {
                if (File.Exists(PathNameDB))
                {
                    db?.Close();
                    Task.Delay(100);
                    File.Delete(PathNameDB);
                }
                if (!File.Exists(PathNameDB))
                {
                    db = new SQLiteConnection(PathNameDB, false);
                    SetSQL(SqlCreateDB, Ver);                    
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, "DB.CreateDB", ex);
                return false;
            }
        }

        void SetSQL(string pSQL,int pVer)
        {
            foreach (var el in pSQL.Split(';'))
            {
                string Sql = el.Replace("\r\n", " ").Trim();
                if (Sql.Length > 4 && !Sql.StartsWith("--"))
                    db.Execute(Sql);
            }
            SetVersion(pVer);
        }

        public bool SetConfig<T>(string pName, T pValue)
        {
            string Value = (typeof(T) == typeof(DateTime) ? ((DateTime)(object)pValue).ToString("yyyy-MM-dd HH:mm:ss") : pValue.ToString());
            string SqlReplaceConfig = "replace into CONFIG(NameVar, DataVar, TypeVar) values(?, ?, ?)";
            return db.Execute(SqlReplaceConfig, pName, pValue, pValue.GetType().ToString()) > 0;
        }

        public T GetConfig<T>(string pStr, T Res = default)
        {
            //T Res = default;
            string SqlConfig = "SELECT DataVar FROM CONFIG WHERE UPPER(NameVar) = UPPER(trim(?))";
            try
            {
                if (typeof(T).BaseType == typeof(Enum))
                {
                    var r = db.ExecuteScalar<string>(SqlConfig, pStr);
                    if(!string.IsNullOrEmpty(r))
                        Res = (T)Enum.Parse(typeof(T), r, true);
                }
                else
                {
                    Res = db.ExecuteScalar<T>(SqlConfig, pStr);
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return Res;
        }

        public int GetVersion => db.ExecuteScalar<int>("PRAGMA user_version");

        public bool SetVersion(int pVer) => db.Execute($"PRAGMA user_version={pVer}") > 0;
        public bool ExecSQL(string pSQL) => db.Execute(pSQL) > 0;

        public IEnumerable<DocWaresEx> GetDocWares(DocId pDocId, int pTypeResult, eTypeOrder pTypeOrder, int pCodeReason = 0)
        {
            var DS = Config.GetDocSetting(pDocId.TypeDoc);
            string Sql = "";
            string OrderQuery = pTypeOrder == eTypeOrder.Name ? "13 desc,3" : "11 desc,1";
            string Reason = pCodeReason > 0 ? $" and CodeReason={pCodeReason}" : "";
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
            try
            {

                if (pTypeResult == 1)
                {
                    Sql = $@"select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dw1.orderdoc as OrderDoc, dw1.CODEWARES as CodeWares,coalesce(dws.name,w.NAMEWARES) as NameWares,
                         --coalesce(dws.quantity,0) as QuantityOrderStr,
                         coalesce(dws.quantity,0) as QuantityOrder,
                        coalesce(dw1.quantityinput,0) as InputQuantity, coalesce(dws.quantitymin,0) as QuantityMin, 
                        coalesce(dws.quantitymax,0) as QuantityMax ,coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as QuantityOld
                      ,dw1.quantityreason as QuantityReason, Max(dw1.CodeReason,dws.CodeReason ) as CodeReason
                        {Color}
                        ,w.codeunit as CodeUnit, dws.CodeReason as CodeReason
                            from Doc d  
                          join (select dw.typedoc ,dw.numberdoc, dw.codewares, sum(dw.quantity) as quantityinput,max(dw.orderdoc) as orderdoc,sum(quantityold) as quantityold,  sum(case when dw.CODEReason>0 then  dw.quantity else 0 end) as quantityreason,
                                       Max(CodeReason) as CodeReason  
                                        from docwares dw where 1=1 {Reason} group by dw.typedoc ,dw.numberdoc,codewares ) dw1 
                            on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc)
                          Left join Wares w on dw1.codewares = w.codewares 
                          left join (
                            select  dws.typedoc ,dws.numberdoc, dws.codewares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantitymin) as quantitymin, max(dws.quantitymax) as quantitymax, max(dws.CodeReason) as CodeReason
                                    from   DocWaresSample dws   group by dws.typedoc ,dws.numberdoc,dws.codewares,dws.name
                            ) as dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc and dws.codewares = dw1.codewares
                          where d.typedoc={pDocId.TypeDoc} and  d.numberdoc = '{pDocId.NumberDoc}'
                       union all
                       select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dws.orderdoc+100000, dws.CODEWARES,coalesce(dws.name,w.NAMEWARES) as NAMEWARES,coalesce(dws.quantity,0) as quantityorder,coalesce(dw1.quantityinput,0) as quantityinput, coalesce(dws.quantitymin,0) as quantitymin, coalesce(dws.quantitymax,0) as quantitymax ,coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as quantityold
                           ,0 as  quantityreason, Max(dw1.CodeReason,dws.CodeReason ) as CodeReason
                      , 3 as Ord
                      ,w.codeunit, dws.CodeReason
                          from Doc d  
                          join DocWaresSample dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc --and dws.codewares = w.codewares
                          left join Wares w on dws.codewares = w.codewares 
                          left join (select dw.typedoc ,dw.numberdoc, dw.codewares, sum(dw.quantity) as quantityinput,sum(dw.quantityold) as quantityold,
                                            Max(CodeReason) as CodeReason
                                        from DocWares dw where 1=1  {Reason} group by dw.typedoc ,dw.numberdoc,codewares) dw1 
                                 on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc and dw1.codewares = dws.codewares)
                          where dw1.TypeDoc is null and d.typedoc={pDocId.TypeDoc} and  d.numberdoc = '{pDocId.NumberDoc}'
                       order by {OrderQuery}";
                    var r = db.Query<DocWaresEx>(Sql);
                    return r;
                }
                if (pTypeResult == 2)
                {
                    Sql = $@"select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dw1.orderdoc as OrderDoc, dw1.CODEWARES as CodeWares,coalesce(dws.name,w.NAMEWARES) as NameWares,
                        coalesce(dws.quantity,0) as QuantityOrderStr,
                        --coalesce(dw1.quantity,0) as InputQuantityStr,
                        coalesce(dw1.quantity,0) as InputQuantity,
                        coalesce(dws.quantitymin,0) as QuantityMin, coalesce(dws.quantitymax,0) as QuantityMax ,
                        coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as QuantityOld,dw1.CODEReason as  CodeReason
                        ,0 as Ord,w.codeunit
                            from Doc d 
                            join DocWares dw1 on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc)
                            left join Wares w on dw1.codewares = w.codewares 
                            left join (
                            select  dws.typedoc ,dws.numberdoc, dws.codewares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantitymin) as quantitymin, max(dws.quantitymax) as quantitymax  
                                    from   DocWaresSample dws   group by dws.typedoc ,dws.numberdoc,dws.codewares,dws.name
                            ) as dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc and dws.codewares = dw1.codewares
                            where d.typedoc={pDocId.TypeDoc} and  d.numberdoc = '{pDocId.NumberDoc}'
                         order by dw1.orderdoc desc";
                    var r = db.Query<DocWaresEx>(Sql);
                    return r;
                }
                if (pTypeResult == 3)
                {
                    Sql = $@"select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, 
max(dw1.orderdoc) as OrderDoc, dw1.CODEWARES as CodeWares,
    coalesce(dws.name,w.NAMEWARES) as NameWares,
                        sum(coalesce(dws.quantity,0)) as QuantityOrderStr,
                        --coalesce(dw1.quantity,0) as InputQuantityStr,
                        sum(coalesce(dw1.quantity,0)) as InputQuantity,
                        sum(coalesce(dws.quantitymin,0)) as QuantityMin, 
                        sum(coalesce(dws.quantitymax,0)) as QuantityMax ,
                        sum(coalesce(d.IsControl,0)) as IsControl, 
                        sum(coalesce(dw1.quantityold,0)) as QuantityOld,
                        dw1.CODEReason as  CodeReason
                        ,0 as Ord,
                        w.codeunit as codeunit
                        ,(  select --dw.CodeReason,dw.quantity,r.NameReason--
group_concat(dw.CodeReason ||':' || dw.quantity ||':' ||r.NameReason ,';') 
from  DocWares dw 
    left join  Reason r on dw.CodeReason=r.CodeReason where dw1.numberdoc = dw.numberdoc and dw.typedoc=dw1.typedoc and dw.CodeWares=dw1.CodeWares ) as StrReason
                            from Doc d 
                            join DocWares dw1 on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc)
                            left join Wares w on dw1.codewares = w.codewares 
                            left join (
                            select  dws.typedoc ,dws.numberdoc, dws.codewares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantitymin) as quantitymin, max(dws.quantitymax) as quantitymax  
                                    from   DocWaresSample dws   group by dws.typedoc ,dws.numberdoc,dws.codewares,dws.name
                            ) as dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc and dws.codewares = dw1.codewares
                            where d.typedoc={pDocId.TypeDoc} and  d.numberdoc = '{pDocId.NumberDoc}'
                            group by d.TypeDoc, d.numberdoc,   dw1.CODEWARES ,coalesce(dws.name,w.NAMEWARES)
                         order by dw1.orderdoc desc";
                    var r = db.Query<DocWaresEx>(Sql);
                    return r;
                }

            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return null;
        }

        public bool ReplaceDoc(IEnumerable<Doc> pDoc,int pTypeDoc=0)
        {
            if (pTypeDoc != 0)
                db.Execute($"delete from Doc where TypeDoc={pTypeDoc}");

            string Sql = @"replace into Doc ( DateDoc, TypeDoc, NumberDoc, CodeWarehouse, CodeReason, IdTemplate, ExtInfo, NameUser, BarCode, Description, State,
                                              IsControl, NumberDoc1C, DateOutInvoice, NumberOutInvoice, Color,DTStart,DTEnd) values 
                                            (@DateDoc,@TypeDoc,@NumberDoc,@CodeWarehouse,@CodeReason,@IdTemplate,@ExtInfo,@NameUser,@BarCode,@Description,max(@State, (select max(d.state) from Doc d where d.Typedoc=@TypeDoc and d.numberdoc=@NumberDoc )),
                                             @IsControl,@NumberDoc1C,@DateOutInvoice,@NumberOutInvoice,@Color,
(select max(d.DTStart) from Doc d where d.Typedoc=@TypeDoc and d.numberdoc=@NumberDoc ),
(select max(d.DTEnd) from Doc d where d.Typedoc=@TypeDoc and d.numberdoc=@NumberDoc )
)";
            //return db.ReplaceAll( pDoc) >= 0;
            int c = 0;
            Sql = $@"replace into Doc ( DateDoc, TypeDoc, NumberDoc, CodeWarehouse,CodeReason, IdTemplate, ExtInfo, NameUser, BarCode, Description, State,
                                              IsControl, NumberDoc1C, DateOutInvoice, NumberOutInvoice, Color,DTStart,DTEnd) values 
                                            (?,?,?,?,case when ?>0 then ? else (select max(d.CodeReason) from Doc d where d.Typedoc=? and d.numberdoc=? ) end,
                                             ?,?,?,?,?,max(?, (select max(d.state) from Doc d where d.Typedoc=? and d.numberdoc=? )),
                                             ?,?,?,?,?,
(select max(d.DTStart) from Doc d where d.Typedoc=? and d.numberdoc=? ),
(select max(d.DTEnd) from Doc d where d.Typedoc=? and d.numberdoc=? )
)";
            try
            {
                db.RunInTransaction(delegate
                {
                    foreach (Doc d in pDoc)
                    {
                        c += db.Execute(Sql, d.DateDoc, d.TypeDoc, d.NumberDoc, d.CodeWarehouse, d.CodeReason, d.CodeReason, d.TypeDoc, d.NumberDoc,
                                             d.IdTemplate, d.ExtInfo, d.NameUser, d.BarCode, d.Description, d.State, d.TypeDoc, d.NumberDoc,
                                             d.IsControl, d.NumberDoc1C, d.DateOutInvoice, d.NumberOutInvoice, d.Color,
                                             d.TypeDoc, d.NumberDoc, d.TypeDoc, d.NumberDoc);
                    }
                });
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return c > 0;
        }

        public IEnumerable<DocVM> GetDoc(TypeDoc pTypeDoc, string pBarCode = null, string pExFilrer = null)
        {
            string Sql = $@"select d.*, Wh.Address as Address,d.State as Color from Doc d 
 left join Warehouse  Wh on d.CodeWarehouse = wh.number 
                                where TypeDoc = {pTypeDoc.CodeDoc} and DateDoc >= date(datetime(CURRENT_TIMESTAMP,'-{pTypeDoc.DayBefore} day'))" +
                                (string.IsNullOrEmpty(pBarCode) ? "" : $" and BarCode like'%{pBarCode}%'") +
                                (string.IsNullOrEmpty(pExFilrer) ? "" : $" and ExtInfo like'%{pExFilrer}%'") +@"
 order by DateDoc DESC";

            var res = db.Query<DocVM>(Sql);
            if (!res.Any() && !string.IsNullOrEmpty(pBarCode))
            {
                Sql = $@"select d.*, Wh.Address as Address,d.State as Color  
from Doc d 
 left join Warehouse  Wh on d.CodeWarehouse = wh.number 
 Join DOCWARESSAMPLE dw on dw.numberdoc=d.numberdoc and dw.TypeDoc=d.TypeDoc
 join barcode bc on dw.codewares=bc.CODEWARES 
   where d.TypeDoc = {pTypeDoc.CodeDoc} and DateDoc >= date(datetime(CURRENT_TIMESTAMP,'-{pTypeDoc.DayBefore} day'))
and bc.BarCode=?
 order by DateDoc DESC";
                res = db.Query<DocVM>(Sql, pBarCode);
            }
            return res;
        }

        public DocVM GetDoc(DocId pDocId)
        {
            string Sql = $@"select d.* , Wh.Name as Address from Doc d 
   left join Warehouse Wh on d.CodeWarehouse = wh.number 
   where d.TypeDoc = {pDocId.TypeDoc} and d.numberdoc = '{pDocId.NumberDoc}'";
            var r = db.Query<DocVM>(Sql);
            if (r != null && r.Any())
                return r.First();
            return null;
        }

        public ParseBarCode GetCodeWares(ParseBarCode pParseBarCode)
        {
            long Res=0;
            string sql = $@"select w.CODEWARES as CodeWares,w.NAMEWARES as NameWares,au.COEFFICIENT as Coefficient,bc.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                                 bc.BARCODE as BarCode ,w.CODEUNIT as BaseCodeUnit 
                                from BARCODE bc 
                                join ADDITIONUNIT au on bc.CODEWARES=au.CODEWARES and au.CODEUNIT=bc.CODEUNIT 
                                join wares w on w.CODEWARES=bc.CODEWARES 
                                join UNITDIMENSION ud on bc.CODEUNIT=ud.CODEUNIT 
                                where bc.BARCODE=?";
      
            var rr1 = db.Query<AdditionUnit>(sql, pParseBarCode.BarCode);
            if (rr1 != null && rr1.Count == 1)
            {
                var r = rr1.FirstOrDefault();
                if (r?.CodeWares > 0)
                {
                    pParseBarCode.CodeWares = r.CodeWares;
                    pParseBarCode.Coefficient = r.Coefficient;
                    pParseBarCode.CodeUnit = r.CodeUnit;
                }                
            }            
            else            
            if (pParseBarCode.BarCode.Length == 13 && pParseBarCode.CodeWares>0)
            {
                sql = $@"select bc.codewares as CodeWares,bc.BARCODE as BarCode from BARCODE bc 
                                     join wares w on bc.codewares=w.codewares and w.codeunit={Config.GetCodeUnitWeight}
                                     where substr(bc.BARCODE,1,6)=?";
                var rr = db.Query<DocWaresEx>(sql, pParseBarCode.BarCode[..6]);
                foreach (var el in rr)
                {
                    if (pParseBarCode.BarCode[..el.BarCode.Length].Equals(el.BarCode))
                    {
                        pParseBarCode.CodeWares = el.CodeWares;
                        pParseBarCode.Quantity = pParseBarCode.BarCode[8..12].ToDecimal();                      
                        break;
                    }
                }
            }          
            return pParseBarCode;
        }
        public DocWaresEx GetScanData(DocId pDocId, ParseBarCode pParseBarCode)
        {
            DocWaresEx res = null;
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
                    sql = $@"select dws.TypeDoc as TypeDoc, dws.NumberDoc as NumberDoc, dws.CODEWARES as CodeWares,dws.NAME as NameWares,1 as Coefficient,{Config.GetCodeUnitPiece} as CodeUnit, 'шт' as NameUnit ,
                             dws.BarCode as BarCode  ,{Config.GetCodeUnitPiece} as BaseCodeUnit  
                            from DocWaresSample dws
                         where  dws.Typedoc={pDocId.TypeDoc}  and dws.numberdoc=pDocId.NumberDoc and " +
                                (pParseBarCode.CodeWares > 0 ? $"dws.CODEWARES={pParseBarCode.CodeWares}" : $"dws.BarCode= {pParseBarCode.BarCode}");
                    var r = db.Query<DocWaresEx>(sql);
                    if (r != null && r.Count == 1)
                        res = r.First();
                }
                else
                {
                    if(!string.IsNullOrEmpty(pParseBarCode.BarCode) && pParseBarCode.CodeWares == 0 && pParseBarCode.Article == 0)
                        GetCodeWares(pParseBarCode);

                    if(pParseBarCode.CodeWares > 0 || pParseBarCode.Article > 0)
                    {
                        String Find = pParseBarCode.CodeWares > 0 ? $"w.CodeWares={pParseBarCode.CodeWares}" : $"w.ARTICLE='{pParseBarCode.Article:D8}'";
                        sql = @"select w.CODEWARES,w.NAMEWARES as NameWares, au.COEFFICIENT as Coefficient,w.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                            '' as BARCODE  ,w.CODEUNIT as BaseCodeUnit 
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT 
                                where " + Find;
                        var r = db.Query<DocWaresEx>(sql);
                        if (r != null && r.Count == 1)
                            res = r.First();
                    }
                } 
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            if (res != null && pDocId.NumberDoc != null)
            {
                res.ParseBarCode = pParseBarCode;
                //res.QuantityBarCode = pParseBarCode.Quantity;
                sql = $@"select coalesce(d.IsControl,0) as IsControl, coalesce(QuantityMax,0) as QuantityMax, coalesce(quantity,0) as QuantityOrder, 
                        case when dws.Typedoc is null then 0 else 1 end as IsRecord from DOC d
                         left join DOCWARESsample dws on d.Typedoc=dws.Typedoc and d.NumberDoc=dws.NumberDoc and dws.codewares={res.CodeWares}
                         where  d.Typedoc={pDocId.TypeDoc} and d.NumberDoc='{pDocId.NumberDoc}'";
                var r = db.Query<DocWaresEx>(sql);
                if (r != null && r.Count == 1)
                {
                    var el = r.First();
                    res.QuantityMax = el.QuantityMax;
                    res.IsControl = el.IsControl;
                    res.QuantityOrder = el.QuantityOrder;
                    res.IsRecord = el.IsRecord;
                }
            }
            
            if (res != null)
            {
                res.NumberDoc = pDocId.NumberDoc;
                res.TypeDoc = pDocId.TypeDoc;
                res.ParseBarCode = pParseBarCode;
            }
            return res;
        }

        public IEnumerable<RaitingTemplateItem> GetRaitingTemplateItem(RaitingTemplate pRT)
        {
            string sql = @"select * from RaitingTemplateItem rti where IdTemplate=?
        order by case when rti.Id<0 then rti.Id else rti.Parent end,  case when rti.Id<0 then 0 else rti.Id end";
            return db.Query<RaitingTemplateItem>(sql, pRT.IdTemplate);
        }

        public IEnumerable<RaitingDocItem> GetRaitingDocItem(DocId pDoc)
        {
            string sql = $@"select d.TypeDoc,d.NumberDoc,Rs.Id, Rs.Parent as Parent, Rs.Text, Rs.Explanation, Rs.RatingTemplate, R.Rating, R.QuantityPhoto, R.Note,
                            Rs.OrderRS, Rs.DTDelete, Rs.ValueRating as ValueRating
        from Doc d 
         join RaitingTemplateItem as Rs on (d.IdTemplate=RS.IdTemplate ) 
         left join RaitingDocItem R on (d.TypeDoc=R.TypeDoc and d.NumberDoc=R.NumberDoc and Rs.Id=R.id)
        where d.TypeDoc={pDoc.TypeDoc} and  d.NumberDoc='{pDoc.NumberDoc}'
        order by case when Rs.Id<0 then Rs.Id else Rs.Parent end ,  case when Rs.Id<0 then 0 else Rs.Id end";
            return db.Query<RaitingDocItem>(sql);
        }

        public bool ReplaceRaitingDocItem(RaitingDocItem pR)
        {
            string Sql = @"replace into RaitingDocItem ( TypeDoc, NumberDoc, Id, Rating, QuantityPhoto, Note) values (?, ?, ?, ?, ?, ?)";

            var res = db.Execute(Sql, pR.TypeDoc, pR.NumberDoc, pR.Id, pR.Rating, pR.QuantityPhoto, pR.Note) >= 0;

            Sql = $@"update doc set  DTStart = case when DTStart is null then (DATETIME('NOW', 'LOCALTIME')) else DTStart end,
        DTEnd = (DATETIME('NOW', 'LOCALTIME')) where  Typedoc={pR.TypeDoc} and numberdoc={pR.NumberDoc}";
            try
            {
                db.Execute(Sql);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return res;
        }

        public bool ReplaceRaitingTemplateItem(IEnumerable<RaitingTemplateItem> pR)
        {
            //string Sql = @"replace into RaitingTemplateItem (  IdTemplate, Id, Parent, Text, RatingTemplate, OrderRS,DTDelete ) values 
            //                                         (@IdTemplate,@Id,@Parent,@Text,@RatingTemplate,@OrderRS,@DTDelete)";                                                   
            foreach (var el in pR) el.Explanation=el.Explanation??el.Text; //!!!TMP Поки не буде приходитиз сервера
            
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceRaitingTemplate(IEnumerable<RaitingTemplate> pR )
        {
            //string Sql = @"replace into RaitingTemplate ( IdTemplate, Text, IsActive ) values (@IdTemplate,@Text,@IsActive)";
            return db.ReplaceAll(pR) >= 0;
        }
        /*public int GetIdRaitingTemplate()
        {
            string Sql = @"select coalesce(max(IdTemplate),0)+1 from RaitingTemplate ";
            return db.ExecuteScalar<int>(Sql);
        }*/

        public int GetIdRaitingTemplateItem(RaitingTemplate pD)
        {
            string Sql = $@"select coalesce(max(Id),0)+1 from RaitingTemplateItem rs where rs.IdTemplate={pD.IdTemplate}";
            return db.ExecuteScalar<int>(Sql);
        }
        public IEnumerable<RaitingTemplate> GetRaitingTemplate()
        {
            string Sql = @"select * from RaitingTemplate";
            return db.Query<RaitingTemplate>(Sql);
        }

        public bool ReplaceDocWaresSample(IEnumerable<DocWaresSample> pDWS)
        {
            int rr = pDWS.Where(r => r.CodeReason > 0).Count();
            rr++;
           // string Sql = @"replace into DocWaresSample ( TypeDoc, NumberDoc, OrderDoc, CodeWares, Quantity, QuantityMin, QuantityMax, Name, BarCode, ExpirationDate, Expiration) values 
           //                                           (@TypeDoc,@NumberDoc,@OrderDoc,@CodeWares,@Quantity,@QuantityMin,@QuantityMax,@Name,@BarCode,@ExpirationDate,@Expiration)";
            return db.ReplaceAll(pDWS) >= 0;
        }

        public bool ReplaceDocWares(DocWares pDW, bool pIsDelete = false)
        {
            try
            {
                if (pIsDelete)
                    db.Execute($"delete from DocWares where TypeDoc={pDW.TypeDoc} and NumberDoc='{pDW.NumberDoc}' and CodeWares={pDW.CodeWares}");

                if (pDW.OrderDoc == 0)
                    pDW.OrderDoc = db.ExecuteScalar<int>($"select coalesce(max(OrderDoc),0)+1 from DocWares where TypeDoc={pDW.TypeDoc} and NumberDoc='{pDW.NumberDoc}'");

                string Sql = $@"replace into DocWares ( TypeDoc, NumberDoc, OrderDoc, CodeWares, Quantity, QuantityOld, CodeReason,ExpirationDate) values 
                                                 ({pDW.TypeDoc},'{pDW.NumberDoc}',{pDW.OrderDoc},{pDW.CodeWares},{pDW.Quantity},{pDW.QuantityOld},{pDW.CodeReason},'{pDW.ExpirationDate}')";

                return db.Execute(Sql) >= 0;
            }
            catch (Exception e)
            { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); return false; }
        }

        public IEnumerable<Warehouse> GetWarehouse()
        {
            string Sql = "select * from Warehouse";
            //db.ExecuteScalar<int>("select count(*) from Warehouse");
            return db.Query<Warehouse>(Sql);
        }

        public bool SetStateDoc(DocVM pDoc)
        {
            string Sql = $@"Update Doc set State={pDoc.State}  where TypeDoc = {pDoc.TypeDoc} and NumberDoc = '{pDoc.NumberDoc}'";
            return db.Execute(Sql) >= 0;
        }

        public bool ReplaceWarehouse(IEnumerable<Warehouse> pWh,bool pIsFull=false)
        {
            if (pIsFull) db.Execute("delete from warehouse");
            //string Sql = @"replace into Warehouse ( Code, Number, Name, Url, InternalIP, ExternalIP, Location ) values 
            //                                      (@CodeWarehouse,@Number,@Name,@Url,@InternalIP,@ExternalIP,@Location )";
            return db.ReplaceAll(pWh) >= 0;
        }

        public bool ReplaceUser(IEnumerable<User> pUser)
        {
            //string Sql = @"replace into User ( CodeUser, NameUser, BarCode, Login, PassWord, TypeUser) values 
            //                                 (@CodeUser,@NameUser,@BarCode,@Login,@PassWord,@TypeUser)";
            return db.ReplaceAll(pUser) >= 0;
        }

        public User GetUserLogin(User pUser)
        {
            string sql = $@"select CodeUser, NameUser, BarCode, Login, PassWord, TypeUser from User where Login={pUser.Login} and PassWord={pUser.PassWord}";
            return db.Query<User>(sql)?.First();
        }

        public void InsLogPrice(LogPrice pLP)
        {
            string Sql = $@"insert into LogPrice ( BarCode, Status,  ActionType, PackageNumber, CodeWares, LineNumber, Article) 
                                          values ('{pLP.BarCode}',{pLP.Status}, {pLP.ActionType},{pLP.PackageNumber},{pLP.CodeWares},{pLP.LineNumber},{(string.IsNullOrEmpty( pLP.Article) ? "0":pLP.Article)})";
            db.Execute(Sql);
            /* try
             {
                 db.Insert( pLP);
             }
             catch (Exception e)
             {
                 // Utils.WriteLog("e", TAG, "InsLogPrice >>" + e.toString());
             }*/
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
                string Sql = $"Update LogPrice set NumberOfReplenishment={pNumberOfReplenishment} where  date('now', '-1 day') and is_send = 0 and LineNumber ={pLineNumber}";
                db.Execute(Sql);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
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
                    db.Execute(sql);
                }
                sql = "select * from LogPrice where IsSend=-1";
                return db.Query<LogPrice>(sql);
            }
            catch (Exception e) { FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e); }
            return null;
        }

        public IEnumerable<long> GetPrintPackageCodeWares(int pActionType, int pPackageNumber, bool pIsMultyLabel)
        {
            string ActionType = "";
            if (pActionType == 0)
                ActionType = " AND ActionType NOT IN (1,2)";
            else
                if (pActionType == 1)
                ActionType = " AND ActionType IN (1,2)";

            string Sql = $"SELECT {(pIsMultyLabel ? "" : "DISTINCT")} CodeWares FROM LogPrice WHERE CodeWares>0 and PackageNumber = {pPackageNumber} AND Status <= 0 AND date(DTInsert) > date('now','-1 day') {ActionType}";

            var n = db.Query<WaresPrice>(Sql);
            return n.Select(el => el.CodeWares);
        }

        public void AfterSendData()
        {
            db.Execute("Update LogPrice set IsSend=1 where IsSend=-1");
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
                var res = db.Query<InitDataPriceCheck>(sql);
                //return res;
                if (res != null && res.Count() > 0)
                    return res.First();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return null;
        }

        public IEnumerable<PrintBlockItems> GetPrintBlockItemsCount()
        {
            string Sql = @"select PackageNumber,count(DISTINCT case when ActionType in (1,2) then null else CodeWares end) as Normal,
                                  count(DISTINCT case when ActionType in (1,2) then CodeWares end) as Yellow 
                    from LogPrice WHERE  Status>= 0 AND date(DTInsert) > date('now','-1 day') 
                    GROUP BY PackageNumber";
            return db.Query<PrintBlockItems>(Sql);
        }


        public bool ReplaceDocWaresExpirationSample(IEnumerable<DocWaresExpirationSample> pDWS)
        {
            db.Execute("delete from DocWaresExpirationSample");
            //string Sql = @"replace into DocWaresExpirationSample ( NumberDoc, DocId, CodeWares, Quantity, ExpirationDate, Expiration, DaysLeft) values 
            //                                                     (@NumberDoc,@DocId,@CodeWares,@Quantity,@ExpirationDate,@Expiration,@DaysLeft)";
            return db.ReplaceAll(pDWS) >= 0;
        }



        public bool ReplaceDocWaresExpiration(DocWaresExpiration pDWS)
        {
            return db.InsertOrReplace(pDWS) >= 0;
        }
        
        public IEnumerable<DocExpiration> GetDocExpiration()
        {
            string sql = @"select gw.CodeGroup as NumberDoc,gw.NameGroup as Description,count(*) as Count,sum(d.CountInput) as CountInput from
(select DES.DocId,DES.CodeWares, case when de.DocId is  null then 0 else 1 end as CountInput
 from DocWaresExpirationSample DES 
   left join DocWaresExpiration DE on DES.CodeWares=DE.CodeWares and DE.DocId=DES.DocId and DATE(DE.DateDoc) = DATE('now')                                                               
                        union all 
        select  DE.DocId,DE.CodeWares,1 as nn
                                from DocWaresExpiration DE
                                left join DocWaresExpirationSample DES on DES.CodeWares=DE.CodeWares and DE.DocId=DES.DocId                                                               
                                where DATE(DE.DateDoc) = DATE('now') and DES.CodeWares is null
                                ) d
join Wares w on d.CodeWares=w.CodeWares
join GroupWares gw on w.codeGroup=gw.CodeGroup
group by gw.CodeGroup,gw.NameGroup
order by gw.NameGroup";
            return db.Query<DocExpiration>(sql);
        }

        public ExpirationDateElementVM GetScanDataExpiration(string pNumberDoc, ParseBarCode pParseBarCode)
        {
            ExpirationDateElementVM res = null;
            //Cursor mCur = null;
            string sql;

            if (pParseBarCode == null)
                return null;
            try
            {
                if (string.IsNullOrEmpty(pParseBarCode.BarCode) && pParseBarCode.CodeWares == 0 && pParseBarCode.Article == 0)
                    GetCodeWares(pParseBarCode);

                // Пошук по коду
                if (pParseBarCode.CodeWares > 0 || pParseBarCode.Article > 0)
                {
                    String Find = pParseBarCode.CodeWares > 0 ? $"w.CodeWares={pParseBarCode.CodeWares}" : $"w.ARTICLE='{pParseBarCode.Article:D8}'";
                    sql = $@"select  DES.NumberDoc,DES.DocId, w.CodeWares,w.NAMEWARES as NameWares, au.COEFFICIENT as Coefficient,w.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                            ( select group_concat(bc.BarCode,',') from BarCode bc where bc.CodeWares=w.CodeWares ) as BARCODE  ,w.CODEUNIT as BaseCodeUnit,
                            des.Quantity,des.Expiration,des.ExpirationDate,des.DaysLeft
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT 
                                join DocWaresExpirationSample DES on w.CodeWares=DES.CodeWares
                                left join DocWaresExpiration DE on DES.CodeWares=DE.CodeWares and DE.DocId=DES.DocId and DATE(DE.DateDoc) = DATE('now')                              
                                where {Find}
                            order by case when DE.CodeWares is null then 1 else 0 end,  des.ExpirationDate";
                    var r = db.Query<ExpirationDateElementVM>(sql);
                    if (r?.Count() >= 1)
                    {
                        // @TypeDoc as TypeDoc, @NumberDoc as NumberDoc,
                        res = r.First();
                    }
                    if (res == null)
                    {
                        sql = $@"select  {pNumberDoc} NumberDoc,'zz'||hex(randomblob(15)) as DocId, w.CodeWares,w.NAMEWARES as NameWares, au.COEFFICIENT as Coefficient,w.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                            ( select group_concat(bc.BarCode,',') from BarCode bc where bc.CodeWares=w.CodeWares ) as BARCODE  ,w.CODEUNIT as BaseCodeUnit,
                            0 as Quantity, w.Expiration,--des.ExpirationDate,
                            w.DaysLeft
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT                                                               
                                where " + Find;
                        r = db.Query<ExpirationDateElementVM>(sql);
                        
                        if (r?.Count() >= 1)
                        {
                            // @TypeDoc as TypeDoc, @NumberDoc as NumberDoc,
                            res = r.First();
                            res.ExpirationDate= new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
                            if (pParseBarCode.Quantity>0) res.Quantity = pParseBarCode.Quantity;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return res;
        }
        
        public IEnumerable<ExpirationDateElementVM> GetDataExpiration(string pNumberDoc)
        {
            string sql = @"select DES.OrderDoc, DES.NumberDoc,DES.DocId, w.CodeWares,w.NameWares as NameWares, au.Coefficient as Coefficient,w.CodeUnit as CodeUnit, ud.ABRUNIT as NameUnit,
                            ( select group_concat(bc.BarCode,',') from BarCode bc where bc.CodeWares=w.CodeWares ) as BARCODE  ,w.CodeUnit as BaseCodeUnit,
                            des.Quantity,des.Expiration,des.ExpirationDate,coalesce( des.DaysLeft, w.DaysLeft) as DaysLeft
,DE.ExpirationDateInput, DE.QuantityInput
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT 
                                join DocWaresExpirationSample DES on w.CodeWares=DES.CodeWares 
                                left join DocWaresExpiration DE on DES.CodeWares=DE.CodeWares and DE.DocId=DES.DocId and DATE(DE.DateDoc) = DATE('now')                             
                                where w.CodeGroup = ?                                
                        union all 
        select DES.OrderDoc, DES.NumberDoc,DES.DocId, w.CodeWares,w.NameWares as NameWares, au.Coefficient as Coefficient,w.CodeUnit as CodeUnit, ud.ABRUNIT as NameUnit,
                            ( select group_concat(bc.BarCode,',') from BarCode bc where bc.CodeWares=w.CodeWares ) as BARCODE  ,w.CodeUnit as BaseCodeUnit,
                            des.Quantity,des.Expiration,des.ExpirationDate,w.DaysLeft,
DE.ExpirationDateInput, DE.QuantityInput
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT 
                                join DocWaresExpiration DE on w.CodeWares=DE.CodeWares and DATE(DE.DateDoc) = DATE('now')
                                left join DocWaresExpirationSample DES on DES.CodeWares=DE.CodeWares and DE.DocId=DES.DocId                                                               
                                where DES.CodeWares is null and w.CodeGroup = ?
                                order by 1
";
            try
            {
                return db.Query<ExpirationDateElementVM>(sql, pNumberDoc, pNumberDoc);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return null;
        }


        public bool ReplaceWares(IEnumerable<Wares> pR, bool pIsFull = false)
        {
            if (pIsFull) ExecSQL("DELETE FROM Wares");
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceAdditionUnit(IEnumerable<AdditionUnit> pR, bool pIsFull = false)
        {
            if (pIsFull) ExecSQL("DELETE FROM AdditionUnit");
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceUnitDimension(IEnumerable<BRB5.Model.DB.UnitDimension> pR, bool pIsFull = false)
        {
            if (pIsFull) ExecSQL("DELETE FROM UnitDimension");
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceBarCode(IEnumerable<BARCode> pR, bool pIsFull = false)
        {
            if (pIsFull) ExecSQL("DELETE FROM BarCode");
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceReason(IEnumerable<BRB5.Model.DB.Reason> pR, bool pIsFull = false)
        {
            if (pIsFull) ExecSQL("DELETE FROM Reason");
            return db.ReplaceAll(pR) >= 0;
        }
        public bool ReplaceGroupWares(IEnumerable<GroupWares> pR, bool pIsFull = false)
        {
            if (pIsFull) ExecSQL("DELETE FROM GroupWares");
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceExpirationWares(IEnumerable<ExpirationWares> pEW)
        {
            int c = 0;
            try
            {
                db.RunInTransaction(delegate { foreach (var el in pEW) c += db.Execute("Update Wares set DaysLeft=? where CodeWares=?", el.DaysLeft, el.CodeWares); });
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return c > 0;
        }

        public IEnumerable<DocWaresExpiration> GetDocWaresExpiration(string pNumberDoc)
        {
            string sql = "select * from DocWaresExpiration where DATE(DateDoc) = DATE('now') --and NumberDoc=?";
            return db.Query<DocWaresExpiration>(sql);//, pNumberDoc);
        }
        public IEnumerable<BRB5.Model.DB.Reason> GetReason(eKindDoc pKindDoc,bool pIsWares=false)
        {
            string Sql = $"Select * FROM Reason where Level={(pIsWares?-1:1) *(int)pKindDoc}";
            return db.Query<BRB5.Model.DB.Reason>(Sql);
        }

        public IEnumerable<WaresAct> GetWaresAct(DocId Doc)
        {
            string sql = $@"select dw.CodeWares,sum(fact) as fact,sum(plan) as plan,w.NameWares, dw.CodeReason
from 
    (SELECT dw.CodeWares, sum(dw.Quantity) as Fact,0 as plan , dw.CodeReason from DocWares  dw 
        where dw.TypeDoc={Doc.TypeDoc} and dw.NumberDoc= '{Doc.NumberDoc}'
        group by dw.CodeWares
     union all
     SELECT dw.CodeWares, 0 as Fact,sum(dw.Quantity) as plan, dw.CodeReason  from DocWaresSample  dw 
        where dw.TypeDoc={Doc.TypeDoc}  and dw.NumberDoc= '{Doc.NumberDoc}'
     group by dw.CodeWares) dw
     join wares w on dw.CodeWares=w.CodeWares
group by dw.CodeWares, w.NameWares
";
            try
            {
                return db.Query<WaresAct>(sql);
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage(this, System.Reflection.MethodBase.GetCurrentMethod().Name, e);
            }
            return null;
        }

        public bool SetDocReason(Doc Doc)
        {
            string Sql = $@"Update Doc set CodeReason={Doc.CodeReason}  where TypeDoc = {Doc.TypeDoc} and NumberDoc = '{Doc.NumberDoc}'";
            return db.Execute(Sql) >= 0;
        }
    }
}