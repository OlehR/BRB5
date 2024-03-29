﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;
using BL.Connector;
using BRB5.Model;
using BRB5.View;
using Xamarin.Essentials;
using Utils;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BL;

//[assembly: Xamarin.Forms.Dependency(typeof(AndroidStorageManager))]

namespace BRB5
{
    public partial class MainPage
    {
        public ObservableCollection<TypeDoc> OCTypeDoc { get; set; }
        Connector c;
        DB db = DB.GetDB();        
        public string Login { get; set; }
        public string Password { get; set; }
        public IEnumerable<LoginServer> LS { get; set; }

        public int SelectedLS { get { return LS == null || LS.Count() == 1 ? 0 : LS.ToList().FindIndex(x => x.Code == Config.LoginServer); } set { Config.LoginServer = LS.ToList()[value].Code; } }
        public bool IsVisLS { get; set; } = true;
        public string Ver { get { return"BRB5 (" + AppInfo.VersionString + ")"; } }
        public string Company { get { return Enum.GetName(typeof(eCompany), Config.Company); } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public MainPage()
        {
            OCTypeDoc = new ObservableCollection<TypeDoc>();           
            InitializeComponent();
            Init();

            if (Config.Company == eCompany.NotDefined) _= Navigation.PushAsync(new Settings());
                
            BindingContext = this;
        }        

        private void OnButtonLogin(object sender, System.EventArgs e)
        {
            _ = Task.Run(async () =>
            {
                var r = await c.LoginAsync(Login, Password, Config.LoginServer);
                if (r.State == 0)
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OCTypeDoc?.Clear();
                        Config.TypeDoc = c.GetTypeDoc(Config.Role, Config.LoginServer);
                        //OCTypeDoc = new ObservableCollection<TypeDoc>(Config.TypeDoc);
                        foreach (var i in Config.TypeDoc) OCTypeDoc.Add(i);
                        SLLogin.IsVisible = false;
                        ListDocs.IsVisible = true;
                    });

                    db.SetConfig<string>("Login", Login);
                    //db.SetConfig<bool>("IsAutoLogin", true);
                    db.SetConfig<string>("Password", Password);
                    db.SetConfig<eLoginServer>("LoginServer", Config.LoginServer);
                    Config.Login = Login;
                    Config.Password = Password;
                    //eLoginServer LoginServer;                   

                    var Wh = c.LoadWarehouse();
                    var rrr=db.ReplaceWarehouse(Wh);

                    long SizeDel = 0, SizeUse = 0;
                    if (Config.Company == eCompany.Sim23 && Device.RuntimePlatform == Device.Android)
                    {
                        var a = db.GetDoc(Config.GetDocSetting(11));
                        (SizeDel, SizeUse) = FileAndDir.DelDir(Config.PathFiles, a.Select(el => el.NumberDoc));
                        FileLogger.WriteLogMessage($"{Config.PathFiles} => SizeDel={SizeDel}, SizeUse=>{SizeUse}");
                        (SizeDel, SizeUse) = FileAndDir.DelDir(Path.Combine(Config.PathFiles, "arx"), a.Select(el => el.NumberDoc));
                        FileLogger.WriteLogMessage($"{Path.Combine(Config.PathFiles, "arx")} => SizeDel={SizeDel}, SizeUse=>{SizeUse}");
                    }

                    if (Config.DateLastLoadGuid.Date != DateTime.Today.Date)
                    {
                        _=Task.Run(async () =>
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
                else
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        _ = DisplayAlert("Проблеми з авторизацією", r.TextError + r.Info, "OK");
                    });
            });
            }

        private async void OnButtonClicked(object sender, System.EventArgs e)
        {
            Button button = (Button)sender;
            Cell cc = button.Parent as Cell;
            var vTypeDoc = cc.BindingContext as TypeDoc;

            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (cameraStatus != PermissionStatus.Granted)
            {
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
            }

            if (cameraStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Error", "Need camera permission", "OK", FlowDirection.MatchParent);
                return;
            }

            switch (vTypeDoc.KindDoc)
            {
                case eKindDoc.PriceCheck:
                    await Navigation.PushAsync(new PriceCheck()); //new CustomScanPage()); // 
                    break;
                case eKindDoc.Normal:
                case eKindDoc.Simple:
                    await Navigation.PushAsync(new Docs(vTypeDoc));
                    break;
                case eKindDoc.RaitingDoc:
                    await Navigation.PushAsync(new RaitingDoc(vTypeDoc));
                    break;
                case eKindDoc.RaitingTempate:
                    await Navigation.PushAsync(new RaitingTemplatesEdit());
                    break;
                case eKindDoc.RaitingTemplateCreate:
                    await Navigation.PushAsync(new RaitingDocsEdit(vTypeDoc));
                    break;
                case eKindDoc.PlanCheck:
                    await Navigation.PushAsync(new PlanCheckPrice());
                    break;
                default:
                    break;
            }
        }


        void Init()
        {
            _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
            Config.IsAutoLogin = db.GetConfig<bool>("IsAutoLogin");
            Config.LoginServer = db.GetConfig<eLoginServer>("LoginServer");
            Config.Company = db.GetConfig<eCompany>("Company");
            Login = db.GetConfig<string>("Login");
            Config.IsViewAllWH = db.GetConfig<bool>("IsViewAllWH");
            Config.IsVibration = db.GetConfig<bool>("IsVibration");
            Config.IsSound = db.GetConfig<bool>("IsSound");
            Config.IsTest = db.GetConfig<bool>("IsTest");
            Config.IsFilterSave = db.GetConfig<bool>("IsFilterSave");
            Config.ApiUrl1 = db.GetConfig<string>("ApiUrl1");
            Config.ApiUrl2 = db.GetConfig<string>("ApiUrl2");
            Config.ApiUrl3 = db.GetConfig<string>("ApiUrl3");
            Config.DateLastLoadGuid = db.GetConfig<DateTime>("DateLastLoadGuid");
            Config.CodeWarehouse = db.GetConfig<int>("CodeWarehouse");
            Config.TypeUsePrinter = db.GetConfig<eTypeUsePrinter>("TypeUsePrinter");
            var tempstr = db.GetConfig<string>("CodesWarehouses");
            if (!string.IsNullOrEmpty(tempstr)) Config.CodesWarehouses = JsonConvert.DeserializeObject<List<int>>(tempstr);
            FileLogger.TypeLog = db.GetConfig<eTypeLog>("TypeLog");
            c = Connector.GetInstance();
            if (c != null)
            {
                LS = c.LoginServer();
                if (LS == null || LS.Count() == 1)
                {
                    IsVisLS = false;
                    Config.LoginServer = LS.First().Code;
                }
            }

            if (Config.IsAutoLogin)
            {
                Password = db.GetConfig<string>("Password");
                OnButtonLogin(null, null);
            }
           
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Settings());
        }

        private void OnAuthorizationClicked(object sender, EventArgs e)
        {
            ListDocs.IsVisible = false;
            SLLogin.IsVisible = true;
        }
    }
}