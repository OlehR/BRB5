using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using BRB5.Connector;
using BRB5.Model;
using BRB5.View;
using Xamarin.Essentials;
using System.Threading;

namespace BRB5
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<TypeDoc> OCTypeDoc { get; set; }
        BRB5.Connector.Connector c;
        DB db = DB.GetDB();
        public string Login { get; set; }
        public string Password { get; set; }        
        public MainPage()
        {
            OCTypeDoc = new ObservableCollection<TypeDoc>();
            c = BRB5.Connector.Connector.GetInstance();
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
                db.SetConfig<bool>("IsAutoLogin", true);
                db.SetConfig<string>("Password", Password);
                Config.Login = Login;
                Config.Password = Password;
                SLLogin.IsVisible = false;
                ListDocs.IsVisible = true;

                OCTypeDoc.Clear();
                foreach (var i in c.GetTypeDoc(Config.Role)) OCTypeDoc.Add(i);
            }
            else
                _ = DisplayAlert("Проблеми з автоизацією", r.TextError + r.Info, "OK");
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
                    await Navigation.PushAsync(new DocItem(vTypeDoc.CodeDoc));
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
            Config.IsAutoLogin = db.GetConfig<bool>("IsAutoLogin");
            Login = db.GetConfig<string>("Login");
            if (Config.IsAutoLogin)
            {
                Password = db.GetConfig<string>("Password");
            }
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
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
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

    }
}
