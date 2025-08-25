//using BRB5.Model;

using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Utils;

namespace BRB6
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                var ex = eventArgs.ExceptionObject as Exception;
                FileLogger.WriteLogMessage(this, "UnhandledException", ex);
                MainPage.DisplayAlert("Error", "An unexpected error occurred. Please restart the application." + ex.Message, "OK");
            };
            TaskScheduler.UnobservedTaskException += (sender, eventArgs) =>
            {
                var ex = eventArgs.Exception;
                FileLogger.WriteLogMessage(this, "UnobservedTaskException", ex);
                eventArgs.SetObserved(); // Prevents the application from crashing
                                         // Log the exception, display an alert, or perform other error handling
                MainPage.DisplayAlert("Error", "An unobserved task error occurred. "+ex.Message, "OK");
            };
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            Application.Current.UserAppTheme = AppTheme.Light;
            MainPage = new NavigationPage(new MainPage());
           
            
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
            var ex = e.Exception as System.Exception;           
            //FileLogger.WriteLogMessage(this, "GlobalException", ex);
            MainPage.DisplayAlert("Error", "An unexpected error occurred. Please restart the application." + ex.Message, "OK");
        }


    }
}
