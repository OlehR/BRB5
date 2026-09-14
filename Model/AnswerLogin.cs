using Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace BRB5.Model
{
    public class User
    {
        public eLoginServer LoginServer { get; set; }
        public int CodeUser { get; set; }
        public string BarCode { get; set; }
        public string Login { get; set; }
        public string PassWord { get; set; }
    }

    public class RequestLogin : User
    {
        public string IP { get; set; }
        public int CodeWarehouse { get; set; } = Config.CodeWarehouse;
        public string SerialNumber { get; set; } = Config.SN;
        public string Version { get; set; } = Config.Version;
        public bool IsTest { get; set; } = Config.IsTest;
    }

    
    /// <summary>
    /// Через Swagger
    /// </summary>
    public class UserBRB : User { public UserBRB() { } }
    public class AnswerLogin:User
    {
        public AnswerLogin() { }
        public AnswerLogin(User pU=null)
        {
           if(pU!=null)
            {
                LoginServer= pU.LoginServer;
                CodeUser = pU.CodeUser;
                BarCode = pU.BarCode;
                Login = pU.Login;
                PassWord = pU.PassWord;
            }
        }
        public string NameUser { get; set; }
        public eRole Role { get; set; }
        public IEnumerable<TypeDoc> TypeDoc { get; set; }
        public IEnumerable<CustomerBarCode> CustomerBarCode { get; set; }
        public eCompany LocalConnect { get; set; }
        public string PathAPK { get; set; }
        public System.Guid UserGuid { get; set; }
        public int CodeUnitWeight { get; set; }
        public int CodeUnitPiece { get; set; }

        public bool IsVisOrderF3 { get; set; }
        public bool IsUseArticle { get; set; }
    }
}
