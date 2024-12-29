using BRB5;
using BRB5.Model;
using Newtonsoft.Json;
using SharedLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utils;
using BL.Connector;

namespace BL
{
    public partial class BL
    {

        public void OnButtonLogin(string Login, string Password, bool DeviceAndroid)
        {

            db.SetConfig<string>("Login", Login);
            db.SetConfig<string>("Password", Password);
            db.SetConfig<eLoginServer>("LoginServer", Config.LoginServer);
            Config.Login = Login;
            Config.Password = Password;              

            var Wh = c.LoadWarehouse();
            var rrr = db.ReplaceWarehouse(Wh);

            long SizeDel = 0, SizeUse = 0;
            if (Config.Company == eCompany.Sim23 && DeviceAndroid)
            {
                var a = db.GetDoc(Config.GetDocSetting(11));
                (SizeDel, SizeUse) = FileAndDir.DelDir(Config.PathFiles, a.Select(el => el.NumberDoc));
                FileLogger.WriteLogMessage($"{Config.PathFiles} => SizeDel={SizeDel}, SizeUse=>{SizeUse}");
                (SizeDel, SizeUse) = FileAndDir.DelDir(Path.Combine(Config.PathFiles, "arx"), a.Select(el => el.NumberDoc));
                FileLogger.WriteLogMessage($"{Path.Combine(Config.PathFiles, "arx")} => SizeDel={SizeDel}, SizeUse=>{SizeUse}");
            }

            if (Config.DateLastLoadGuid.Date != DateTime.Today.Date)
            {
                _ = Task.Run(async () =>
                {
                    var r = await c.LoadGuidDataAsync(true);
                    if (r.State == 0)
                    {
                        Config.DateLastLoadGuid = DateTime.Now;
                        db.SetConfig<DateTime>("DateLastLoadGuid", Config.DateLastLoadGuid);
                    }
                });

            }

        }

        public void Init()
        {
            Config.IsAutoLogin = db.GetConfig<bool>("IsAutoLogin");
            Config.LoginServer = db.GetConfig<eLoginServer>("LoginServer");
            Config.Company = db.GetConfig<eCompany>("Company");
            Config.IsViewAllWH = db.GetConfig<bool>("IsViewAllWH");
            Config.IsVibration = db.GetConfig<bool>("IsVibration");
            Config.IsSound = db.GetConfig<bool>("IsSound");
            Config.IsTest = db.GetConfig<bool>("IsTest");
            Config.IsFilterSave = db.GetConfig<bool>("IsFilterSave"); 
            //Config.IsFullScreenScan = db.GetConfig<bool>("IsFullScreenScan");
            Config.ApiUrl1 = db.GetConfig<string>("ApiUrl1");
            Config.ApiUrl2 = db.GetConfig<string>("ApiUrl2");
            Config.ApiUrl3 = db.GetConfig<string>("ApiUrl3");
            Config.ApiUrl4 = db.GetConfig<string>("ApiUrl4");
            Config.DateLastLoadGuid = db.GetConfig<DateTime>("DateLastLoadGuid");
            Config.CodeWarehouse = db.GetConfig<int>("CodeWarehouse");
            Config.TypeUsePrinter = db.GetConfig<eTypeUsePrinter>("TypeUsePrinter");
            Config.PhotoQuality = db.GetConfig<ePhotoQuality>("PhotoQuality");
            Config.Compress = db.GetConfig<int>("Compress");
            Config.Compress = Config.Compress == 0 ? 80 : Config.Compress;
            var tempstr = db.GetConfig<string>("CodesWarehouses");
            if (!string.IsNullOrEmpty(tempstr)) Config.CodesWarehouses = JsonConvert.DeserializeObject<List<int>>(tempstr);
            FileLogger.TypeLog = db.GetConfig<eTypeLog>("TypeLog");
            c=Connector.ConnectorBase.GetInstance();


        }



    }
}
