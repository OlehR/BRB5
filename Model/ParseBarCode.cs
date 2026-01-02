using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class ParseBarCode
    {
        [JsonIgnore]
        public bool IsHandInput { get; set; } = false;
        [JsonIgnore]
        public string StartString { get; set; } = null;
        public string BarCode { get; set; } = null;
        public long CodeWares { get; set; } = 0;
        [JsonIgnore]
        public decimal Price { get; set; } = 0m;
        [JsonIgnore]
        public decimal PriceOpt { get; set; } = 0m;
        public int Article { get; set; }
        /// <summary>
        /// SKU нових версій 1С
        /// </summary>
        public long SKU { get; set; } = 0;
        [JsonIgnore]
        public decimal Quantity { get; set; } = 0m;
        public eTypePriceInfo TypePriceInfo { get; set; }
        public decimal PercentDiscount { get; set; } = 0m;
        public eTypeCode TypeCode { get; set; } = eTypeCode.NotDefine;
        public int CodeOperator { get; set; } = 0;
        [JsonIgnore]
        public int CodeUnit { get; set; }
        [JsonIgnore]
        public decimal Coefficient { get; set; }
    }
}
