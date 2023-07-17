using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utils;

namespace BRB5.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "device.scanner.EVENT" })]
    public class MyBroadcastReceiverPM550 : BroadcastReceiver
    {
        static readonly public string IntentEvent= "device.scanner.EVENT";
        static readonly public string IntentEventValue = "EXTRA_EVENT_DECODE_VALUE";
        static  Action<string> BarCode;

        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                var data = intent.GetByteArrayExtra(IntentEventValue);
                FileLogger.WriteLogMessage($"MyBroadcastReceiverPM550  BarCodeScaner=>{data.Count()}");
                string Res = System.Text.Encoding.Default.GetString(data);
                //BarCode?.Invoke(Res);
                FileLogger.WriteLogMessage($"MyBroadcastReceiverPM550  BarCodeScaner=>{Res}");
            }
            catch (Exception e)
            {
                
                var m = e.Message;
                FileLogger.WriteLogMessage($"MyBroadcastReceiverPM550 Exception=>{m}");
            }
        }
    }
}