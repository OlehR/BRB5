//using BRB5.Model;

using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Utils;
using Timer = System.Timers.Timer;
#if ANDROID 
using Android.Views;
#endif

namespace BRB6
{
    public partial class App : Application
    {
        private Timer InactivityTimer;

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
                //MainPage.DisplayAlert("Error", "An unobserved task error occurred. "+ex.Message, "OK");
            };
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            Application.Current.UserAppTheme = AppTheme.Light;
            MainPage = new NavigationPage(new MainPage());


            InactivityTimer = new Timer(TimeSpan.FromMinutes(5).TotalMilliseconds); 
            InactivityTimer.Elapsed += InactivityTimerElapsed;
        }

        protected override void OnStart()
        {
            base.OnStart();
            InactivityTimer?.Start();
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            InactivityTimer?.Stop();
#if ANDROID
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activity = Platform.CurrentActivity;
                activity?.Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
            });
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();
            InactivityTimer?.Start();
        }

        private void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {      
            FileLogger.WriteLogMessage(this, "GlobalException", e.Exception);
            var ex = e.Exception as System.Exception;           
            //FileLogger.WriteLogMessage(this, "GlobalException", ex);
            MainPage.DisplayAlert("Error", "An unexpected error occurred. Please restart the application." + ex.Message, "OK");
        }

        private void InactivityTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
#if ANDROID
                var activity = Platform.CurrentActivity;
                activity?.Window?.ClearFlags(WindowManagerFlags.KeepScreenOn);
#endif
            });
        }

        public void ResetInactivityTimer()
        {
            InactivityTimer.Stop();
            InactivityTimer.Start();

#if ANDROID
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var activity = Platform.CurrentActivity;
                activity?.Window?.AddFlags(WindowManagerFlags.KeepScreenOn);
            });
#endif
        }
    }
}
