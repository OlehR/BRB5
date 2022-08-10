using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class ParseBarCode
    {
        [JsonIgnore]
        public bool IsOnlyBarCode { get; set; } = false;
        public string BarCode { get; set; } = null;
        public int CodeWares { get; set; } = 0;
        [JsonIgnore]
        public decimal Price { get; set; } = 0m;
        [JsonIgnore]
        public decimal PriceOpt { get; set; } = 0m;
        public int Article { get; set; }
        [JsonIgnore]
        public decimal Quantity { get; set; } = 0m;
    }
}
