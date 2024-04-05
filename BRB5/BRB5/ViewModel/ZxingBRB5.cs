using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;
using ZXing;

namespace BRB5.ViewModel
{
    public class ZxingBRB5
    {

        /// <summary>
        /// Костиль через баг https://github.com/Redth/ZXing.Net.Mobile/issues/710
        /// </summary>
        public static ZXingScannerView SetZxing(Grid pV, ZXingScannerView pZxing, Action<string> action)
        {
            if (pZxing != null)
            {
                if (Device.RuntimePlatform == Device.iOS)
                    return pZxing;
                pV.Children.Remove(pZxing);
            }
            pZxing = new ZXingScannerView();
            pV.Children.Add(pZxing);

            pZxing.Options = new MobileBarcodeScanningOptions
            {
                PossibleFormats = new List<BarcodeFormat>
                    {
                        BarcodeFormat.EAN_13,
                        BarcodeFormat.EAN_8,
                        BarcodeFormat.QR_CODE,
                    },
                UseNativeScanning = true,
                TryUseUltraWideCamera =true,
                DelayBetweenContinuousScans=2000
            };
            pZxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                {
                    pZxing.IsAnalyzing = false;
                    action?.Invoke(result.Text);
                    pZxing.IsAnalyzing = true;
                });
            return pZxing;
        }
    }
}
