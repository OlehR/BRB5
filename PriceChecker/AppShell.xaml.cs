using PriceChecker.View;
namespace PriceChecker
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Сторінки, які поза меню
            Routing.RegisterRoute(nameof(UPriceChecker), typeof(UPriceChecker));
            Routing.RegisterRoute(nameof(CustomerInfo), typeof(CustomerInfo));

        }

        protected override void OnNavigated(ShellNavigatedEventArgs args)
        {
            base.OnNavigated(args);

            // Splash і UPriceChecker – меню приховано
            if (args.Current.Location.OriginalString.Contains(nameof(SplashPage)) ||
                args.Current.Location.OriginalString.Contains(nameof(UPriceChecker)) ||
                args.Current.Location.OriginalString.Contains(nameof(CustomerInfo)))
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
