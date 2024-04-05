using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Text;
using Utils;

namespace BL
{
    public partial class BL
    {
        WaresPrice WP;

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
    }
}
