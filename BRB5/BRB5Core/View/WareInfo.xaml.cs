﻿using BRB5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WareInfo : ContentPage
    {
        DB db = DB.GetDB(); 
        Connector.Connector c;
        public WaresPrice WP { get; set; }
        public bool IsVisPromotion {  get; set; }  = false;
        public bool IsNotFullScreenImg { get; set; } = true;
        public bool IsFullScreenImg { get; set; } = false;
        public string ImageUri { get; set; } = "Photo.png"; 
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        public WareInfo(ParseBarCode parseBarCode)
        {
            InitializeComponent();
            c = Connector.Connector.GetInstance();
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, Device.RuntimePlatform == Device.iOS);
            
            WP = c.GetPrice(parseBarCode, eTypePriceInfo.Full);
           
            if (WP.ActionType > 0)  IsVisPromotion = true;
            
            ImageUri = Config.ApiUrl1+$"Wares/{WP.CodeWares:D9}.png";
            WareImage.Source = new UriImageSource
            {
                Uri = new Uri(ImageUri),
                CachingEnabled = true,
                CacheValidity = new TimeSpan(7, 0, 0, 0)
            };
            WareImageFull.Source = new UriImageSource
            {
                Uri = new Uri(ImageUri),
                CachingEnabled = true,
                CacheValidity = new TimeSpan(7, 0, 0, 0)
            };

            this.BindingContext = this;
        }

        private void OnClickPrint(object sender, EventArgs e)
        {
            if (IsEnabledPrint) 
                _ = DisplayAlert("Друк", c.PrintHTTP(new[] { WP.CodeWares }), "OK");            
        }

        private void OnImageTapped(object sender, EventArgs e)
        {
            WareImageFull.IsVisible = !WareImageFull.IsVisible;
            WareInfoMain.IsVisible = !WareInfoMain.IsVisible;
        }
    }
}