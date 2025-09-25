using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BRB5
{
    public enum eCompany
    {
        NotDefined = 0,
        PSU,
        Sim23,
        Universal
        //VPSU,
        //Sim23FTP
    }

    public enum eLoginServer
    {
        NotDefined = -1,       
        Central = 0,
        Local = 1,
        Bitrix = 2,
        Offline = 3
    }

    public enum eRole
    {
        NotDefined = 0,
        Admin = 1,
        User = 2,
        Auditor = 3,
        Client = 4,
        Manager = 5
    }

    public enum eTypeControlDoc
    {
        /// <summary>
        /// Не дозволяти додавати відсутній товар
        /// </summary>
        Control,
        /// <summary>
        /// Запитувати чи добавляти
        /// </summary>
        Ask,
        /// <summary>
        /// Додавати без питань.
        /// </summary>
        NoControl
    }

    public enum eTypeOrder
    {
        NoOrder,
        Scan,
        Name
    }

    public enum eTypeUsePrinter
    {
        [Description("Без Принтера")]
        NotDefined = 0,
        [Description("Тільки при вході")]
        OnlyStart = 1,
        [Description("Авто підключення")]
        AutoConnect = 2,
        [Description("Стаціонарний з обрізжчиком")]
        StationaryWithCut = 3,
        [Description("Стаціонарний з обрізжчиком (автовибір)")]
        StationaryWithCutAuto = 4
    }

    public static class EnumMethods
    {

        /*public static string GetString(this eTypeUsePrinter pThis)
        {
            switch (pThis)
            {
                case eTypeUsePrinter.NotDefined: return "Без Принтера";
                case eTypeUsePrinter.OnlyStart: return "Тільки при вході";
                case eTypeUsePrinter.AutoConnect: return "Авто підключення";
                case eTypeUsePrinter.StationaryWithCut: return "Стаціонарний з обрізжчиком";
                case eTypeUsePrinter.StationaryWithCutAuto: return "Стаціонарний з обрізжчиком (автовибір)";
                default: return "Невідоме значення";
            }
        }*/
        public static string GetDescription(Enum value)
        {
            var enumMember = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var descriptionAttribute =
                enumMember == null
                    ? default(DescriptionAttribute)
                    : enumMember.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
            return
                descriptionAttribute == null
                    ? value.ToString()
                    : descriptionAttribute.Description;
        }
        public static int GetValue(this ePhotoQuality pPQ)
        {
            return pPQ switch
            {
                ePhotoQuality.Original => 4032,
                ePhotoQuality.High => 2016,
                ePhotoQuality.Medium => 1008,
                _ => 0,
            };
        }
    }

    public enum eTypeScaner
    {
        NotDefine,
        Zebra,
        PM550,
        PM351,
        PM84,
        Camera,
        BitaHC61,
        ChainwayC61,
        MetapaceM_K4,
        NLS_MT67,
        KeyBoard
    }
    public enum eTermsForIlliquidWare
    {
        [Description("100% повернення")]
        FullRefund = 1,
        [Description("списання")]
        WriteOff = 2
    }
    public enum eTypePriceInfo
    {
        //[Description("скорочена")]
        Short = 0,
        //[Description("повна")]
        Normal = 1,
        //[Description("розширена")]
        Full = 2
    }

    public enum eTypeChoice
    {
        NotDefine,
        All,
        OnlyHead,
        NoAnswer
    }

    public enum ePhotoQuality
    {
        [Description("Оригінальна")]
        Original,
        [Description("Висока")]
        High,
        [Description("Середня")]
        Medium
    }

    public enum eCheckWareScaned
    {
        [Description("Nothing")]
        Nothing = 0,
        [Description("Скануйте товар")]
        PriceTagScaned = 1,
        [Description("Скануйте цінник")]
        WareScaned = 2,
        [Description("WareNotFit")]
        WareNotFit = 3,
        [Description("PriceTagNotFit")]
        PriceTagNotFit = 4,
        [Description("Скануйте цінник чи товар")]
        Success = 5,
        [Description("Невірне сканування")]
        Bad = 6,
        [Description("Невірна ціна")]
        BadPrice = 7,
    }
}
