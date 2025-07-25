using Utils;
using Equipments;


namespace PriceChecker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            FileLogger.WriteLogMessage("App", "App", "Start");
            ScanerCom =new ScanerCom("COM10",9600);
            //ScanerCom.Init();
            FileLogger.WriteLogMessage("App", "App", "End");
        }
        static public ScanerCom ScanerCom;
        protected override Window CreateWindow(IActivationState? activationState)
        {
            //return new Window(new AppShell());

            const int newheight = 768;
            const int newwidth = 1024;

            var wins = new Window(new AppShell());
            wins.Height = wins.MinimumHeight = wins.MaximumHeight = newheight;
            wins.Width = wins.MinimumWidth = wins.MaximumWidth = newwidth;
            return wins;
        }
    }
}