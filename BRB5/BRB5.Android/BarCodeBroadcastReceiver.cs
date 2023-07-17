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

namespace BRB5.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { "device.scanner.EVENT" })]
    public class MySampleBroadcastReceiverPM550 : BroadcastReceiver
    {
        static readonly public string IntentEvent= "device.scanner.EVENT";
        static readonly public string IntentEventValue = "device.scanner.EVENT";
        static  Action<string> BarCode;

        public override void OnReceive(Context context, Intent intent)
        {
            var data =intent.GetByteArrayExtra(IntentEventValue);
            string Res= data.ToString();
            BarCode?.Invoke(Res);
            // Do stuff here
        }
    }
}