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
        WaresPrice WP;
        WaresPrice[] WPH = new WaresPrice[2];

        public (WaresPrice, string) FoundWares(string pBarCode, int PackageNumber, int LineNumber, bool pIsHandInput, bool pIsDoubleScan, bool IsOnline = true)
        {
            WaresPrice CheckWP;
            string MessageDoubleScan = string.Empty;
            LogPrice l;

            if (IsOnline)
            {
                CheckWP = c.GetPrice(c.ParsedBarCode(pBarCode, pIsHandInput));
            }
            else
            {
                var data = GetWaresFromBarcode(0, null, pBarCode, pIsHandInput);
                CheckWP = new WaresPrice(data);
            }

            if (pIsDoubleScan)
            {
                MessageDoubleScan = SearchDoubleScan(CheckWP);
                if (WP.StateDoubleScan == eCheckWareScaned.Success)
                {
                    l = new LogPrice(100, WP, PackageNumber, LineNumber);
                    db.InsLogPrice(l);
                }
            }
            else
            {
                WP = CheckWP;
                l = new LogPrice(WP, IsOnline, PackageNumber, LineNumber);
                db.InsLogPrice(l);
            }

            return (WP, MessageDoubleScan);
        }

        private string SearchDoubleScanNew(WaresPrice CheckWP, int PackageNumber, int LineNumber)
        {
            string Res = "";
            if (CheckWP?.CodeWares != 0){
                for (int i = WPH.Count() - 1; i >= 0; i--)
                {
                    if (WPH[i] == null) continue;
                    eCheck R = CompareDoubleScan(CheckWP, WPH[i]);
                    if (R == eCheck.Same) break;
                    if (R == eCheck.Ok)
                    {
                        SaveDoubleScan(100, CheckWP, PackageNumber, LineNumber);
                        for (int j = 0; j < WPH.Count(); j--) WPH[j] = null;
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
                        WPH[1] = CheckWP;
                    }
                }
                if (WPH[0] == null) Res = "Скануйте цінник чи товар";
                if (WPH[0] == null) WPH[0] = CheckWP;
                
                else Res = (CheckWP.IsBarCode ? "Скануйте цінник" : "Скануйте товар");
            }
            return Res;
        }
        enum eCheck { Ok,Same,Bad }
        private eCheck CompareDoubleScan(WaresPrice pWP, WaresPrice pS)
        {
            if (pWP.CodeWares == pS.CodeWares)
                if (pWP.IsBarCode == !pS.IsBarCode) return eCheck.Ok; else return eCheck.Same;
            return eCheck.Bad;
        }

            private string SearchDoubleScan(WaresPrice CheckWP)
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
    
    
        public void SaveDoubleScan(int Status, WaresPrice pWP, int PackageNumber, int LineNumber)
        {
            var l = new LogPrice(Status, pWP, PackageNumber, LineNumber);
            db.InsLogPrice(l);
            WP = null;
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
