using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace BRB5.Model
{
    public class Config
    {
        public static Action<string> BarCode;
        public static Action<double> OnProgress;
        public static string Manufacturer;
        public static string Model;
        //static DB db = DB.GetDB();
        public static int CodeWarehouse;
        static eCompany _Company = eCompany.NotDefined;
        public static eTypeScaner TypeScaner;
        public static eCompany Company
        {
            get {
                if (_Company == eCompany.NotDefined)
                {
                    DB db = DB.GetDB();
                    _Company = db.GetConfig<eCompany>("Company");
                }
                return  _Company; }
            set { _Company = value; }
        }
        public static string PathDownloads = null;

        public static string ApiUrl1 { get; set; }

        public static string ApiUrl2 { get; set; }

        public static string ApiUrl3 { get; set; }

        public static string SN;
        public static DateTime DateLastLoadGuid { get; set; }
        public static int Ver { get { return int.Parse(AppInfo.VersionString.Replace(".", "")); } }
        public static bool IsAutoLogin { get; set; } = false;
        public static bool IsVibration { get; set; } = false;
        public static bool IsSound { get; set; } = false;
        public static bool IsTest { get; set; } = true;
        public static bool IsLoginCO = true;
        public static eLoginServer LoginServer = eLoginServer.Bitrix;
        public static string Login { get; set; } = "LOX";
        public static string Password { get; set; } = "321";
        public static eRole Role = eRole.NotDefined;
        public static int CodeUser { get; set; } = 233;
        public static string NameUser { get; set; }
        public static eTypeUsePrinter TypeUsePrinter { get; set; } = eTypeUsePrinter.NotDefined;
        
        public static int GetCodeUnitWeight { get { return Company == eCompany.Sim23 || Company == eCompany.Sim23FTP ? 166 : 7; } }

        public static int GetCodeUnitPiece { get { return Company == eCompany.Sim23 || Company == eCompany.Sim23FTP ? 796 : 19; } }

        public static string PathFiles { get { string res=@"D:\temp";
                try {
                    res = Path.Combine(Config.PathDownloads /*FileSystem.AppDataDirectory*/,"BRBFiles"); 
                    if(!Directory.Exists(res))
                        Directory.CreateDirectory(res);
                } catch (Exception e)
                { 
                } return res; } }

        public static IEnumerable<TypeDoc> TypeDoc;
        public static TypeDoc GetDocSetting(int pTypeDoc) {
            var r = TypeDoc.Where(el => el.CodeDoc == pTypeDoc);
            if (r.Count() == 1) return r.First();
            return null; }

        public static Action<Location> OnLocation { get; set; }
        public static Location Location =null;
        public static Warehouse LocationWarehouse = null;
        //static object Lock= new object();
        static CancellationTokenSource cts;
        public static async Task<IEnumerable<Warehouse>> GetCurrentLocation(IEnumerable<Warehouse> Wh)
        {
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(10));
                cts = new CancellationTokenSource();
                Location = await Geolocation.GetLocationAsync(request, cts.Token);

                if (Location != null)
                {
                    foreach (var el in Wh)
                    {
                        el.Distance = Location.CalculateDistance(el.GPSX, el.GPSY, DistanceUnits.Kilometers) * 1000;
                    }
                    Console.WriteLine($"Latitude: {Location.Latitude}, Longitude: {Location.Longitude}, Altitude: {Location.Altitude}");
                    var res = Wh.OrderBy(o => o.Distance);
                    if (res.Any())
                    {
                        LocationWarehouse = res.First();
                    }
                    OnLocation?.Invoke(Location);
                    return res;
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
        public void Dispose()
        {
            if (cts != null && !cts.IsCancellationRequested)
                cts.Cancel();
        }

        public static eTypeScaner GetTypeScaner()
        {
            if(Device.RuntimePlatform == Device.iOS)
                return eTypeScaner.Camera;
            if ( (Manufacturer.Contains("Zebra Technologies") || Manufacturer.Contains("Motorola Solutions")))
                return eTypeScaner.Zebra;
            if (Model.Equals("PM550") && (Manufacturer.Contains("POINTMOBILE") || Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM550;
            if (Model.Equals("PM351") && (Manufacturer.Contains("POINTMOBILE") || Manufacturer.Contains("Point Mobile Co., Ltd.")))
                return eTypeScaner.PM351;
            if (Model.Equals("HC61") || Manufacturer.Contains("Bita"))
                return eTypeScaner.BitaHC61;
            return eTypeScaner.Camera;
        }

        // public static string GenRaitingFileName(Raiting r);
    }
}
