using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace BRB5
{
    public enum eCompany
    {
        NotDefined = 0,
        SparPSU,
        Sim23,
        VPSU,
        Sim23FTP
    }

    public enum eLoginServer
    {
        NotDefined = -1,
        Local = 0,
        Central = 1,
        Bitrix = 2,
        Offline = 3
    }

    public enum eRole
    {
        NotDefined,
        Admin,
        User,
        Auditor,
        Client,
        Manager
    }

    public enum eTypeControlDoc
    {
        /// <summary>
        /// Не дозволяти додавати даний товар
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



    public enum eStateHTTP
    {
        Exeption = -1,
        HTTP_Not_Define_Error = 0,
        HTTP_OK = 200,

        /**
         * HTTP Status-Code 201: Created.
         */
        HTTP_CREATED = 201,

        /**
         * HTTP Status-Code 202: Accepted.
         */
        HTTP_ACCEPTED = 202,

        /**
         * HTTP Status-Code 203: Non-Authoritative Information.
         */
        HTTP_NOT_AUTHORITATIVE = 203,

        /**
         * HTTP Status-Code 204: No Content.
         */
        HTTP_NO_CONTENT = 204,

        /**
         * HTTP Status-Code 205: Reset Content.
         */
        HTTP_RESET = 205,

        /**
         * HTTP Status-Code 206: Partial Content.
         */
        HTTP_PARTIAL = 206,

        /* 3XX: relocation/redirect */

        /**
         * HTTP Status-Code 300: Multiple Choices.
         */
        HTTP_MULT_CHOICE = 300,

        /**
         * HTTP Status-Code 301: Moved Permanently.
         */
        HTTP_MOVED_PERM = 301,

        /**
         * HTTP Status-Code 302: Temporary Redirect.
         */
        HTTP_MOVED_TEMP = 302,

        /**
         * HTTP Status-Code 303: See Other.
         */
        HTTP_SEE_OTHER = 303,

        /**
         * HTTP Status-Code 304: Not Modified.
         */
        HTTP_NOT_MODIFIED = 304,

        /**
         * HTTP Status-Code 305: Use Proxy.
         */
        HTTP_USE_PROXY = 305,

        /* 4XX: client error */

        /**
         * HTTP Status-Code 400: Bad Request.
         */
        HTTP_BAD_REQUEST = 400,

        /**
         * HTTP Status-Code 401: Unauthorized.
         */
        HTTP_UNAUTHORIZED = 401,

        /**
         * HTTP Status-Code 402: Payment Required.
         */
        HTTP_PAYMENT_REQUIRED = 402,

        /**
         * HTTP Status-Code 403: Forbidden.
         */
        HTTP_FORBIDDEN = 403,

        /**
         * HTTP Status-Code 404: Not Found.
         */
        HTTP_NOT_FOUND = 404,

        /**
         * HTTP Status-Code 405: Method Not Allowed.
         */
        HTTP_BAD_METHOD = 405,

        /**
         * HTTP Status-Code 406: Not Acceptable.
         */
        HTTP_NOT_ACCEPTABLE = 406,

        /**
         * HTTP Status-Code 407: Proxy Authentication Required.
         */
        HTTP_PROXY_AUTH = 407,

        /**
         * HTTP Status-Code 408: Request Time-Out.
         */
        HTTP_CLIENT_TIMEOUT = 408,

        /**
         * HTTP Status-Code 409: Conflict.
         */
        HTTP_CONFLICT = 409,

        /**
         * HTTP Status-Code 410: Gone.
         */
        HTTP_GONE = 410,

        /**
         * HTTP Status-Code 411: Length Required.
         */
        HTTP_LENGTH_REQUIRED = 411,

        /**
         * HTTP Status-Code 412: Precondition Failed.
         */
        HTTP_PRECON_FAILED = 412,

        /**
         * HTTP Status-Code 413: Request Entity Too Large.
         */
        HTTP_ENTITY_TOO_LARGE = 413,

        /**
         * HTTP Status-Code 414: Request-URI Too Large.
         */
        HTTP_REQ_TOO_LONG = 414,

        /**
         * HTTP Status-Code 415: Unsupported Media Type.
         */
        HTTP_UNSUPPORTED_TYPE = 415,

        /* 5XX: server error */

        /**
         * HTTP Status-Code 500: Internal Server Error.
         * @deprecated   it is misplaced and shouldn't have existed.
         */

        HTTP_SERVER_ERROR = 500,

        /**
         * HTTP Status-Code 500: Internal Server Error.
         */
        HTTP_INTERNAL_ERROR = 500,

        /**
         * HTTP Status-Code 501: Not Implemented.
         */
        HTTP_NOT_IMPLEMENTED = 501,

        /**
         * HTTP Status-Code 502: Bad Gateway.
         */
        HTTP_BAD_GATEWAY = 502,

        /**
         * HTTP Status-Code 503: Service Unavailable.
         */
        HTTP_UNAVAILABLE = 503,

        /**
         * HTTP Status-Code 504: Gateway Timeout.
         */
        HTTP_GATEWAY_TIMEOUT = 504,

        /**
         * HTTP Status-Code 505: HTTP Version Not Supported.
         */
        HTTP_VERSION = 505
    }

    public enum eTypeScaner
    {
        NotDefine,
        Zebra,
        PM550,
        PM351,
        Camera,
        BitaHC61,
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
