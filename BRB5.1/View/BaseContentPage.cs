using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace BRB6.View
{
    public class BaseContentPage : ContentPage
    {
        public BaseContentPage()
        {
            this.On<iOS>().SetUseSafeArea(true);
        }
    }
}