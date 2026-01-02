using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;

namespace BRB5.Model
{
    public class WaresGL
    {
        public string CodeWares
        {
            get { return Wares == null ? null : string.Join(",", Wares); }
            set { if (value == null) Wares = []; else Wares = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(el => el.ToLong()); }
        }
        public IEnumerable<long> Wares { get; set; }
        //public string Article { get; set; }
        string _NameDocument = null;
        public string NameDocument { get { return _NameDocument ?? $"{NameDCT}-{Login}"; } set { _NameDocument = value; } }
        public long CodeWarehouse { get; set; }
        public DateTime Date { get; set; }
        public string SerialNumber { get; set; }
        public string NameDCT { get; set; }
        public string Login { get; set; }
        [Obsolete("BrandName is deprecated, please use Brand from Warehouse")]
        public eShopTM BrandName
        {
            get
            {
                if (CodeWarehouse < 30)
                    return eShopTM.Vopak;
                else if (CodeWarehouse == 163 || CodeWarehouse == 170)
                    return eShopTM.Lubo;
                else return eShopTM.Spar;
            }
        }
        /// <summary>
        /// Тільки наявні на залишку товари
        /// </summary>
        public bool IsOnlyRest { get; set; }
    }
}
