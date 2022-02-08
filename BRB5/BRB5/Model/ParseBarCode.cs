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
        public double Price { get; set; } = 0d;
        [JsonIgnore]
        public double PriceOpt { get; set; } = 0d;
        public int Article { get; set; }
        [JsonIgnore]
        public double Quantity { get; set; } = 0d;
    }
}
