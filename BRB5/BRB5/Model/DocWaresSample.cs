
namespace BRB5.Model
{
    public class DocWaresSample : DocWaresId
    {
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
    }
}
