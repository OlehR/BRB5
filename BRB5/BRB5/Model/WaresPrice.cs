using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class WaresPrice:Result
    {
        public WaresPrice() { }
        public WaresPrice(HttpResult pHttp, string pInfo=null) : base(pHttp, pInfo) { }
        public WaresPrice(int pState = 0, string pTextError = "Ok", string pInfo = "") : base(pState, pTextError, pInfo) { }
        public int Code { get; set; }
        public string Name { get; set; }

        //зберігаємо в копійках із за відсутності Decimal
        public decimal Price { get; set; }
        //public String strPrice() { return String.format("%.2f", (double)Price / 100d){get;set;} }
        public decimal PriceOld { get; set; }
        public decimal QuantityOpt { get; set; }
        public int PriceOpt { get; set; }
        public int PriceOptOld { get; set; }

        public string BarCodes { get; set; }
        //public int ColorPriceOpt() { return Color.parseColor(OldPriceOpt != PriceOpt ? "#ee4343" : "#3bb46e"){get;set;} }

        public string Unit { get; set; }
        public string Article { get; set; }
        public decimal Rest { get; set; }
        public decimal Sum { get; set; }
        public int ActionType { get; set; } //0 - без акції, 1 - жовтий цінник

        public decimal PriceBase { get; set; }
        public decimal MinPercent { get; set; }
        public decimal PriceMin { get; set; }
        public decimal PriceIndicative { get; set; }

        public string PromotionName { get; set; }
        public decimal PriceMain { get; set; }
        public DateTime PromotionBegin { get; set; }
        public DateTime PromotionEnd { get; set; }

        public bool Is100g { get; set; }
    }
}
