using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using BRB5.Connector;
using BRB5.Model;
using BRB5.View;
using Xamarin.Essentials;
using System.Threading;
using Utils;

//[assembly: Xamarin.Forms.Dependency(typeof(AndroidStorageManager))]

namespace BRB5
{

    public partial class MainPage : ContentPage
    {

        public ObservableCollection<TypeDoc> OCTypeDoc { get; set; }
        BRB5.Connector.Connector c;
        DB db = DB.GetDB();
        Utils u = Utils.GetUtils();
        public string Login { get; set; }
        public string Password { get; set; }
        public IEnumerable<LoginServer> LS { get; set; }

        public int SelectedLS { get {  return LS == null || LS.Count()==1? 0 : LS.ToList().FindIndex(x => x.Code == Config.LoginServer); } set { Config.LoginServer = LS.ToList()[value].Code; } }
        public bool IsVisLS { get; set; } = true;
        public string Ver { get { return "BRB5 (" + AppInfo.VersionString + ")"; } }
        public string Company { get { return Enum.GetName(typeof(eCompany), Config.Company); } }
        public MainPage()
        {
            OCTypeDoc = new ObservableCollection<TypeDoc>();
            c = Connector.Connector.GetInstance();
            InitializeComponent();            
            Init();
            
            BindingContext = this;
        }

        private  void OnButtonLogin(object sender, System.EventArgs e)
        {
            var r = c.Login(Login, Password,Config.LoginServer);
            if (r.State == 0)
            {
                db.SetConfig<string>("Login", Login);
                //db.SetConfig<bool>("IsAutoLogin", true);
                db.SetConfig<string>("Password", Password);
                db.SetConfig<eLoginServer>("LoginServer", Config.LoginServer);
                Config.Login = Login;
                Config.Password = Password;
                SLLogin.IsVisible = false;
                ListDocs.IsVisible = true;
                //eLoginServer LoginServer;

                OCTypeDoc?.Clear();
                Config.TypeDoc = c.GetTypeDoc(Config.Role, Config.LoginServer);
                //OCTypeDoc = new ObservableCollection<TypeDoc>(Config.TypeDoc);
                foreach (var i in Config.TypeDoc) OCTypeDoc.Add(i);

                var Wh=c.LoadWarehouse();
                db.ReplaceWarehouse(Wh);

                long SizeDel = 0, SizeUse = 0;
                if (Config.Company == eCompany.Sim23)
                {
                    var a = db.GetDoc(11);
                     (SizeDel, SizeUse) = u.DelDir(Config.PathFiles, a.Select(el=>el.NumberDoc));
                }

                if(Config.DateLastLoadGuid.Date != DateTime.Today.Date)
                {
                    c.LoadGuidData(true);
                    Config.DateLastLoadGuid = DateTime.Now;
                    db.SetConfig<DateTime>("DateLastLoadGuid", Config.DateLastLoadGuid);
                }
            }
            else
                _ = DisplayAlert("Проблеми з авторизацією", r.TextError + r.Info, "OK");
        }

        private async void OnButtonClicked(object sender, System.EventArgs e)
        {           
            Button button = (Button)sender;
            Cell cc = button.Parent as Cell;
            var vTypeDoc = cc.BindingContext as TypeDoc;

            switch (vTypeDoc.KindDoc)
            {
                case eKindDoc.PriceCheck:
                    await Navigation.PushAsync(new PriceCheck());//new CustomScanPage()); // 
                    break;
                case eKindDoc.Normal:
                case eKindDoc.Simple:
                    await Navigation.PushAsync(new DocsStandart(vTypeDoc));
                    break;
                case eKindDoc.Raiting:
                    await Navigation.PushAsync(new Docs(vTypeDoc.CodeDoc));
                    break;
                default:                  
                    break;
            }
        }      


        void Init()
        {
            _ = GetCurrentLocation();
            Config.IsAutoLogin = db.GetConfig<bool>("IsAutoLogin");
            Config.LoginServer = db.GetConfig<eLoginServer>("LoginServer");
            Login = db.GetConfig<string>("Login");
            if (Config.IsAutoLogin)
            {
                Password = db.GetConfig<string>("Password");
                OnButtonLogin(null,null);
            }
            
            if (c != null)
            {
                LS = c.LoginServer();
                if (LS == null || LS.Count() == 1)
                {
                    IsVisLS = false;
                    Config.LoginServer = LS.First().Code;
                }

            }
            Config.IsVibration = db.GetConfig<bool>("IsVibration");
            Config.IsSound = db.GetConfig<bool>("IsSound");
            Config.IsTest = db.GetConfig<bool>("IsTest");
            Config.ApiUrl1 = db.GetConfig<string>("ApiUrl1");
            Config.ApiUrl2 = db.GetConfig<string>("ApiUrl2");
            Config.ApiUrl3 = db.GetConfig<string>("ApiUrl3");
            Config.DateLastLoadGuid = db.GetConfig<DateTime>("DateLastLoadGuid");
            Config.CodeWarehouse = db.GetConfig<int>("CodeWarehouse");
            Config.Company = db.GetConfig<eCompany>("Company");
            Config.TypeUsePrinter = db.GetConfig<eTypeUsePrinter>("TypeUsePrinter");
            FileLogger.TypeLog = db.GetConfig<eTypeLog>("TypeLog");
            /*var Wh = GetCurrentLocation().Result;
           var FWh = Wh.First();
           if (FWh.Distance>0 && FWh.Distance<0.032)
           {
               //Знайшли текучий магазин
           }
           else //вибір вручну магазина.
           { }*/

        }

        CancellationTokenSource cts;

        async Task<IEnumerable<Warehouse>> GetCurrentLocation()
        {
            var Wh = db.GetWarehouse();
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token);
                
                if (location != null)
                {
                    foreach(var el in Wh)
                    {
                        el.Distance = location.CalculateDistance(el.GPSX, el.GPSY, DistanceUnits.Kilometers);
                    }
                    Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    return Wh.OrderBy(o => o.Distance);
                    
                }                
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
            }

            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
            }
            catch (Exception ex)
            {
                // Unable to get location
            }
            return Wh;
        }

        protected override void OnDisappearing()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
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
