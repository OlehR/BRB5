using BRB5.Model;
using Equipments;
using Microsoft.Extensions.Configuration;
using Utils;


namespace PriceChecker
{
    public partial class App : Application
    {

        public static IConfigurationRoot AppConfiguration;           

            
        public App()
        {
            InitializeComponent();
#if WINDOWS
            AppConfiguration = new ConfigurationBuilder()
                .AddJsonFile( "appsettings.json")
                .Build();

            FileLogger.Init("Logs", 0, eTypeLog.Full);

            Config.ComPortScaner = AppConfiguration["ComPortScaner"]??"COM9";
            Config.Login = AppConfiguration["Login"];
            Config.Password = AppConfiguration["Password"];
            ScanerCom =new ScanerCom(Config.ComPortScaner, 9600);
#endif
#if ANDROID
            Config.Login = "Price";
            Config.Password= "Price";
#endif
            FileLogger.WriteLogMessage("App", "App", "Start");          
   
            FileLogger.WriteLogMessage("App", "App", "End");
            Application.Current.UserAppTheme = AppTheme.Light;
            SetupExceptionHandling();
        }
        static public ScanerCom ScanerCom;
        protected override Window CreateWindow(IActivationState? activationState)
        {
            Window window = new Window(new AppShell());
            window.Destroying += Window_Destroying;
            return window;

            // return new Window(new AppShell());

            //const int newheight = 768;
            //const int newwidth = 1024;
            //var wins = new Window(new AppShell());
            //wins.Height = wins.MinimumHeight = wins.MaximumHeight = newheight;
            //wins.Width = wins.MinimumWidth = wins.MaximumWidth = newwidth;
            //return wins;
        }

        private void Window_Destroying(object sender, EventArgs e)
        {
            try
            {
                FileLogger.WriteLogMessage(this, "Window_Destroying", "Run explorer.exe");
                System.Diagnostics.Process.Start("explorer.exe");
            }
            catch (Exception)
            {
            }
        }

        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                LogUnhandledException((Exception)e.ExceptionObject, "AppDomain.CurrentDomain.UnhandledException");

            
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogUnhandledException(e.Exception, "TaskScheduler.UnobservedTaskException");
                e.SetObserved();
            };
        }
        private void LogUnhandledException(Exception exception, string source)
        {
            string message = $"Unhandled exception ({source})";
            try
            {
                FileLogger.WriteLogMessage(this, "LogUnhandledException", exception);
                System.Reflection.AssemblyName assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName();
                message = string.Format("Unhandled exception in {0} v{1}", assemblyName.Name, assemblyName.Version);
            }
            catch (Exception ex)
            {
                FileLogger.WriteLogMessage(this, "Exception in LogUnhandledException", ex);

            }
            finally
            {
                FileLogger.WriteLogMessage(this, message, exception);
            }
            if (!source.Equals("TaskScheduler.UnobservedTaskException"))
                try
                {                    
                        System.Diagnostics.Process.Start("explorer.exe");
                   // Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    FileLogger.WriteLogMessage(this, "Exception in LogUnhandledException", ex);
                }
        }
    }
}