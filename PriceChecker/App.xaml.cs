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

            AppConfiguration = new ConfigurationBuilder()
                .AddJsonFile( "appsettings.json")
                .Build();
            FileLogger.Init("Logs", 0, eTypeLog.Full);
            Config.ComPortScaner = AppConfiguration["ComPortScaner"]??"COM9";
            FileLogger.WriteLogMessage("App", "App", "Start");
            ScanerCom =new ScanerCom(Config.ComPortScaner, 9600);
            //ScanerCom.Init();
            FileLogger.WriteLogMessage("App", "App", "End");
        }
        static public ScanerCom ScanerCom;
        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());

            //const int newheight = 768;
            //const int newwidth = 1024;
            //var wins = new Window(new AppShell());
            //wins.Height = wins.MinimumHeight = wins.MaximumHeight = newheight;
            //wins.Width = wins.MinimumWidth = wins.MaximumWidth = newwidth;
            //return wins;
        }
    }
}