using Page = Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Page;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace BRB5.View
{
    public class BaseContentPage : ContentPage
    {
        public BaseContentPage()
        {
            Page.SetUseSafeArea(this, true);
        }
    }
}