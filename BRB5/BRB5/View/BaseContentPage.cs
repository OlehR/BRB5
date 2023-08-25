using Xamarin.Forms;
using Page = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Page;

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