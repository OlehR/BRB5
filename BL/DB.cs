﻿//using BRB5.Model;
using BRB5.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
namespace BRB5
{
    public class DB
    {
        static DB Db = null;
        public static DB GetDB(string pPathDB=null)
        {
            if (!string.IsNullOrEmpty(pPathDB))
                { BaseDir = pPathDB; Db = null; }
            if (Db == null)
                Db = new DB();
            return Db;
        }

        public SQLiteConnection db;
        const string NameDB = "BRB5.db";
        public static string BaseDir = Path.GetTempPath();

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

CREATE TABLE Doc (
    DateDoc           DATE      NOT NULL,
    TypeDoc           INTEGER   NOT NULL DEFAULT (0),
    NumberDoc         TEXT      NOT NULL,
    CodeWarehouse     INTEGER   NOT NULL DEFAULT (0),
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
    CodeReason  INTEGER,
    DTInsert    TIMESTAMP       DEFAULT (DATETIME('NOW', 'LOCALTIME') )
);
CREATE UNIQUE INDEX DocWaresTNO ON DocWares ( TypeDoc, NumberDoc, OrderDoc, CodeReason);
CREATE INDEX DocWaresTNW ON DocWares (TypeDoc ASC, NumberDoc ASC, CodeWares ASC);

CREATE TABLE DocWaresSample (
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    OrderDoc    INTEGER         NOT NULL,
    CodeWares   INTEGER         NOT NULL,
    Quantity     NUMBER,
    QuantityMin NUMBER,
    QuantityMax NUMBER, --NUMERIC (12, 3),
    Name         TEXT,
    BarCode      TEXT);
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
    Address    TEXT,
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
";

        public string PathNameDB { get { return Path.Combine(BaseDir, NameDB); } }

        public DB(string pBaseDir) : this() { BaseDir = pBaseDir; }
        public DB()
        {
            FileLogger.WriteLogMessage($"PathNameDB=>{PathNameDB}");
            
            if (!File.Exists(PathNameDB))
            {
                db = new SQLiteConnection(PathNameDB, false);
                //Створюємо базу       
                foreach (var el in SqlCreateDB.Split(';'))
                    if (el.Length > 4) 
                db.Execute(el);               
            }
            else
                db = new SQLiteConnection(PathNameDB, false);
        }
        
        public bool SetConfig<T>(string pName, T pValue)
        {
            string Value = (typeof(T) == typeof(DateTime) ? ((DateTime)(object)pValue).ToString("yyyy-MM-dd HH:mm:ss") : pValue.ToString());
            string SqlReplaceConfig  = "replace into CONFIG(NameVar, DataVar, TypeVar) values(?, ?, ?)";            
            return db.Execute(SqlReplaceConfig,  pName, pValue  ,  pValue.GetType().ToString())>0;
        }

        public T GetConfig<T>(string pStr)
        {
            T Res = default(T);
            string SqlConfig= "SELECT DataVar FROM CONFIG WHERE UPPER(NameVar) = UPPER(trim(?))";
            try
            {
                if (typeof(T).BaseType == typeof(Enum))
                { 
                    var r= db.ExecuteScalar< string>(SqlConfig,  pStr );
                    Res = (T)Enum.Parse(typeof(T), r, true);                   
                }
                else                       
                {
                    Res = db.ExecuteScalar< T>(SqlConfig,  pStr );
                }
            }
            catch (Exception e) 
            { 
            }
            return Res ;
        }

        public IEnumerable<DocWaresEx> GetDocWares(DocId pDocId, int pTypeResult, eTypeOrder pTypeOrder,int pCodeReason=0)
        {
            var DS = Config.GetDocSetting(pDocId.TypeDoc);
            string Sql = "";
            string OrderQuery = pTypeOrder == eTypeOrder.Name? "13 desc,3" :"11 desc,1";
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
                         coalesce(dws.quantity,0) as QuantityOrderStr,coalesce(dw1.quantityinput,0) as InputQuantity, coalesce(dws.quantitymin,0) as QuantityMin, 
                        coalesce(dws.quantitymax,0) as QuantityMax ,coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as QuantityOld
                      ,dw1.quantityreason as QuantityReason
                        {Color}
                        ,w.codeunit as CodeUnit
                            from Doc d  
                          join (select dw.typedoc ,dw.numberdoc, dw.codewares, sum(dw.quantity) as quantityinput,max(dw.orderdoc) as orderdoc,sum(quantityold) as quantityold,  sum(case when dw.CODEReason>0 then  dw.quantity else 0 end) as quantityreason  
                                        from docwares dw where 1=1 {Reason} group by dw.typedoc ,dw.numberdoc,codewares ) dw1 
                            on (dw1.numberdoc = d.numberdoc and d.typedoc=dw1.typedoc)
                          Left join Wares w on dw1.codewares = w.codewares 
                          left join (
                            select  dws.typedoc ,dws.numberdoc, dws.codewares,dws.name, sum(dws.quantity) as quantity,  min(dws.quantitymin) as quantitymin, max(dws.quantitymax) as quantitymax  
                                    from   DocWaresSample dws   group by dws.typedoc ,dws.numberdoc,dws.codewares,dws.name
                            ) as dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc and dws.codewares = dw1.codewares
                          where d.typedoc={pDocId.TypeDoc} and  d.numberdoc = '{pDocId.NumberDoc}'
                       union all
                       select d.TypeDoc as TypeDoc, d.numberdoc as NumberDoc, dws.orderdoc+100000, dws.CODEWARES,coalesce(dws.name,w.NAMEWARES) as NAMEWARES,coalesce(dws.quantity,0) as quantityorder,coalesce(dw1.quantityinput,0) as quantityinput, coalesce(dws.quantitymin,0) as quantitymin, coalesce(dws.quantitymax,0) as quantitymax ,coalesce(d.IsControl,0) as IsControl, coalesce(dw1.quantityold,0) as quantityold
                           ,0 as  quantityreason
                      , 3 as Ord
                      ,w.codeunit
                          from Doc d  
                          join DocWaresSample dws on d.numberdoc = dws.numberdoc and d.typedoc=dws.typedoc --and dws.codewares = w.codewares
                          left join Wares w on dws.codewares = w.codewares 
                          left join (select dw.typedoc ,dw.numberdoc, dw.codewares, sum(dw.quantity) as quantityinput,sum(dw.quantityold) as quantityold 
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
                        coalesce(dw1.quantity,0) as InputQuantityStr,
--coalesce(dw1.quantity,0) as InputQuantity,
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
                
            }
            catch (Exception e)
            {
                string msg = e.Message;
                //Utils.WriteLog("e", TAG, "GetDocWares >>", e);
            }
            return null;
        }

        public bool ReplaceDoc(IEnumerable<Doc> pDoc)
        {
            string Sql = @"replace into Doc ( DateDoc, TypeDoc, NumberDoc, CodeWarehouse, IdTemplate, ExtInfo, NameUser, BarCode, Description, State,
                                              IsControl, NumberDoc1C, DateOutInvoice, NumberOutInvoice, Color,DTStart,DTEnd) values 
                                            (@DateDoc,@TypeDoc,@NumberDoc,@CodeWarehouse,@IdTemplate,@ExtInfo,@NameUser,@BarCode,@Description,max(@State, (select max(d.state) from Doc d where d.Typedoc=@TypeDoc and d.numberdoc=@NumberDoc )),
                                             @IsControl,@NumberDoc1C,@DateOutInvoice,@NumberOutInvoice,@Color,
(select max(d.DTStart) from Doc d where d.Typedoc=@TypeDoc and d.numberdoc=@NumberDoc ),
(select max(d.DTEnd) from Doc d where d.Typedoc=@TypeDoc and d.numberdoc=@NumberDoc )
)";
            return db.ReplaceAll( pDoc) >= 0;
        }

        public IEnumerable<Doc> GetDoc(TypeDoc pTypeDoc, string pBarCode = null, string pExFilrer = null)
        {
            string Sql = $@"select d.*, Wh.Name as Address,d.State as Color from Doc d 
 left join Warehouse  Wh on d.CodeWarehouse = wh.number 
                                where TypeDoc = {pTypeDoc.CodeDoc} and DateDoc >= date(datetime(CURRENT_TIMESTAMP,'-{pTypeDoc.DayBefore} day'))" +
                                (string.IsNullOrEmpty(pBarCode)?"": $" and BarCode like'%{pBarCode}%'") +
                                (string.IsNullOrEmpty(pExFilrer) ? "" : $" and ExtInfo like'%{pExFilrer}%'") +
" order by DateDoc DESC";

            var res = db.Query< Doc>(Sql);
            if(!res.Any() && !string.IsNullOrEmpty(pBarCode))
            {
                Sql = $@"select d.*, Wh.Name as Address,d.State as Color  
from Doc d 
 left join Warehouse  Wh on d.CodeWarehouse = wh.number 
 Join DOCWARESSAMPLE dw on dw.numberdoc=d.numberdoc and dw.TypeDoc=d.TypeDoc
 join barcode bc on dw.codewares=bc.CODEWARES 
   where d.TypeDoc = {pTypeDoc.CodeDoc} and DateDoc >= date(datetime(CURRENT_TIMESTAMP,'-{pTypeDoc.DayBefore} day'))
and bc.BarCode=?
 order by DateDoc DESC";
                res = db.Query< Doc>(Sql, pBarCode );
            }
           
            return res;
        }

        public Doc GetDoc(DocId pDocId)
        {
            string Sql = $@"select d.* , Wh.Name as Address from Doc d 
   left join Warehouse Wh on d.CodeWarehouse = wh.number 
   where d.TypeDoc = {pDocId.TypeDoc} and d.numberdoc = pDocId.NumberDoc";
            var r= db.Query< Doc>(Sql);
            if(r!=null&&r.Any())
                return r.First();
            return null;
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
                    sql = $@"select dws.TypeDoc as TypeDoc, dws.NumberDoc as NumberDoc, dws.CODEWARES as CodeWares,dws.NAME as NameWares,1 as Coefficient,{Config.GetCodeUnitPiece} as CodeUnit, 'шт' as NameUnit ,
                             dws.BarCode as BarCode  ,{Config.GetCodeUnitPiece} as BaseCodeUnit  
                            from DocWaresSample dws
                         where  dws.Typedoc={pDocId.TypeDoc}  and dws.numberdoc=pDocId.NumberDoc and " + 
                                (pParseBarCode.CodeWares > 0 ? $"dws.CODEWARES={pParseBarCode.CodeWares}" : $"dws.BarCode= {pParseBarCode.BarCode}");
                    var r = db.Query< DocWaresEx>(sql);
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
                                where bc.BARCODE=?";
                        var r = db.Query<DocWaresEx>(sql, pParseBarCode.BarCode);          
                            if (r != null && r.Count() == 1)
                            res = r.First();
                        // Пошук по штрихкоду виробника
                        if (pParseBarCode.BarCode.Length == 13 && res == null)
                        {
                            sql = $@"select bc.codewares as CodeWares,bc.BARCODE as BarCode from BARCODE bc 
                                     join wares w on bc.codewares=w.codewares and w.codeunit={Config.GetCodeUnitWeight}
                                     where substr(bc.BARCODE,1,6)=?";
                            var rr = db.Query< DocWaresEx>(sql, pParseBarCode.BarCode.Substring(0, 6) );

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
                        if(res != null)  res.ParseBarCode = pParseBarCode;
                       // return res;
                    }
                }
                // Пошук по коду
                if (res == null && (pParseBarCode.CodeWares > 0 || pParseBarCode.Article >0))
                {
                    String Find = pParseBarCode.CodeWares > 0 ? $"w.code_wares={pParseBarCode.CodeWares}" : $"w.ARTICL='{pParseBarCode.Article:D8}'";
                    sql = @"select w.CODEWARES,w.NAMEWARES as NameWares, au.COEFFICIENT as Coefficient,w.CODEUNIT as CodeUnit, ud.ABRUNIT as NameUnit,
                            '' as BARCODE  ,w.CODEUNIT as BaseCodeUnit 
                                from WARES w 
                                join ADDITIONUNIT au on w.CODEWARES=au.CODEWARES and au.CODEUNIT=w.CODEUNIT 
                                join UNITDIMENSION ud on w.CODEUNIT=ud.CODEUNIT 
                                where " + Find;
                    var r = db.Query<DocWaresEx>(sql);
                    if (r != null && r.Count() == 1)
                    {
                        // @TypeDoc as TypeDoc, @NumberDoc as NumberDoc,
                        res = r.First();
                    }
                    
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
                sql = $@"select coalesce(d.IsControl,0) as IsControl, coalesce(QuantityMax,0) as QuantityMax, coalesce(quantity,0) as QuantityOrder, 
                        case when dws.Typedoc is null then 0 else 1 end as IsRecord from DOC d
                         left join DOCWARESsample dws on d.Typedoc=dws.Typedoc and d.NumberDoc=dws.NumberDoc and dws.codewares={res.CodeWares}
                         where  d.Typedoc={pDocId.TypeDoc} and d.NumberDoc='{pDocId.NumberDoc}'";
                var r = db.Query<DocWaresEx>(sql);
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
            if (res != null)
            {
                res.NumberDoc = pDocId.NumberDoc;
                res.TypeDoc = pDocId.TypeDoc;
            }
            return res;
        }
        
        public IEnumerable<RaitingTemplateItem> GetRaitingTemplateItem(RaitingTemplate pRT)
        {
            string sql = @"select * from RaitingTemplateItem rti where IdTemplate=?
        order by case when rti.Id<0 then rti.Id else rti.Parent end,  case when rti.Id<0 then 0 else rti.Id end";
            return db.Query<RaitingTemplateItem>(sql, pRT.IdTemplate);            
        }

        public IEnumerable<Model.RaitingDocItem> GetRaitingDocItem(DocId pDoc)
        {
            string sql = $@"select d.TypeDoc,d.NumberDoc,Rs.Id,Rs.Parent as Parent,Rs.Text,Rs.RatingTemplate,R.Rating,R.QuantityPhoto,R.Note,
                            Rs.OrderRS,Rs.DTDelete,Rs.ValueRating as ValueRating
        from Doc d 
         join RaitingTemplateItem as Rs on (d.IdTemplate=RS.IdTemplate ) 
         left join RaitingDocItem R on (d.TypeDoc=R.TypeDoc and d.NumberDoc=R.NumberDoc and Rs.Id=R.id)
        where d.TypeDoc={pDoc.TypeDoc} and  d.NumberDoc={pDoc.NumberDoc}
        order by case when Rs.Id<0 then Rs.Id else Rs.Parent end ,  case when Rs.Id<0 then 0 else Rs.Id end
        ";
            return db.Query<Model.RaitingDocItem>(sql);            
        }

        public bool ReplaceRaitingDocItem(Model.RaitingDocItem pR)
        {
            string Sql = @"replace into RaitingDocItem ( TypeDoc, NumberDoc, Id, Rating, QuantityPhoto, Note) values (?, ?, ?, ?, ?, ?)";
                       
            var res= db.Execute(Sql, pR.TypeDoc, pR.NumberDoc, pR.Id, pR.Rating, pR.QuantityPhoto, pR.Note) >= 0;

            Sql = $@"update doc set  DTStart = case when DTStart is null then (DATETIME('NOW', 'LOCALTIME')) else DTStart end,
        DTEnd = (DATETIME('NOW', 'LOCALTIME')) where  Typedoc={pR.TypeDoc} and numberdoc={pR.NumberDoc}";
            try
            {
                db.Execute(Sql);
            }catch (Exception ex) 
            { var s = ex.Message; }
            return res;
        }

        public bool ReplaceRaitingTemplateItem(IEnumerable<RaitingTemplateItem> pR)
        {
            string Sql = @"replace into RaitingTemplateItem (  IdTemplate, Id, Parent, Text, RatingTemplate, OrderRS,DTDelete ) values 
                                                      (@IdTemplate,@Id,@Parent,@Text,@RatingTemplate,@OrderRS,@DTDelete)";                                                   
            return db.ReplaceAll(pR) >= 0;
        }

        public bool ReplaceRaitingTemplate(IEnumerable<RaitingTemplate> pR)
        {
            string Sql = @"replace into RaitingTemplate ( IdTemplate, Text, IsActive ) values 
                                                      (@IdTemplate,@Text,@IsActive)";
            
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
            string Sql = @"replace into DocWaresSample ( TypeDoc, NumberDoc, OrderDoc, CodeWares, Quantity, QuantityMin, QuantityMax, Name, BarCode) values 
                                                       (@TypeDoc,@NumberDoc,@OrderDoc,@CodeWares,@Quantity,@QuantityMin,@QuantityMax,@Name,@BarCode)";
            return db.ReplaceAll(pDWS) >= 0;
        }

        public bool ReplaceDocWares(DocWares pDW)
        {
            //if(!pDW.GetType().Name.Equals("DocWares"))
            string Sql = $@"replace into DocWares ( TypeDoc, NumberDoc, OrderDoc, CodeWares, Quantity, QuantityOld, CodeReason) values 
                                                 ({pDW.TypeDoc},'{pDW.NumberDoc}',{pDW.OrderDoc},{pDW.CodeWares},{pDW.Quantity},{pDW.QuantityOld},{pDW.CodeReason})";
            try
            {
                return db.Execute(Sql) >= 0;
            }catch(Exception e) 
            { Console.WriteLine(e.ToString()); return false; }
        }

        public IEnumerable<Warehouse> GetWarehouse()
        {
            string Sql = "select * from Warehouse";
            //db.ExecuteScalar<int>("select count(*) from Warehouse");
            return db.Query<Warehouse>(Sql);
        }

        public bool SetStateDoc(Doc pDoc)
        {
            string Sql = $@"Update Doc set State={pDoc.State}  where TypeDoc = {pDoc.TypeDoc} and NumberDoc = {pDoc.NumberDoc}";
            return db.Execute(Sql) >= 0;
        }

        public bool ReplaceWarehouse(IEnumerable<Warehouse> pWh)
        {
            db.Execute("delete from warehouse");
            string Sql = @"replace into Warehouse ( Code, Number, Name, Url, InternalIP, ExternalIP, Location ) values 
                                                  (@Code,@Number,@Name,@Url,@InternalIP,@ExternalIP,@Location )";
            return db.ReplaceAll(pWh) >= 0;
        }

        public bool ReplaceUser(IEnumerable<User> pUser)
        {
            string Sql = @"replace into User ( CodeUser, NameUser, BarCode, Login, PassWord, TypeUser) values 
                                             (@CodeUser,@NameUser,@BarCode,@Login,@PassWord,@TypeUser)";
            return db.ReplaceAll(pUser) >= 0;
        }

        public User GetUserLogin(User pUser)
        {
            string sql = $@"select CodeUser, NameUser, BarCode, Login, PassWord, TypeUser from User where Login={pUser.Login} and PassWord={pUser.PassWord}";
            return db.Query<User>(sql)?.First();
        }

        public void InsLogPrice(LogPrice pLP)// String pBarCode, Integer pStatus, Integer pActionType, Integer pPackageNumber, Integer pCodeWarees, String pArticle, Integer pLineNumber)
        {
            string Sql = @" insert into LogPrice ( BarCode, Status,  ActionType, PackageNumber, CodeWares, LineNumber, Article) 
                                          values (@BarCode,@Status, @ActionType,@PackageNumber,@CodeWares,@LineNumber,@Article)";
            try
            {
                db.Insert( pLP);
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
                string Sql = $"Update LogPrice set NumberOfReplenishment={pNumberOfReplenishment} where  date('now', '-1 day') and is_send = 0 and LineNumber ={pLineNumber}";
                db.Execute(Sql);
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
                    db.Execute(sql);
                }
                sql = "select * from LogPrice where IsSend=-1";
                return db.Query<LogPrice>(sql);
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

            string Sql = $"SELECT {(pIsMultyLabel ? "" : "DISTINCT")} CodeWares FROM LogPrice WHERE CodeWares>0 and PackageNumber = {pPackageNumber } AND Status <= 0 AND date(DTInsert) > date('now','-1 day') {ActionType}";
            
            var n = db.Query<WaresPrice>(Sql);
            return n.Select(el=>el.CodeWares);            
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
                var res=db.Query<InitDataPriceCheck>(sql);
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
            return db.Query<PrintBlockItems>(Sql);
        }
    }
}