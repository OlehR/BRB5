
using System;

namespace BRB5.Model
{
    public class DocWaresSample : DocWaresId
    {
        public DocWaresSample() { }
        public DocWaresSample(DocWaresId pDW):base(pDW) 
        { }
        /// <summary>
        /// не використовується
        /// </summary>
        public double QuantityMin { get; set; }
        /// <summary>
        /// Максимальна кількість товару в документі, контролюється якщо IsControl=1
        /// </summary>
        public double QuantityMax { get; set; }
        /// <summary>
        /// Назва ОЗ(Основного Засобу)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Штрихкод ОЗ
        /// </summary>
        public string BarCode { get; set; }

        public DateTime ExpirationDate { get; set; }

        public decimal Expiration { get; set; }

        /// <summary>
        /// Додаткова інформація      
        /// </summary>
        public string ExtInfo { get; set; }
    }
}
