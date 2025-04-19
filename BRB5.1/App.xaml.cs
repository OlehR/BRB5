//using BRB5.Model;
using BRB5.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls;
using Microsoft.Maui;
using BRB5;
using System.Runtime.ExceptionServices;
using Microsoft.Maui.Controls.Shapes;
using Utils;

namespace BRB6
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage());
            Application.Current.UserAppTheme = AppTheme.Light;
            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {      
                    FileLogger.WriteLogMessage(this, "GlobalException", e.ToJson()); 
        }


    }
}
