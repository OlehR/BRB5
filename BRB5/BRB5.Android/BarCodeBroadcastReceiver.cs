using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace BRB5.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [Service(Exported = true)]
    [IntentFilter(new[] { "device.scanner.EVENT", "ua.uz.vopak.brb4", "com.symbol.datawedge.api.ACTION", "com.scanner.broadcast" })]
    public class MyBroadcastReceiver : BroadcastReceiver
    {
        public MyBroadcastReceiver() 
        {
            var xx = GetHashCode();
        }
        static public string IntentEvent { get {
                string Res = null;
                    switch(Config.TypeScaner)
                {
                    case eTypeScaner.PM351:
                    case eTypeScaner.PM550:
                        Res= "device.scanner.EVENT";
                        break;
                    case eTypeScaner.Zebra:
                        Res = "ua.uz.vopak.brb4";
                        break;
                    case eTypeScaner.BitaHC61:
                        Res = "com.scanner.broadcast" ;
                        break;
                }
                return Res;
                    } }

        static public string IntentEventValue { get {
                string Res = null;
                    switch(Config.TypeScaner)
                {
                    case eTypeScaner.PM351:
                    case eTypeScaner.PM550:
                        Res= "EXTRA_EVENT_DECODE_VALUE";
                        break;
                    case eTypeScaner.Zebra:
                        Res = "com.symbol.datawedge.data_string";
                        break;
                    case eTypeScaner.BitaHC61:
                        Res = "data";
                        break;
                }
                return Res;
                    } }
        static bool IsByte { get { return Config.TypeScaner == eTypeScaner.PM351 || Config.TypeScaner == eTypeScaner.PM550; } }

        public override void OnReceive(Context context, Intent intent)
        {
            var xx = GetHashCode();
            String Res = null;
            try
            {
                if (IsByte)
                {
                    var data = intent.GetByteArrayExtra(IntentEventValue);
                    Res = Encoding.Default.GetString(data);
                }
                else
                    Res = intent.GetStringExtra(IntentEventValue);
                FileLogger.WriteLogMessage($"MyBroadcastReceiver BarCodeScaner=>{Res}");
                if (Res != null && !Res.Equals("READ_FAIL"))
                {
                   // FileLogger.WriteLogMessage($"MyBroadcastReceiver Invoke=>{Res}");
                    Config.BarCode?.Invoke(Res);
                }
                
            }
            catch (Exception e)
            {
                var m = e.Message;
                FileLogger.WriteLogMessage($"MyBroadcastReceiver Exception=>{m}");
            }
        }
    }

}