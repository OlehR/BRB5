using System;
using System.Collections.Generic;
using System.Text;

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
        public string BarCode{ get; set; }
        public int BaseCodeUnit{ get; set; }

        public decimal BeforeQuantity { get; set; }
        public decimal QuantityMin { get; set; }
        public decimal QuantityMax { get; set; }
        //public double QuantityOld{ get; set; }
        public decimal QuantityOrder { get; set; }
        public decimal InputQuantity { get; set; }
        public decimal QuantityReason { get; set; }
        public decimal QuantityBarCode { get { return ParseBarCode?.Quantity ?? 0m; } }
        /// <summary>
        /// Чи є даний товар в документі
        /// </summary>
        public bool IsRecord { get; set; }

        // Лоти  3- недостача. //2 - надлишок, // 1 - є з причиною // 0 - все ОК.
        // Ревізія. // 0- Зеленим кольором пораховані, 2- оранжевим додані вручну, 0- жовтим непораховані.
        public int Ord{ get; set; }

        public ParseBarCode ParseBarCode { get; set; }

        //public boolean IsRecord = false;
        // 3 - червоний, 2- оранжевий, 1 - жовтий, 0 - зелений, інше грязно жовтий-ранжевий.
        public string GetBackgroundColor
        {
            get
            {
                /*  if(!DocSetting.IsViewPlan)
                      return "fff3cd";*/
                switch (Ord)
                {
                    case 3:
                        return "#FFB0B0";
                    case 2:
                        return "#FFC050";
                    case 1:
                        return "#FFFF80";
                    case 0:
                        return "#80FF80";
                    default:
                        return "#fff3cd";
                }
            }
        }

        public string GetLightBackgroundColor
        {
            get
            {
                /*  if(!DocSetting.IsViewPlan)
                      return "fff3cd";*/
                switch (Ord)
                {
                    case 3:
                        return "#FFD1D1";
                    case 2:
                        return "#ffdd8a";
                    case 1:
                        return "#ffffb7";
                    case 0:
                        return "#c4ffc4";
                    default:
                        return "#fff3cd";
                }
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




}
