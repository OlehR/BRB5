using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Scan : ContentPage
    {
        public Scan()
        {
            InitializeComponent();

            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                {
                    zxing.IsAnalyzing = false;




                    zxing.IsAnalyzing = true;
                });
        }
    }
}