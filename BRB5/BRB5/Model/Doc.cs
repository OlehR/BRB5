using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Text;
using Xamarin.Forms;

namespace BRB5
{
    public class DocId
    {
        /// <summary>
        /// Тип документа (1-ревізія 2-приходи тощо,11-опитування)
        /// </summary>
        public int TypeDoc { get; set; }
        /// <summary>
        /// (Номер документа в 1С)
        /// </summary>
        public string NumberDoc { get; set; }
    }

    public class Doc:DocId
    {
        /// <summary>
        /// Стан 0 - готується, 1 - збережено ...
        /// </summary>
        public int State { get; set; }

         
        /// <summary>
        /// Дата документа
        /// </summary>
        public DateTime DateDoc { get; set; } 
        public string DateString { get { return DateDoc.ToString("dd.MM.yy"); } }

        /// <summary>
        /// 1- Якщо треба контролювати асортимент та кількість, для замовлень та можливо інших документів.
        /// </summary>
        public int IsControl { get; set; } // 

        /// <summary>
        /// Шаблон
        /// </summary>
        public int IdTempate { get; set; }
        /// <summary>
        /// Код складу
        /// </summary>
        public int CodeWarehouse { get; set; }
        /// <summary>
        /// Додаткова інформація, яка може вплинути на обробку документа напиклад ЗКПО постачальника
        /// Для Анкет код складу магазина.
        /// </summary>
        public string ExtInfo { get; set; } // 
        /// <summary>
        /// користувач який створив документ рядок
        /// </summary>
        public string NameUser { get; set; } // 
        /// <summary>
        /// штрихкод документа, якщо є для швидкого пошуку
        /// </summary>
        public string BarCode { get; set; } //
        /// <summary>
        ///  (опис для ревізій коментар, якщо використовують для приходів назва контрагента)
        /// </summary>
        public string Description { get; set; } //
        /// <summary>
        /// Для замовлень номер прихідної, якщо створено.
        /// </summary>
        public string NumberDoc1C { get; set; } // 
        /// <summary>
        /// Дата розхідної накладної для приходу
        /// </summary>
        public string DateOutInvoice { get; set; } // 
        /// <summary>
        /// Номер розхідної накладної для приходу
        /// </summary>
        public string NumberOutInvoice { get; set; } // 
        /// <summary>
        /// Колір відображення документа
        /// </summary>
        public int Color { get; set; }

        // 9 - червоний, 2- оранжевий, 1 - жовтий, 0 - зелений, інше грязно жовтий-ранжевий.
        public string GetColor
        {
            get
            {
                switch (Color)
                {
                    case 9:
                        return "#FFB0B0"; //Червоний
                    case 2:
                        return "#FFC050"; //Оранжевий
                    case 0:
                        return "#FFFF80"; //Жовтий
                    case 1:
                        return "#80FF80"; //Зелений
                    default:
                        return "#fff3cd";
                }
            }
        }
        //public Color GetColor { get { return Color == 0?new Color(0xdcdcdc) : new Color(Color); } }

        //public int isClose; //0- не закривати, 1 - закривати.
        public Doc() { }
        public Doc(int pTypeDoc, string pNumberDoc)
        {
            TypeDoc = pTypeDoc;
            NumberDoc = pNumberDoc;
        }


       /* public Date GetDateOutInvoice()
        {
            SimpleDateFormat formatterDate = new SimpleDateFormat("yyyy-MM-dd");
            Date DateOut = Calendar.getInstance().getTime();
            try
            {
                DateOut = formatterDate.parse(DateOutInvoice);
            }
            catch (Exception e)
            {
                try { DateOut = formatterDate.parse(formatterDate.format(DateOut)); } catch (Exception ee) { }
            }
            return DateOut;
        }*/
    }
}
