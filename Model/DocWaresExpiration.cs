
using System;
using System.Drawing;
using System.Linq;
using Utils;

namespace BRB5.Model
{
    public class DocWaresExpiration : DocWaresId
    {
        public DocWaresExpiration() { }
        public DocWaresExpiration(DocWaresId pDW):base(pDW)  { }
        /// <summary>
        ///       
        /// </summary>
        public string DocId { get; set; }
        public decimal Expiration { get; set; }
        public DateTime ExpirationDate { get; set; }

        private decimal _InputQuantity;
        public decimal QuantityInput { get { return _InputQuantity; } set { _InputQuantity = value; OnPropertyChanged("InputQuantity");  } }
        public DateTime ExpirationDateInput { get; set; }

        /*
        /// <summary>
        /// Назва ОЗ(Основного Засобу)
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Штрихкод ОЗ
        /// </summary>
        public string BarCode { get; set; }
        */     

        public string DaysLeft { get; set; }
        public int[] DaysRight { get { return DaysLeft.Split(';')?.Select(e => e.ToInt()).ToArray() ?? new int[0]; } }
        
        int GetColourIndex()
        {
            int i = 0;
            while (i < DaysRight.Length)
            {
               if (QuantityInput <= DaysRight[i]  ) break; 
               i++;
            }
            return i-1;
        }

        public PercentColor GetColor { get { int i = GetColourIndex();  return i < 0 || i>= PercentColor.Length ? null: PercentColor[i]; } }

        /// <summary>
        /// Треба перенести в конектор
        /// </suвmmary>
        PercentColor[] PercentColor = new PercentColor[4] { new PercentColor(10,Color.Green), new PercentColor(25, Color.Yellow) , new PercentColor(50, Color.Orange) , new PercentColor(75, Color.Pink) };

    }
    public class PercentColor
    {
        public PercentColor(int pPercent, Color pColor)
        {
            Percent = pPercent;
            Color = pColor;
        }
        public int Percent { get; set; }
        public Color Color { get; set; }
    }
}
