//using BRB5.Model;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.IO;
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
    TYPEVar    TEXT      NOT NULL DEFAULT 'String',
    DESCRIPTION TEXT,
    UserCreate TIMESTAMP NOT NULL  DEFAULT CURRENT_TIMESTAMP);
CREATE UNIQUE INDEX ConfigId ON Config ( NameVar);

CREATE TABLE DOC (
    DateDoc           DATE      NOT NULL,
    TypeDoc           INTEGER   NOT NULL DEFAULT (0),
    NumberDoc         TEXT      NOT NULL,
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
    RatingTemplate INTEGER         NOT NULL DEFAULT (0));
CREATE UNIQUE INDEX RaitingSampleId ON RaitingSample (TypeDoc,Id);

CREATE TABLE Raiting(
    TypeDoc     INTEGER         NOT NULL DEFAULT (0),
    NumberDoc   TEXT,
    Id           INTEGER  NOT NULL,   
    Rating       INTEGER NOT NULL DEFAULT (0),   
    QuantityPhoto INTEGER NOT NULL DEFAULT (0),
    Note TEXT);
CREATE UNIQUE INDEX RaitingId ON Raiting (TypeDoc,Id);";

        public SQLite db;
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
            
            var PathNameDB=Path.Combine(Dir, NameDB);

            if (!File.Exists(PathNameDB))
            {
                //var receiptFilePath = Path.GetDirectoryName(ReceiptFile);
                //if (!Directory.Exists(receiptFilePath))
                //    Directory.CreateDirectory(receiptFilePath);
                //Створюємо щоденну табличку з чеками.
                db = new SQLite(PathNameDB);
                db.ExecuteNonQuery(SqlCreateDB);
                //db.Close();
                //db = null;
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


        public bool ReplaceDoc(IEnumerable<Doc> pDoc)
        {
            string Sql = @"replace into Doc ( DateDoc, TypeDoc, NumberDoc, ExtInfo, NameUser, BarCode, Description, State, IsControl, NumberDoc1C, DateOutInvoice, NumberOutInvoice, Color) values 
                                            (@DateDoc,@TypeDoc,@NumberDoc,@ExtInfo,@NameUser,@BarCode,@Description,@State,@IsControl,@NumberDoc1C,@DateOutInvoice,@NumberOutInvoice,@Color)";
           
                return db.BulkExecuteNonQuery<Doc>(Sql,pDoc) >= 0;
        }

        public IEnumerable<Doc> GetDoc(int pTypeDoc)
        {
            string Sql = "select * from Doc where TypeDoc= @TypeDoc ";
            return db.Execute<object, Doc>(Sql, new  { TypeDoc = pTypeDoc });
        }

        public IEnumerable<Raiting> GetRating(DocId pDoc)
        {
            string sql = @"select Rs.TypeDoc,Rs.NumberDoc,Rs.Id,Rs.Parent,Rs.IsHead,Rs.Text,Rs.RatingTemplate,R.Rating,R.QuantityPhoto,R.Note
        from RaitingSample as Rs
        left join Raiting R on Rs.TypeDoc=R.TypeDoc and  Rs.NumberDoc=R.NumberDoc and Rs.Id=R.id
        where Rs.TypeDoc=@TypeDoc and  Rs.NumberDoc=@NumberDoc";
            return db.Execute<DocId, Raiting>(sql, pDoc);
        }

        public bool ReplaceRaiting(Raiting pR)
        {
            string Sql = @"replace into Raiting ( TypeDoc, NumberDoc, Id, Rating, QuantityPhoto, Note) values 
                                                (@TypeDoc,@NumberDoc,@Id,@Rating,@QuantityPhoto,@Note)";
            return db.ExecuteNonQuery<Raiting>(Sql, pR) >= 0;
        }

        public bool ReplaceRaitingSample(IEnumerable<Raiting> pR)
        {
            string Sql = @"replace into RaitingSample ( TypeDoc, NumberDoc, Id, Parent, IsHead, Text, RatingTemplate ) values 
                                                      (@TypeDoc,@NumberDoc,@Id,@Parent,@IsHead,@Text,@RatingTemplate )";
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

        public bool ReplaceWarehouse(IEnumerable<Warehouse> pWh)
        {
            string Sql = @"replace into Warehouse ( Code, Number, Name, Url, InternalIP, ExternalIP, Location ) values 
                                                  (@Code,@Number,@Name,@Url,@InternalIP,@ExternalIP,@Location )";
            return db.BulkExecuteNonQuery<Warehouse>(Sql, pWh) >= 0;
        }
    
    }
}