using System.Collections.ObjectModel;
using BL.Connector;
using BRB5.Model;
using BRB6.View;
using BL;
using BRB5;

//[assembly: Xamarin.Forms.Dependency(typeof(AndroidStorageManager))]

namespace BRB6
{
    public partial class MainPage 
    {
        public ObservableCollection<TypeDoc> OCTypeDoc { get; set; }
        Connector c;
        DB db;
        BL.BL Bl;
        public string Login { get; set; }
        public string Password { get; set; }
        public IEnumerable<LoginServer> LS { get; set; }

        public int SelectedLS { get { return LS == null || LS.Count() == 1 ? 0 : LS.ToList().FindIndex(x => x.Code == Config.LoginServer); } set { Config.LoginServer = LS.ToList()[value].Code; } }
        public bool IsVisLS { get; set; } = true;
        private bool _IsVisibleBack = false;
        public bool IsVisibleBack { get { return _IsVisibleBack; } set { _IsVisibleBack = value; OnPropertyChanged(nameof(IsVisibleBack)); } }
        public string Ver { get { return"BRB5 (" + AppInfo.VersionString + ")"; } }
        public string Company { get { return Enum.GetName(typeof(eCompany), Config.Company); } }
        public bool IsSoftKeyboard { get { return Config.IsSoftKeyboard; } }
        public MainPage()
        {

            ProtoBRB.Init();
            db = DB.GetDB(ProtoBRB.GetPathDB);
            Bl = BL.BL.GetBL();
            Config.Company = db.GetConfig<eCompany>("Company");
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
                    Config.TypeDoc = c.GetTypeDoc(Config.Role, Config.LoginServer);
                    Dispatcher.Dispatch(() =>
                    {
                        OCTypeDoc?.Clear();                        
                        //OCTypeDoc = new ObservableCollection<TypeDoc>(Config.TypeDoc);
                        foreach (var i in Config.TypeDoc) OCTypeDoc.Add(i);
                        SLLogin.IsVisible = false;
                        ListDocs.IsVisible = true;
                    });
                   Bl.OnButtonLogin(Login, Password, DeviceInfo.Platform == DevicePlatform.Android);
                }
                else
                    Dispatcher.Dispatch(() =>
                    {
                        _ = DisplayAlert("Проблеми з авторизацією", r.TextError + r.Info, "OK");
                    });
            });
            }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item == null) return;

            // Отримання вибраного елемента
            var vTypeDoc = e.Item as TypeDoc;

            var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();

            if (cameraStatus != PermissionStatus.Granted)
                cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();

            if (cameraStatus != PermissionStatus.Granted)
            {
                await DisplayAlert("Error", "Need camera permission", "OK", FlowDirection.MatchParent);
                return;
            }

            switch (vTypeDoc.KindDoc)
            {
                case eKindDoc.PriceCheck:
                    await Navigation.PushAsync(new PriceCheck(vTypeDoc));
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
                case eKindDoc.ExpirationDate:
                    await Navigation.PushAsync(new ExpirationDate());
                    break;
                case eKindDoc.NotDefined:
                    Dispatcher.Dispatch(() =>
                    {
                        OCTypeDoc?.Clear();
                        Config.TypeDoc = c.GetTypeDoc(Config.Role, Config.LoginServer, vTypeDoc.Group);
                        foreach (var i in Config.TypeDoc) OCTypeDoc.Add(i);
                        IsVisibleBack = true;
                    });
                    break;
                default:
                    break;
            }

            // Скидання вибраного елемента, щоб зняти виділення
            ((ListView)sender).SelectedItem = null;
        }

        void Init()
        {
            _ = LocationBrb.GetCurrentLocation(db.GetWarehouse());
            Login = db.GetConfig<string>("Login");
            Bl.Init();
            c = ConnectorBase.GetInstance();
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

        protected override void OnDisappearing()  {  base.OnDisappearing(); }

        private async void OnSettingsClicked(object sender, EventArgs e) { await Navigation.PushAsync(new Settings());  }

        private void OnAuthorizationClicked(object sender, EventArgs e)
        {
            ListDocs.IsVisible = false;
            SLLogin.IsVisible = true;
        }

        private void BackToMainList(object sender, EventArgs e)
        {
            Dispatcher.Dispatch(() =>
            {
                IsVisibleBack = false;
                OCTypeDoc?.Clear();
                Config.TypeDoc = c.GetTypeDoc(Config.Role, Config.LoginServer);
                foreach (var i in Config.TypeDoc) OCTypeDoc.Add(i);
            });
        }
    }
}