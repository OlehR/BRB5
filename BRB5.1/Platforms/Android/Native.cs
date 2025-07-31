using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Net;
using Android.Net.Wifi;
using BRB5.Model;
using BRB6.PlatformDependency;
using System.Net;
using Utils;

namespace BRB6
{
    public class Native : NativeBase
    {
        public override byte[] ResizeImage(byte[] imageData, float max, int compress = 90)
        {
            // Load the bitmap            
            Bitmap resizedImage,originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

            int imageMax = Math.Max(originalImage.Width, originalImage.Height);
            if (max < imageMax)
            {
                bool IsHeightMax = originalImage.Width < originalImage.Height;
                var coef = max / imageMax;

                var height = (int)(IsHeightMax ? max : originalImage.Height * coef);
                var width = (int)(!IsHeightMax ? max : originalImage.Width * coef);

                resizedImage = Bitmap.CreateScaledBitmap(originalImage, width, height, false);
            }
            else
                resizedImage = originalImage;
            using (MemoryStream ms = new MemoryStream())
            {
                resizedImage.Compress(Bitmap.CompressFormat.Jpeg, compress, ms);
                return ms.ToArray();
            }
        }

        public override string GetIP()
        {
            string Res = null ;
            /*WifiManager wifiManager = (WifiManager)Android.App.Application.Context.GetSystemService(Service.WifiService);
            int ipaddress = wifiManager.ConnectionInfo.IpAddress;
            IPAddress ipAddr = new IPAddress(ipaddress);
            Res = ipAddr.ToString();*/
            var connectivityManager = (ConnectivityManager)Platform.AppContext.GetSystemService(Context.ConnectivityService);
            var linkProperties = connectivityManager?.GetLinkProperties(connectivityManager?.ActiveNetwork);

            if (linkProperties != null)            
                foreach (var linkAddress in linkProperties.LinkAddresses)                
                    if (linkAddress?.Address is Java.Net.Inet4Address inet4Address)                    
                        Res = inet4Address.HostAddress;                    
            return Res;
        }

        public override async Task<bool> CheckNewVerAsync()
        {
            try
            {
                var versionString = AppInfo.BuildString;
                var ver = await UtilAndroid.DownloadStringAsync("https://raw.githubusercontent.com/OlehR/BRB5/master/Apk/Ver.txt");
                return ver.ToInt() > versionString.ToInt();
            }
            catch (Exception e)
            {
                FileLogger.WriteLogMessage("Native", "CheckNewVerAsync", e);
                return false;
            }
        }
        public override async Task InstallAsync(Action<double> pA) { await UtilAndroid.InstallAPKAsync(pA); }
    }
}