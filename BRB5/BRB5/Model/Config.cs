using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class Config
    {
        static DB db = DB.GetDB();
        public static int CodeWarehouse = 0;
        static eCompany _Company = eCompany.NotDefined;
        public static eCompany Company
        {
            get {
                if(_Company == eCompany.NotDefined)
                    _Company = db.GetConfig<eCompany>("Company");
                return  _Company; }
            set { _Company = value; }
        }
        public static string SN;
        public static int Ver;
        public static bool IsAutoLogin = false;
        public static bool IsLoginCO = false;
        public static string Login { get; set; } = "LOX";
        public static string Password { get; set; } = "321";
        public static eRole Role = eRole.NotDefined;
        public static int GetCodeUnitWeight() { return Company == eCompany.Sim23 ? 166 : 7; }

        public static int GetCodeUnitPiece() { return Company == eCompany.Sim23 ? 796 : 19; }
    }
}
