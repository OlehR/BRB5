using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5
{
    public enum eKindDoc {Normal,Simple, PriceCheck, Raiting }
    public class TypeDoc
    {
        public int CodeDoc { get; set; }
        public eKindDoc KindDoc { get; set; } = eKindDoc.Normal;
        public string NameDoc { get; set; }
        //public string NameClass { get { return $"{}_{CodeDoc}"}; }
        /// <summary>
        /// Як реагувати коли сканується товар, відсутній в документі (Control, Ask, NoControl)
        /// </summary>
        public eTypeControlDoc TypeControlQuantity = eTypeControlDoc.NoControl;
        
        /// <summary>
        /// Шукати документ по штрихкоду
        /// </summary>
        public bool IsFindBarCode = false;
        /// <summary>
        /// Показувати причину( бій брак тощо)
        /// </summary>
        public bool IsViewReason = false;
        /// <summary>
        /// Показувати планові(фактичні) показники.
        /// </summary>
        public bool IsViewPlan = false;
        /// <summary>
        /// Показувати користувача в списку.
        /// </summary>
        public bool IsShowUser = true;
        /// <summary>
        /// -1 - стандартна, 1 - 7-23 Ревізія? 2 -7-23 Лоти.
        /// </summary>
        public int TypeColor = -1;
        /// <summary>
        /// Скільки днів дивитись документи До і Після сьогодня.
        /// </summary>
        public int DayBefore = 2;
        public int DayAfter = 5;
        /// <summary>
        /// Чи показувати реквізити розхідного документа
        /// </summary>
        public bool IsViewOut = false;
        /// <summary>
        /// Чи можна повторно зберігати документ
        /// </summary>
        public bool IsMultipleSave = true;
        /// <summary>
        /// Передавати в 1с лише проскановані позиції чи і непроскановані з 0 кількістю.
        /// </summary>
        public bool IsSaveOnlyScan = true;
        /// <summary>
        /// Дозволяти в документ добавляти позиції з 0 кількістю (для мініревізій)
        /// </summary>
        public bool IsAddZero = false;
        /// <summary>
        /// Документ з назвою і штрихкодом в табличній частині
        /// </summary>
        public bool IsSimpleDoc = false;
        /// <summary>
        /// Код API для документа (723 -(0-2)
        /// </summary>
        public int CodeApi = 0;

    }
}
