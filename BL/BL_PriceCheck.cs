using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Utils;

namespace BL
{
    public partial class BL
    {
        //WaresPrice WP;
        WaresPrice[] WPH = new WaresPrice[2];

        public void ClearWPH() { WPH[0] = null; WPH[1]=null; }
        public WaresPrice  FoundWares(string pBarCode, int PackageNumber, int LineNumber, bool pIsHandInput, bool pIsDoubleScan, bool IsOnline = true, eTypePriceInfo pTPI = eTypePriceInfo.Normal)
        {
            WaresPrice CheckWP;            
            LogPrice l;

            if (IsOnline)
            {
                CheckWP = c.GetPrice(c.ParsedBarCode(pBarCode, pIsHandInput), pTPI)?? new() { Name= $"Товар не знайдено =>{pBarCode}" };
            }
            else
            {
                var data = GetWaresFromBarcode(0, null, pBarCode, pIsHandInput);
                CheckWP = new WaresPrice(data);
            }

            if (pIsDoubleScan)
            {
                SearchDoubleScan(CheckWP,PackageNumber, LineNumber);                
            }
            if (!pIsDoubleScan|| (CheckWP.PriceOld >0&& CheckWP.Price != CheckWP.PriceOld) ||(CheckWP.PriceOptOld > 0 && CheckWP.PriceOpt != CheckWP.PriceOptOld))
            {
                //WP = CheckWP;
                CheckWP.StateDoubleScan = eCheckWareScaned.BadPrice;
                l = new LogPrice(CheckWP, IsOnline, PackageNumber, LineNumber);
                db?.InsLogPrice(l);
            }

            return CheckWP;
        }

        private void SearchDoubleScan(WaresPrice CheckWP, int PackageNumber, int LineNumber)
        {
            if (CheckWP?.CodeWares != 0){
                for (int i = WPH.Count() - 1; i >= 0; i--)
                {
                    if (WPH[i] == null) continue;
                    eCheck R = CompareDoubleScan(CheckWP, WPH[i]);
                    if (R == eCheck.Same) break;
                    if (R == eCheck.Ok)
                    {
                        SaveDoubleScan(100, CheckWP, PackageNumber, LineNumber);
                        for (int j = 0; j < WPH.Count(); j++) WPH[j] = null;
                        CheckWP.StateDoubleScan = eCheckWareScaned.Success; //Res = "Скануйте цінник чи товар";
                        break;
                    }

                    if (R == eCheck.Bad && i == 0)
                    {
                        if (WPH[1] != null)
                        {
                           // for (int j = 0; j < WPH.Count(); j--)
                                SaveDoubleScan(WPH[0].IsBarCode ? 101 : 102, CheckWP, PackageNumber, LineNumber);
                            WPH[0] = WPH[1];
                        }
                        CheckWP.StateDoubleScan = eCheckWareScaned.Bad; 
                        WPH[1] = CheckWP;
                    }
                }                
                    
                if (WPH[0] == null && CheckWP.StateDoubleScan != eCheckWareScaned.Success) WPH[0] = CheckWP;

                if(CheckWP.StateDoubleScan != eCheckWareScaned.Bad&& CheckWP.StateDoubleScan != eCheckWareScaned.Success) 
                    CheckWP.StateDoubleScan=(CheckWP.IsBarCode ? eCheckWareScaned.WareScaned : eCheckWareScaned.PriceTagScaned); //"Скануйте цінник" : "Скануйте товар");
            }            
        }
        enum eCheck { Ok,Same,Bad }
        private eCheck CompareDoubleScan(WaresPrice pWP, WaresPrice pS)
        {
            if (pWP.CodeWares == pS.CodeWares)
                if (pWP.IsBarCode == !pS.IsBarCode) return eCheck.Ok; else return eCheck.Same;
            return eCheck.Bad;
        }

        /*private string SearchDoubleScan(WaresPrice CheckWP)
        {
            string MessageDoubleScan;
            if (WP == null)
            {
                WP = CheckWP;
                if (WP.ParseBarCode.BarCode == null)
                {
                    MessageDoubleScan = "Скануйте товар";
                    WP.StateDoubleScan = eCheckWareScaned.PriceTagScaned;
                }
                else
                {
                    MessageDoubleScan = "Скануйте цінник";
                    WP.StateDoubleScan = eCheckWareScaned.WareScaned;
                }
            }
            else
            {
                if (WP.StateDoubleScan != eCheckWareScaned.Nothing && WP.StateDoubleScan != eCheckWareScaned.Success)
                {   
                    //поточний цінник
                    if (WP.ParseBarCode.BarCode == null)
                    {
                        if (CheckWP.ParseBarCode.BarCode == null)
                        {
                            MessageDoubleScan = "Скануйте товар";
                            WP.StateDoubleScan = eCheckWareScaned.WareNotFit;
                        }
                        else
                        {
                            if (WP.CodeWares == CheckWP.CodeWares)
                            {
                                WP = CheckWP;
                                MessageDoubleScan = "Скануйте цінник чи товар";
                                WP.StateDoubleScan = eCheckWareScaned.Success;

                            }
                            else
                            {
                                MessageDoubleScan = " Товар не підходить. \n Скануйте товар";
                                WP.StateDoubleScan = eCheckWareScaned.WareNotFit;
                            }
                        }
                    }
                    // поточний товар
                    else
                    {
                        if (CheckWP.ParseBarCode.BarCode == null)
                        {
                            if (WP.CodeWares == CheckWP.CodeWares)
                            {
                                WP = CheckWP;
                                MessageDoubleScan = "Скануйте цінник чи товар";
                                WP.StateDoubleScan = eCheckWareScaned.Success;

                            }
                            else
                            {
                                MessageDoubleScan = " Цінник не підходить. \n Скануйте цінник";
                                WP.StateDoubleScan = eCheckWareScaned.PriceTagNotFit;
                            }
                        }
                        else
                        {
                            MessageDoubleScan = "Скануйте цінник";
                            WP.StateDoubleScan = eCheckWareScaned.PriceTagNotFit;
                        }
                    }
                }
                else
                {
                    WP = CheckWP;
                    if (WP.ParseBarCode.BarCode == null)
                    {
                        MessageDoubleScan = "Скануйте товар";
                        WP.StateDoubleScan = eCheckWareScaned.PriceTagScaned;
                    }
                    else
                    {
                        MessageDoubleScan = "Скануйте цінник";
                        WP.StateDoubleScan = eCheckWareScaned.WareScaned;
                    }
                }

            }
            
            return MessageDoubleScan;
        }
    */
    
        public void SaveDoubleScan(int Status, WaresPrice pWP, int PackageNumber, int LineNumber)
        {
            var l = new LogPrice(Status, pWP, PackageNumber, LineNumber);
            db.InsLogPrice(l);
            //WP = null;
        }

        public ObservableCollection<DocWaresEx> GetDataPCP(IEnumerable<DocWares> tempInfo, DocVM Doc, int ShelfType)
        {
            int i = 0;
            foreach (var item in tempInfo)
            {
                item.TypeDoc = 13;
                item.OrderDoc = i++;
            }

            db.ReplaceDocWaresSample(tempInfo.Select(el => new DocWaresSample(el)));

            return new ObservableCollection<DocWaresEx>(db.GetDocWares(Doc, 1, eTypeOrder.Name, ShelfType));

        }
    }
}
