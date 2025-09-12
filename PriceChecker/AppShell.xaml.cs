using PriceChecker.View;
namespace PriceChecker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(AdminPriceChecker), typeof(AdminPriceChecker));
            Routing.RegisterRoute(nameof(UPriceChecker), typeof(UPriceChecker));
            Routing.RegisterRoute(nameof(PrintPage), typeof(PrintPage));
            //Routing.RegisterRoute(nameof(SplashPage), typeof(SplashPage));
        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            // Вимикаємо меню на SplashPage та UPriceChecker для неавторизованих
            if (args.Current.Location.OriginalString.Contains(nameof(SplashPage)) ||
                args.Current.Location.OriginalString.Contains(nameof(UPriceChecker)))
            {
                FlyoutBehavior = FlyoutBehavior.Disabled;
            }
            else
            {
                FlyoutBehavior = FlyoutBehavior.Flyout;
            }
        }
    }
}
