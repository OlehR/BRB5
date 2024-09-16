using BL.Connector;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using BRB5.Model;
using System.Collections.ObjectModel;
using System.Linq;

namespace BL
{
    public partial class BL
    {
        public decimal CountBeforeQuantity(int pCodeWares, ObservableCollection<DocWaresEx> ListWares)
        {
            decimal res = 0;
            if (ListWares.Count() > 0)
            {
                foreach (var ware in ListWares)
                {
                    ware.Ord = -1;
                    if (ware.CodeWares == pCodeWares)
                    {
                        res += ware.InputQuantity;
                        ware.Ord = 0;
                    }
                }
            }
            return res;
        }
        public void Reset(DocWaresEx ScanData, ObservableCollection<DocWaresEx> ListWares)
        {
            if (ScanData != null && ListWares.Count() > 0)
            {
                foreach (var ware in ListWares)
                {
                    if (ware.CodeWares == ScanData.CodeWares && ware.InputQuantity != 0)
                    {
                        ware.QuantityOld = ware.InputQuantity;
                        ware.InputQuantity = 0;
                        ware.Quantity = 0;
                        db.ReplaceDocWares(ware);
                    }
                }

            }
        }
        public ObservableCollection<DocVM> SetColorType(IEnumerable<DocVM> pDocs)
        {
            int index = 0;
            foreach (var pDoc in pDocs)
            {
                pDoc.Even = (index % 2 == 0);
                index++;
            }

            return new ObservableCollection<DocVM>(pDocs);
        }
    }
}
