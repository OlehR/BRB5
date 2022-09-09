using BRB5.Model;
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
        public TypeDoc TypeDoc { get; set; }
        public Scan(int pTypeDoc)
        {
            InitializeComponent();
            TypeDoc = Config.GetDocSetting(pTypeDoc);

            zxing.OnScanResult += (result) =>
                Device.BeginInvokeOnMainThread(async () =>
                // Stop analysis until we navigate away so we don't keep reading barcodes
                {
                    zxing.IsAnalyzing = false;

                    //FoundWares(result.Text, false);


                    zxing.IsAnalyzing = true;
                });
        }
    }
}