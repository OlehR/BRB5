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
        public System.Guid UserGuid { get; set; }
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
    }
}
