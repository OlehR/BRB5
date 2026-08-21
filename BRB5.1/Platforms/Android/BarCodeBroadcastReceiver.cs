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
                string Res  = Config.TypeScaner switch
                {
                    eTypeScaner.PM351 or eTypeScaner.PM550 or eTypeScaner.PM84 or eTypeScaner.PM68 => "device.scanner.EVENT",
                    eTypeScaner.Zebra or eTypeScaner.ZebraWithOutKeyBoard => "ua.UniCS.TM.BRB",//"ua.uz.vopak.brb4";
                    eTypeScaner.BitaHC61 or eTypeScaner.ChainwayC61 or eTypeScaner.ChainwayC66 or eTypeScaner.MetapaceM_K4 or eTypeScaner.NLS_MT67 or eTypeScaner.NLS_MT93 or eTypeScaner.MEFERI_ME61  => "com.scanner.broadcast",
                    _ => "ua.UniCS.TM.BRB",
                };
                return Res;
                    } }

        static public string IntentEventValue { get {
                string Res = Config.TypeScaner switch
                {
                    eTypeScaner.PM351 or eTypeScaner.PM550 or eTypeScaner.PM84 or eTypeScaner.PM68 => "EXTRA_EVENT_DECODE_VALUE",
                    eTypeScaner.Zebra or eTypeScaner.ZebraWithOutKeyBoard => "com.symbol.datawedge.data_string",             
                    eTypeScaner.BitaHC61 or eTypeScaner.ChainwayC61 or eTypeScaner.ChainwayC66 or eTypeScaner.MetapaceM_K4 or eTypeScaner.NLS_MT67 or eTypeScaner.NLS_MT93 or eTypeScaner.MEFERI_ME61=> "data",
                    _=> "data"
                };
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