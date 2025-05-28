using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BRB5;
using BL.Connector;
using Utils;
using UtilNetwork;

namespace BL
{
    public partial class BL
    {
        static BL Bl = null;
        public static BL GetBL()
        {
            if (Bl == null)
                Bl = new BL();
            return Bl;
        }

        public DB db = DB.GetDB();
        public BRB5.Model.Connector c = ConnectorBase.GetInstance();
        public void SendLogPrice()
        {
            for (int i = 0; i < 20; i++)
            {
                var List = db.GetSendData(100);
                if (List == null && List.Count() == 0)
                    break;

                Result res = c.SendLogPrice(List);
                if (res.State == 0)
                {
                    try
                    {
                        db.AfterSendData();
                        //int[] varRes = db.GetCountScanCode();
                        //LI.AllScan = varRes[0];
                        //LI.BadScan = varRes[1];
                    }
                    catch (Exception e)
                    {
                        //Utils.WriteLog("e", TAG, "SendLogPricePSU  >>", e);
                        break;
                    }
                }
                else
                    break;
            }

        }

        // Зміна стану документа і відправляємо в 1С
        /* public Result UpdateDocState(int pState, int pTypeDoc, String pNumberDoc, DateTime pDateOutInvoice, String pNumberOutInvoice, int pIsClose)
         {
             DocSetting DS = Config.GetDocSetting(pTypeDoc);
             if (DS != null && !DS.IsMultipleSave)
             {
                 int State = db.GetStateDoc(pTypeDoc, pNumberDoc);
                 if (State >= 1)
                     return new Result(-2, "Даний документ не можна повторно зберігати!");
             }

             db.UpdateDocState(pState, pTypeDoc, pNumberDoc);
             List<WaresItemModel> wares = db.GetDocWares(pTypeDoc, pNumberDoc, (DS == null || DS.IsSaveOnlyScan ? 2 : 1), eTypeOrder.Scan);
             return c.SyncDocsData(pTypeDoc, pNumberDoc, wares, pDateOutInvoice, pNumberOutInvoice, pIsClose);

         }*/
        public string PrintPackage(int actionType, int packageNumber, bool IsMultyLabel)
        {
            var codeWares = db.GetPrintPackageCodeWares(Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto ? -1 : actionType, packageNumber, IsMultyLabel);
            //    SetProgress(5);//priceCheckerActivity.loader.setVisibility(View.VISIBLE);
            if (Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCut || Config.TypeUsePrinter == eTypeUsePrinter.StationaryWithCutAuto)
               return c.PrintHTTP(codeWares);
            else
                foreach (long CodeWares in codeWares) {
                    PrintLabel(CodeWares);
                }
            return null;
            //SetProgress(100);
           
        }

        public void PrintLabel(long pCodeWares)
        {
         /*   boolean isError = false;
            if (codeWares == null)
                return;
            String CodeWares = codeWares.trim();

            try
            {
                LI = new PricecheckerHelper().getPriceCheckerData(LI, CodeWares, false, config);
                if (LI.resHttp != null && !LI.resHttp.isEmpty())
                {
                    LI.Init(new JSONObject(LI.resHttp));
                    if (LI.OldPrice != LI.Price || LI.OldPriceOpt != LI.PriceOpt)
                    {
                        LI.BadScan++;
                        byte[] b = new byte[0];
                        try
                        {
                            b = LI.LevelForPrinter(Printer.GetTypeLanguagePrinter());
                        }
                        catch (UnsupportedEncodingException e)
                        {
                            //e.printStackTrace();
                        }
                        try
                        {
                            Printer.sendData(b);
                        }
                        catch (IOException e)
                        {
                            //LI.InfoPrinter="Lost Connect";
                            //e.printStackTrace();
                        }
                        if (Printer.varPrinterError != ePrinterError.None)
                            LI.PrinterError = Printer.varPrinterError;
                    }
                }
            }
            catch (Exception ex)
            {
                isError = true;
            }
            return;*/
        }

        /// <summary>
        /// Отримати Товар по штрихкоду
        /// </summary>
        /// <param name="pTypeDoc"></param>
        /// <param name="pNumberDoc"></param>
        /// <param name="pBarCode"></param>
        /// <param name="pIsOnlyBarCode"></param>
        /// <returns></returns>
        public DocWaresEx GetWaresFromBarcode(int pTypeDoc, String pNumberDoc, String pBarCode, bool pIsHandInput)
        {            
            bool IsSimpleDoc = false;
            if (pTypeDoc > 0)
                IsSimpleDoc = Config.GetDocSetting(pTypeDoc).IsSimpleDoc;
            ParseBarCode PBarcode = c.ParsedBarCode(pBarCode, !pIsHandInput && !IsSimpleDoc);
            DocWaresEx res = db.GetScanData( new DocId() { TypeDoc = pTypeDoc, NumberDoc = pNumberDoc }, PBarcode);// pBarCode, pIsOnlyBarCode,false);
            //String outLog = "Null";
            if (res != null) ;
            //  outLog = res.CodeWares + "," + res.QuantityBarCode + "," + res.NameWares;
            else
              if (Config.Company == eCompany.Sim23 && pTypeDoc == 7 && PBarcode.CodeWares != 0)
            { //Якщо ревізія а товар не знайдено

                DocWaresSample DWS = new DocWaresSample() { TypeDoc = pTypeDoc, NumberDoc = pNumberDoc, OrderDoc = 100000 + (int)PBarcode.CodeWares,
                    Quantity = 1m, QuantityMax = 1d, Name = pBarCode };
                
                db.ReplaceDocWaresSample(new DocWaresSample[] { DWS});
                res = new DocWaresEx();//(DWS);
                res.Coefficient = 1;
                res.CodeUnit = Config.GetCodeUnitPiece;
                res.BaseCodeUnit = res.CodeUnit;
                res.NameUnit = "Шт";
            }

           // Utils.WriteLog("i", TAG, "SaveDocWares=>" + String.valueOf(pTypeDoc) + "," + pNumberDoc + "," + gson.toJson(PBarcode) +
            //        ",\nres=>" + outLog);
            return res;
        }

        public  Warehouse GetWarehouse(int pCodeWarehouse)
        {
            var Warehouses = db.GetWarehouse();
             
             if (Warehouses == null) return null;

             foreach (var el in Warehouses)
                 if (el.CodeWarehouse == pCodeWarehouse)
                     return el;
            return null;
        }

       
    }
}
