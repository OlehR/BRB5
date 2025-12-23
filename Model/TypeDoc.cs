using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5
{
    public enum eKindDoc { NotDefined=0, Normal=1, Simple=2, PriceCheck=3, RaitingDoc=4, RaitingTempate=5, RaitingTemplateCreate=6, PlanCheck=7, ExpirationDate=8, LotsCheck=9, Lot=10}
    public enum eGroup { NotDefined, Price,Doc,Raiting, FixedAssets }
    public enum eTypeReplenishment {None/*Відсутне*/,Input /*Вручну*/,Auto /* Проставляємо 1*/ }
    public class TypeDoc
    {
        public eGroup Group {  get; set; }
        public int CodeDoc { get; set; }
        public eKindDoc KindDoc { get; set; } = eKindDoc.Normal;
        public string NameDoc { get; set; }
        //public string NameClass { get { return $"{}_{CodeDoc}"}; }
        /// <summary>
        /// Як реагувати коли сканується товар, відсутній в документі (Control, Ask, NoControl)
        /// </summary>
        public eTypeControlDoc TypeControlQuantity { get; set; } = eTypeControlDoc.NoControl;
        
        /// <summary>
        /// Шукати документ по штрихкоду
        /// </summary>
        public bool IsFindBarCode { get; set; }  = false;
        /// <summary>
        /// Показувати причину( бій брак тощо)
        /// </summary>
        public bool IsViewReason { get; set; } = false;
        /// <summary>
        /// Показувати причину в шапці документа.
        /// </summary>
        public bool IsViewReasonHead { get; set; } = false;
        /// <summary>
        /// Показувати планові(фактичні) показники.
        /// </summary>
        public bool IsViewPlan { get; set; } = false;
        /// <summary>
        /// Показувати користувача в списку.
        /// </summary>
        public bool IsShowUser { get; set; } = true;
        /// <summary>
        /// -1 - стандартна, 1 - 7-23 Ревізія? 2 -7-23 Лоти.
        /// </summary>
        public int TypeColor { get; set; } = 1;
        /// <summary>
        /// Скільки днів дивитись документи До і Після сьогодня.
        /// </summary>
        public int DayBefore { get; set; } = 2;
        public int DayAfter { get; set; } = 5;
        /// <summary>
        /// Чи показувати реквізити розхідного документа
        /// </summary>
        public bool IsViewOut { get; set; } = false;
        /// <summary>
        /// Чи можна повторно зберігати документ
        /// </summary>
        public bool IsMultipleSave { get; set; } = true;
        /// <summary>
        /// Передавати в 1с лише проскановані позиції чи і непроскановані з 0 кількістю.
        /// </summary>
        public bool IsSaveOnlyScan { get; set; } = true;
        /// <summary>
        /// Дозволяти в документ добавляти позиції з 0 кількістю (для мініревізій)
        /// </summary>
        public bool IsAddZero { get; set; } = false;
        /// <summary>
        /// Документ з назвою і штрихкодом в табличній частині
        /// </summary>
        public bool IsSimpleDoc { get; set; } = false;
        /// <summary>
        /// Код API для документа (723 -(0-2)
        /// </summary>
        public int CodeApi { get; set; } = 0;
        /// <summary>
        /// Якщо -1 використовуємо CodeApi , інакше цей код для збереження документа
        /// </summary>
        public int CodeApiSave { get; set; } = -1;
        public bool IsCreateNewDoc { get; set; } = false;
        //public bool IsViewOPKO = false;
        public eTypeReplenishment TypeReplenishment { get; set; } = eTypeReplenishment.None;
        public bool IsVisible { get; set; } = true;
        /// <summary>
        /// Чи використовувати лише HTTP  для отримання списку документів.
        /// </summary>
        public bool IsOnlyHttp { get; set; } = false;
        public int LinkedCodeDoc { get; set; } = 0;

    }
}
