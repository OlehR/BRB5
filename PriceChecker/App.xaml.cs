namespace PriceChecker
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

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