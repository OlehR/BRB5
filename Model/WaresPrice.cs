using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class WaresPrice:Result
    {
        public WaresPrice() { }
        public WaresPrice(DocWaresEx pW) 
        { 
            CodeWares=pW.CodeWares;
            Name = pW.NameWares;
            BarCodes = pW.BarCode;
            Unit = pW.NameUnit;
            ParseBarCode=pW.ParseBarCode;


        }
        public WaresPrice(HttpResult pHttp, string pInfo=null) : base(pHttp, pInfo) { }
        public WaresPrice(int pState = 0, string pTextError = "Ok", string pInfo = "") : base(pState, pTextError, pInfo) { }
        public int CodeWares { get; set; }
        public string Name { get; set; }

        //зберігаємо в копійках із за відсутності Decimal
        public decimal Price { get; set; }
        //public String strPrice() { return String.format("%.2f", (double)Price / 100d){get;set;} }
        public decimal PriceOld { get { return ParseBarCode?.Price ?? 0; } }
        public decimal QuantityOpt { get; set; }
        public decimal PriceOpt { get; set; }
        public decimal PriceOptOld { get { return ParseBarCode?.PriceOpt ?? 0; } }

        //public string StartString { get; set; }

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

        public ParseBarCode ParseBarCode { get; set; }
        public string StrHttpResult { get { return StateHTTP.ToString(); } }
        public bool IsPriceOk { get { return PriceOld == Price && PriceOptOld == PriceOpt; } }

        //Датa останнього приходу товару на склад торговий зал
        public DateTime LastArrivalDate { get; set; }
        //Кількість останнього приходу товару на склад торговий зал
        public decimal LastArrivalQuantity { get; set; }
        //Статус товару (відкритий 1 або закритий 0 в асортиментній матриці магазину)
        public int StateWare { get; set; }
        //Умови роботи з неліквідним товаром (100% повернення, списання)
        public eTermsForIlliquidWare TermsForIlliquidWare { get; set; }
        //Залишок по складу браку
        public List<DefectBalance> BalanceDefects { get; set; }
    }
    public struct DefectBalance
    {
        public Warehouse WH { get; set; }
        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }
    }
}
