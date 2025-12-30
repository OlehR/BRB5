using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Utils;
using System.Linq;
using System.Threading.Tasks;
using BRB5;
using UtilNetwork;

namespace BL.Connector
{
    public class RaitingTemplateSE : RaitingDocItem
    {
        /// <summary>
        /// Шаблон опитування.
        /// </summary>
        public int IdTempate { get; set; }
    }

    public class ConnectorSE_FTP : ConnectorBase
    {

        //DB Db = new DB();
        FTP Ftp = new FTP();

        /// <summary>
        /// Список Документів доступних по ролі
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public override IEnumerable<TypeDoc> GetTypeDoc(eRole pRole, eLoginServer pLS, eGroup pGroup = eGroup.NotDefined)
        {
            var Res = new List<TypeDoc>()
            {    new TypeDoc() { CodeDoc = 11, KindDoc = eKindDoc.RaitingDoc, NameDoc = "Опитування" }, };
            return Res;
        }

        public override async Task<Result> LoginAsync(string pLogin, string pPassWord, eLoginServer pLoginServer, string pBarCode = null)
        {
            Result Res;
            try
            {
                if (Ftp.DownLoad("Template/User.json", Config.PathFiles))
                {
                    var strU = File.ReadAllText(Path.Combine(Config.PathFiles, "User.json"));
                    var U = JsonConvert.DeserializeObject<List<BRB5.Model.AnswerLogin>>(strU);
                    //db.ReplaceUser(U);             
                }
                if (Ftp.DownLoad("Template/Warehouse.json", Config.PathFiles))
                {
                    var strWh = File.ReadAllText(Path.Combine(Config.PathFiles, "Warehouse.json"));
                    var Wh = JsonConvert.DeserializeObject<List<Warehouse>>(strWh);
                    db.ReplaceWarehouse(Wh);
                }
            }
            catch (Exception e)
            {
                Res = new Result(-1, e.Message);
                FileLogger.WriteLogMessage($"ConnectorSE_FTP.Login=>() Res=>({Res.State},{Res.Data},{Res.TextError})", eTypeLog.Error);
                return Res;
            }

            var res = db.GetUserLogin(new User() { Login = pLogin.Trim(), PassWord = pPassWord.Trim() });
            if (res != null && res.CodeUser > 0)
            {
                return new Result(0);
            }
            return new Result(-1, "Невірний логін чи пароль");
        }

        public override async Task<Result> LoadDocsDataAsync(int pTypeDoc, string pNumberDoc, bool pIsClear)
        {
            var Res = new Result();
            if (pTypeDoc == 11)
            {
                try
                {
                    if (Ftp.DownLoad("Template/Doc.json", Config.PathFiles) && Ftp.DownLoad("Template/RaitingTemplate.json", Config.PathFiles))
                    {
                        var strDoc = File.ReadAllText(Path.Combine(Config.PathFiles, "Doc.json"));
                        var Doc = JsonConvert.DeserializeObject<List<Doc>>(strDoc, proto.JsonSettings);
                        db.ReplaceDoc(Doc);

                        var strRS = File.ReadAllText(Path.Combine(Config.PathFiles, "RaitingTemplate.json"));
                        var R = JsonConvert.DeserializeObject<List<RaitingTemplateSE>>(strRS);

                        foreach(var el in Doc)
                        {
                            var RS = R.Where(e => e.IdTempate == el.IdTemplate);
                            foreach (var e in RS)
                            {
                                e.NumberDoc = el.NumberDoc;
                                e.TypeDoc = el.TypeDoc;
                            }
                            //db.ReplaceRaitingTemplateItem(RS);
                        }                      
                    }
                                         
                }
                catch (Exception e)
                {
                    Res = new Result(-1, e.Message);
                    FileLogger.WriteLogMessage($"ConnectorSE_FTP.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Data},{Res.TextError})", eTypeLog.Error);
                    return Res;
                }
            }
            FileLogger.WriteLogMessage($"ConnectorSE_FTP.LoadDocsData=>(pTypeDoc=>{pTypeDoc}, pNumberDoc=>{pNumberDoc},pIsClear=>{pIsClear}) Res=>({Res.State},{Res.Data},{Res.TextError})", eTypeLog.Error);

            return Res;
        }

        /// <summary>
        /// Вивантаження Рейтингів
        /// </summary>
        /// <param name="pR"></param>
        /// <returns></returns>
        public override async  Task<Result> SendRatingAsync(IEnumerable<BRB5.Model.RaitingDocItem> pR, DocVM pDoc, bool pIsArchive = false)
        {
            var Res = new Result();
            try
            {
                var RD = new List<Raitings>();

                StringBuilder sb = new StringBuilder();
                foreach (var el in pR)
                {
                    sb.Append($"{el.Id};{el.Rating};{el.Note.Replace(';', ',')}{Environment.NewLine}");
                }

                string Dir = $"Data/{DateTime.Now:yyyy.MM.dd}";
                if (!Ftp.FtpDirectoryExists(Dir))
                    Ftp.CreateDir(Dir);
                string NameFile = $"{pDoc?.CodeWarehouse}_{Config.CodeUser}_{pR.First().NumberDoc}.csv";
                Ftp.UpLoad(Encoding.ASCII.GetBytes(sb.ToString()), $"{Dir}/{NameFile}");
            }
            catch (Exception ex)
            {
                Res = new Result(ex);
                FileLogger.WriteLogMessage($"ConnectorSE_FTP.SendRaiting=>() Res=>({Res.State},{Res.Data},{Res.TextError})", eTypeLog.Error);
            }
            return Res;
        }

        /// <summary>
        /// Вивантажеємо на сервер файли Рейтингів
        /// </summary>
        /// <returns></returns>
        public override async Task<Result> SendRatingFilesAsync(string pNumberDoc,int pTry=2, int pMaxSecondSend = 0, int pSecondSkip = 0)
        {
            var Res = new Result();
            var DirArx = Path.Combine(Config.PathFiles, "arx");
            if (!Directory.Exists(DirArx))
            {
                Directory.CreateDirectory(DirArx);
            }

            var R = new RequestSendRaitingFile() { planId = int.Parse(pNumberDoc), action = "file", userId = Config.CodeUser };
            string Dir = $"Data/{DateTime.Now:yyyy.MM.dd}";
            foreach (var f in Directory.GetFiles(Path.Combine(Config.PathFiles, pNumberDoc)))
            {
                try
                {
                    if (Ftp.UpLoad(Path.Combine(Config.PathFiles, f), Dir))
                    {
                        string FileFrom = Path.Combine(Config.PathFiles, f);
                        File.Move(FileFrom, Path.Combine(DirArx, f));
                        FileLogger.WriteLogMessage($"ConnectorPSU.SendRaitingFiles Send=>(File={f})", eTypeLog.Expanded);
                    }
                    else
                    {
                        Res = new Result(-1, "Не передався файл", f);
                        FileLogger.WriteLogMessage($"ConnectorSE_FTP.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Data},{Res.TextError})", eTypeLog.Expanded);
                    }
                }
                catch (Exception e)
                {
                    Res = new Result(e);
                    FileLogger.WriteLogMessage($"ConnectorSE_FTP.SendRaitingFiles=>(File={f}) Res=>({Res.State},{Res.Data},{Res.TextError})", eTypeLog.Error);
                }
            }
            return Res;
        }
    }
}
