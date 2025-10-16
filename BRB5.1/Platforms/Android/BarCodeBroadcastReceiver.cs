using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BRB5;
using BRB5.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Utils;
//using Utils;

namespace BRB6
{
    [BroadcastReceiver(Enabled = true)]
    [Service(Exported = true)]
    [IntentFilter(["device.scanner.EVENT", "ua.uz.vopak.brb4", "ua.UniCS.TM.BRB", "com.scanner.broadcast"],Categories = [Intent.CategoryDefault] )]
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
                    case eTypeScaner.PM84:
                    case eTypeScaner.PM68:
                        Res = "device.scanner.EVENT";
                        break;
                    case eTypeScaner.Zebra:
                        Res = "ua.UniCS.TM.BRB"; //"ua.uz.vopak.brb4";
                        break;
                    case eTypeScaner.BitaHC61:
                    case eTypeScaner.ChainwayC61:
                    case eTypeScaner.MetapaceM_K4:
                    case eTypeScaner.NLS_MT67:
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
                    case eTypeScaner.PM84:
                    case eTypeScaner.PM68:
                        Res = "EXTRA_EVENT_DECODE_VALUE";
                        break;
                    case eTypeScaner.Zebra:
                        Res = "com.symbol.datawedge.data_string";
                        break;
                    case eTypeScaner.BitaHC61:
                    case eTypeScaner.ChainwayC61:
                    case eTypeScaner.MetapaceM_K4:
                    case eTypeScaner.NLS_MT67:
                        Res = "data";
                        break;
                }
                return Res;
                    } }
        static bool IsByte { get { return Config.TypeScaner == eTypeScaner.PM351 || Config.TypeScaner == eTypeScaner.PM550 || Config.TypeScaner == eTypeScaner.PM84 || Config.TypeScaner == eTypeScaner.PM68 ; } }

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
                //FileLogger.WriteLogMessage($"MyBroadcastReceiver BarCodeScaner=>{Res}");
                if (Res != null && !Res.Equals("READ_FAIL"))
                {                    
                    Config.BarCode?.Invoke(Res.Replace("\n", ""));
                }                
            }
            catch (Exception e)
            {
                var m = e.Message;
                FileLogger.WriteLogMessage(this, "MyBroadcastReceiver.OnReceive", e);                
            }
        }
    }

}