//using BRB5.Model;
using System.Runtime.ExceptionServices;
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
                    FileLogger.WriteLogMessage(this, "GlobalException", e.Exception);
            var s = e.Exception.Message;
        }


    }
}
