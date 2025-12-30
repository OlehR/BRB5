using BL;
using BL.Connector;
using BRB5.Model;
using BRB5;

namespace BRB6.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class WareInfo : ContentPage
    {
        DB db = DB.GetDB(); 
        Connector c;
        public WaresPrice WP { get; set; }
        public bool IsVisPromotion {  get; set; }  = false;
        private bool _IsNotFullScreenImg = true;
        public bool IsNotFullScreenImg { get { return _IsNotFullScreenImg; } set { _IsNotFullScreenImg = value; OnPropertyChanged(nameof(IsNotFullScreenImg));  } }
        private bool _IsFullScreenImg = false;
        public bool IsFullScreenImg { get { return _IsFullScreenImg; } set { _IsFullScreenImg = value; OnPropertyChanged(nameof(IsFullScreenImg)); } }
        private bool _IsVisIOSFull = false;
        public bool IsVisIOSFull { get { return _IsVisIOSFull; } set { _IsVisIOSFull = value; OnPropertyChanged(nameof(IsVisIOSFull)); } }
        public string ImageUri { get; set; } = "photo.png"; 
        public bool IsEnabledPrint { get { return Config.TypeUsePrinter != eTypeUsePrinter.NotDefined; } }
        public UriImageSource Picture { get; set; }
        public Uri UriPicture { get { return new Uri(Config.ApiUrl1 + $"Wares/{WP.CodeWares:D9}.png"); } }
        private int InfoHeight;

        public WareInfo(ParseBarCode parseBarCode)
        {
            InitializeComponent();
            c = ConnectorBase.GetInstance();
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            NavigationPage.SetHasNavigationBar(this, DeviceInfo.Platform == DevicePlatform.iOS);            
            WP = c.GetPrice(parseBarCode, eTypePriceInfo.Full)?.Data;                
            if (WP.RestWarehouse != null)  RestWarehouseListShow(WP.RestWarehouse);
            if (WP.Сondition != null) FillConditionList(WP.Сondition);
            if (WP.ActionType > 0)  IsVisPromotion = true;            
            ImageUri = Config.ApiUrl1+$"Wares/{WP.CodeWares:D9}.png";            
            Picture  = new UriImageSource
            {
                Uri = new Uri(ImageUri),
                CachingEnabled = false,
                CacheValidity = new TimeSpan(7, 0, 0, 0)
            };
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (DeviceInfo.Platform == DevicePlatform.iOS) 
            {
                WareImageIOS.IsVisible = true; 
                WareImage.IsVisible = false;
            } 
            this.BindingContext = this;
            CalculateAndSetScrollViewHeight();
        }
        private void RestWarehouseListShow(IEnumerable<RestWarehouse> warehouses)
        {
            var t = db.GetWarehouse();
            var w = t.FirstOrDefault(x => x.CodeWarehouse == Config.CodeWarehouse);
            foreach (var warehouse in warehouses)
            {
                var grid = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(7, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) }
                    },
                    BackgroundColor = Color.FromArgb("#adaea7")
                };

                var NameWarehouseLabel = new Label
                {
                    Text = warehouse.NameWarehouse,
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.NoWrap,
                    BackgroundColor = Colors.White

                };

                var QuantityLabel = new Label
                {
                    Text = warehouse.Quantity.ToString(),
                    TextColor = Colors.Black,
                    FontAttributes = FontAttributes.Bold,
                    LineBreakMode = LineBreakMode.NoWrap,
                    BackgroundColor = Colors.White
                };
                Grid.SetColumn(QuantityLabel, 1);

                var DateLabel = new Label
                {
                    Text = warehouse.Date.ToString("dd.MM.yyyy"),
                    TextColor = Colors.Black,
                    LineBreakMode = LineBreakMode.NoWrap,
                    BackgroundColor = Colors.White
                };
                Grid.SetColumn(DateLabel, 2);

                if (w != null && w.Name==warehouse.NameWarehouse)
                {
                    NameWarehouseLabel.BackgroundColor = Colors.SandyBrown;
                    QuantityLabel.BackgroundColor = Colors.SandyBrown;
                    DateLabel.BackgroundColor = Colors.SandyBrown;
                }

                grid.Children.Add(NameWarehouseLabel);
                grid.Children.Add(QuantityLabel);
                grid.Children.Add(DateLabel);
                RestWarehouseList.Children.Add(grid);
            }
        }
        public void FillConditionList(IEnumerable<СonditionClass> conditions)
        {
            ConditionList.Children.Clear();

            foreach (var condition in conditions)
            {
                var stackLayout = new StackLayout
                {
                    Orientation = StackOrientation.Horizontal
                };

                var conditionLabel = new Label
                {
                    Text = condition.Сondition
                };

                var contrLabel = new Label
                {
                    Text = condition.Contr
                };

                stackLayout.Children.Add(conditionLabel);
                stackLayout.Children.Add(contrLabel);

                ConditionList.Children.Add(stackLayout);
            }
        }
        private void CalculateAndSetScrollViewHeight()
        {
            var mainDisplayInfo = DeviceDisplay.Current.MainDisplayInfo;
            var screenHeight = mainDisplayInfo.Height / mainDisplayInfo.Density;
            var navigationBarHeight = GetNavigationBarHeight();
            var imageHeight = WareImage.HeightRequest;
            var scrollViewHeight = screenHeight - imageHeight - navigationBarHeight;
            WareInfoMainScrollView.HeightRequest = scrollViewHeight;
        }

        private double GetNavigationBarHeight()
        {
            const double navigationBarHeightDp = 36;
            var density = DeviceDisplay.Current.MainDisplayInfo.Density;
            return navigationBarHeightDp * density;
        }

        private void OnClickPrint(object sender, EventArgs e) {  if (IsEnabledPrint)  _ = DisplayAlert("Друк", c.PrintHTTP(new[] { WP.CodeWares }), "OK");     }
        private void OnImageTapped(object sender, EventArgs e)
        {
            IsNotFullScreenImg = !IsNotFullScreenImg;
            // TODO Xamarin.Forms.Device.RuntimePlatform is no longer supported. Use Microsoft.Maui.Devices.DeviceInfo.Platform instead. For more details see https://learn.microsoft.com/en-us/dotnet/maui/migration/forms-projects#device-changes
            if (DeviceInfo.Platform == DevicePlatform.iOS)
                 IsVisIOSFull = !IsVisIOSFull;
            else IsFullScreenImg = !IsFullScreenImg;
        }
    }
}