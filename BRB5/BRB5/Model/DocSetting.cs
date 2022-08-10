using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class DocSetting
    {
        public int TypeDoc;
        public String NameDoc;

        public eTypeControlDoc TypeControlQuantity = eTypeControlDoc.NoControl;
        // Доступ до документа за додатковим URL
        public bool IsUrlAdd = false;
        //Шукати документ по штрихкоду
        public bool IsAddBarCode = false;
        //Показувати причину( бій брак тощо)
        public bool IsViewReason = false;
        // Показувати планові(фактичні) показники.
        public bool IsViewPlan = false;
        // Показувати користувача в списку.
        public bool IsShowUser = true;
        // -1 - стандартна, 1 - 7-23 Ревізія? 2 -7-23 Лоти.
        public int TypeColor = -1;
        // Скільки днів дивитись документи До і Після сьогодня.
        public int DayBefore = 2;
        public int DayAfter = 5;
        //Чи показувати реквізити розхідного документа
        public bool IsViewOut = false;
        // Чи можна повторно зберігати документ
        public bool IsMultipleSave = true;
        //Передавати в 1с лише проскановані позиції чи і непроскановані з 0 кількістю.
        public bool IsSaveOnlyScan = true;
        //Дозволяти в документ добавляти позиції з 0 кількістю (для мініревізій)
        public bool IsAddZero = false;
        // Документ з назвою і штрихкодом в табличній частині
        public bool IsSimpleDoc = false;
        // Код API для документа (723 -(0-2)
        public int CodeApi = 0;

        public DocSetting(int pTypeDoc, String pNameDoc)
        {
            TypeDoc = pTypeDoc;
            NameDoc = pNameDoc;
        }
        public DocSetting(int pTypeDoc, String pNameDoc, eTypeControlDoc pTypeControlQuantity, bool pIsUrlAdd, bool pIsAddBarCode, bool pIsViewReason, bool pIsViewPlan, bool pIsShowUser, int pTypeColor, int pDayBefore, int pDayAfter, bool pIsViewOut, bool pIsmultipleSave, bool pIsSaveOnlyScan, bool pIsAddZero, bool pIsSimpleDoc, int pCodeApi) : this(pTypeDoc, pNameDoc)
        {
            TypeControlQuantity = pTypeControlQuantity;
            IsUrlAdd = pIsUrlAdd;
            IsAddBarCode = pIsAddBarCode;
            IsViewReason = pIsViewReason;
            IsViewPlan = pIsViewPlan;
            IsShowUser = pIsShowUser;
            TypeColor = pTypeColor;
            DayBefore = pDayBefore;
            DayAfter = pDayAfter;
            IsViewOut = pIsViewOut;
            IsMultipleSave = pIsmultipleSave;
            IsSaveOnlyScan = pIsSaveOnlyScan;
            IsAddZero = pIsAddZero;
            IsSimpleDoc = pIsSimpleDoc;
            CodeApi = pCodeApi;
        }
    }
}
