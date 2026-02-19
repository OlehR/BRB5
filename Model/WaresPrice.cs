using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using UtilNetwork;

namespace BRB5.Model
{
    public class WaresPrice: INotifyPropertyChanged
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
        public long CodeWares { get; set; }
        public long CodeUser { get; set; }
        public string Name { get; set; }

        //зберігаємо в копійках із за відсутності Decimal
        public decimal Price { get; set; }
        //public String strPrice() { return String.format("%.2f", (double)Price / 100d){get;set;} }
        public decimal PriceOld { get { return ParseBarCode?.Price ?? 0; } }
        public decimal QuantityOpt { get; set; }
        public decimal PriceOpt { get; set; }
        public decimal PriceOptOld { get { return ParseBarCode?.PriceOpt ?? 0; } }

        //public string StartString { get; set; }

        private string _BarCodes;
        public string BarCodes { get { return _BarCodes; } set { _BarCodes = value; OnPropertyChanged(nameof(BarCodes)); } }
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

        public int TypePriceTag { get; set; } = 0;
        public int PLU { get; set; } = 0;
        public string PromotionShortName 
        { get
            {
                if (!string.IsNullOrEmpty(PromotionName))
                {
                    string[] parts = PromotionName.Split(new[] { ' ' }, 2);
                    return parts[1];
                }
                return "";
            } 
        }
        public string PromotionNumber 
        { get
            {
                if (!string.IsNullOrEmpty(PromotionName))
                {
                    string[] parts = PromotionName.Split(new[] { ' ' }, 2);
                    return parts[0].TrimStart('0');
                }
                return "";                
            } 
        }

        public decimal PriceMain { get; set; }
        public DateTime PromotionBegin { get; set; }
        public DateTime PromotionEnd { get; set; }

        public bool Is100g { get; set; }
        public bool IsOnlyCard { get; set; }
        public decimal PriceNormal { get; set; }

        public ParseBarCode ParseBarCode { get; set; }
        public bool IsBarCode {get{ return ParseBarCode==null || ( ParseBarCode.Price==0 && ParseBarCode.BarCode!=null); }}

        //public string StrHttpResult { get { return StateHTTP.ToString(); } }
        public bool IsPriceOk { get { return PriceOld == Price && PriceOptOld == PriceOpt; } }

        //Датa останнього приходу товару на склад торговий зал
        public DateTime LastIncomeDate { get; set; }
        //Кількість останнього приходу товару на склад торговий зал
        public decimal LastIncomeQuantity { get; set; }
        // Кількість АМ
        public int MinQuantity { get; set; }
        //Умови роботи з неліквідним товаром (100% повернення, списання)
        public eTermsForIlliquidWare TermsForIlliquidWare { get; set; }
        //Залишок по складу браку
        public IEnumerable<RestWarehouse> RestWarehouse { get; set; }
        public eCheckWareScaned StateDoubleScan { get; set; }

        public string Country { get; set; }

        public string Parent { get; set; }
        public IEnumerable<СonditionClass> Сondition { get; set; }
        public int QuantityShelf { get; set; }
        public bool IsPriceOptYellow { get; set; } = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class RestWarehouse
    {
        public string NameWarehouse  { get; set; }
        public decimal Quantity { get; set; }
        public DateTime Date { get; set; }        
    }

    public class СonditionClass
    {
        public string Contr { get; set; }
        public string Сondition { get; set; }
    }
    
}
