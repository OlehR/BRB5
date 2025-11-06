using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;
//using Xamarin.Forms;

namespace BRB5.Model
{
    public class DocWaresEx:DocWares
    {               
        //public DocSetting DocSetting;    
        public string NameWares{ get; set; }
        public int Coefficient{ get; set; }
        public int CodeUnit{ get; set; }
        public string NameUnit{ get; set; }
        public string IsControl { get; set;  }
        private string _BarCode;
        public string BarCode { get { return _BarCode; } set { _BarCode = value; OnPropertyChanged(nameof(BarCode)); } }
        public int BaseCodeUnit{ get; set; }

        private decimal _BeforeQuantity;
        public decimal BeforeQuantity { get { return _BeforeQuantity; } set { _BeforeQuantity = value; OnPropertyChanged(nameof(BeforeQuantity)); } }
        public decimal QuantityMin { get; set; }
        public decimal QuantityMax { get; set; }
        //public double QuantityOld{ get; set; }
        public decimal QuantityOrder { get; set; }
        public string QuantityOrderStr { set { QuantityOrder = Convert.ToDecimal(value); } }
        public decimal QuantityReason { get; set; }
        private decimal _QuantityBarCode;
        public decimal QuantityBarCode { get { return ParseBarCode?.Quantity ?? 0m; } set { _QuantityBarCode = value; OnPropertyChanged(nameof(QuantityBarCode)); } }
        /// <summary>
        /// Чи є даний товар в документі
        /// </summary>
        public bool IsRecord { get; set; }

        // Лоти  3- недостача. //2 - надлишок, // 1 - є з причиною // 0 - все ОК.
        // Ревізія. // 0- Зеленим кольором пораховані, 2- оранжевим додані вручну, 0- жовтим непораховані.
        private int _Ord;
        public int Ord{ get { return _Ord; } set { _Ord = value; OnPropertyChanged(nameof(GetBackgroundColor)); } }
        ///public Keyboard Keyboard { get { return CodeUnit == 7 ? Keyboard.Telephone : Keyboard.Numeric; } }

        public ParseBarCode ParseBarCode { get; set; }

        public string StrReason { set { if (!string.IsNullOrEmpty(value)) { var d = value.Split(';'); ProblematicItems = d.Select(el => new ReasonItem(el));
                } } }
        public IEnumerable<ReasonItem> ProblematicItems { get; set; }

        public string NameReason { get; set; }
        public bool IsVisProblematic { get { return QuantityReason > 0; } } 
        public bool Even { get; set; } = false;

        //public boolean IsRecord = false;
        // 3 - червоний, 2- оранжевий, 1 - жовтий, 0 - зелений, -1 - білий, інше грязно жовтий-ранжевий.
        public string GetBackgroundColor
        {
            get
            {
                /*  if(!DocSetting.IsViewPlan)
                      return "fff3cd";*/

                if (Even)
                    return Ord switch
                    {
                        3 => "#FFB0B0",
                        2 => "#FFC050",
                        1 => "#FFFF80",
                        0 => "#80FF80",
                        _ => "#fff3cd",
                    };
                else
                    return Ord switch
                    {
                        3 => "#FFD1D1",
                        2 => "#ffdd8a",
                        1 => "#ffffb7",
                        0 => "#c4ffc4",
                        -1 => "#ffffff",
                        _ => "#fff3cd",
                    };
            }
        }

        /*
        public string GetNameUnit() { return NameUnit + "X"; }
        public string GetInputQuantity() { return InputQuantity == 0.0d ? "" : string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", InputQuantity); }
        public string GetInputQuantityZero() { return string.format(Locale.US, CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", InputQuantity); }
        public string GetQuantityBase() { return string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", (double)Coefficient * InputQuantity); }
        public string GetQuantityOld() { return QuantityOld == 0.0d ? "" : string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", QuantityOld); }
        public string GetBeforeQuantity()
        {
            return string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", BeforeQuantity) +
                    (QuantityOrder > 0 && DocSetting.IsViewPlan ? "/" + string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", QuantityOrder) : "") +
                    (QuantityMax == Double.MAX_VALUE || QuantityMax == 1000000 ? "" : "/" + string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", QuantityMax))
                    ;
        }

     
        //public string GetQuantityOrder() { return string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", QuantityOrder); }
       // public string GetQuantityReason() { return string.format(CodeUnit == config.GetCodeUnitWeight() ? "%.3f" : "%.0f", QuantityReason); }

        public WaresItemModel() { ClearData(); }
        Activity Context;
        BarcodeView barcodeView;
        public WaresItemModel(BarcodeView pBarcodeView)
        {
            ClearData();
            barcodeView = pBarcodeView;
        }
        public WaresItemModel(DocWaresSample pDWS)
        {
            TypeDoc = pDWS.TypeDoc;
            NumberDoc = pDWS.NumberDoc;
            CodeWares = pDWS.CodeWares;
            NameWares = pDWS.Name;
            QuantityMax = pDWS.QuantityMax;
            QuantityMin = pDWS.QuantityMin;
            //QuantityOrder= pDWS.Quantity;
        }
        // public  WaresItemModel(WaresItemModel p){return (WaresItemModel)clone(p);}

        public void Set(WaresItemModel parWIM)
        {
            if (parWIM.NumberDoc != null && parWIM.TypeDoc > 0)
            {
                NumberDoc = parWIM.NumberDoc;
                TypeDoc = parWIM.TypeDoc;
            }
            if (parWIM.OrderDoc > 0)
                OrderDoc = parWIM.OrderDoc;

            CodeWares = parWIM.CodeWares;
            NameWares = parWIM.NameWares;
            Coefficient = parWIM.Coefficient;
            CodeUnit = parWIM.CodeUnit;
            NameUnit = parWIM.NameUnit;
            BarCode = parWIM.BarCode;
            BaseCodeUnit = parWIM.BaseCodeUnit;
            QuantityMin = parWIM.QuantityMin;
            QuantityMax = parWIM.QuantityMax;
            CodeReason = parWIM.CodeReason;
            QuantityBarCode = parWIM.QuantityBarCode;
            QuantityOrder = parWIM.QuantityOrder;

        }
     
        public void ClearData(string pNameWares=null)
        {
            CodeWares = 0;
            NameWares = pNameWares;
            Coefficient = 0;
            CodeUnit = 0;
            NameUnit = "";
            BarCode = "";
            BaseCodeUnit = 0;
            BeforeQuantity = 0;
            QuantityMin = 0;
            InputQuantity = 0;
            QuantityMax = Double.MaxValue;
            CodeReason = 0;
        }*/

    }

    public class ReasonItem
    {
        public decimal Quantity { get; set; }
        public int CodeReason { get; set; }
        public string ReasonName { get; set; }
        public  ReasonItem() { }
        public ReasonItem(string p) 
        {
            var d = p.Split(':');
            if (d.Length > 0)
                CodeReason = d[0].ToInt();           
            if (d.Length > 1)
                Quantity = d[1].ToDecimal();
            if (d.Length > 2)
                ReasonName = d[2];
        }
    }


}
