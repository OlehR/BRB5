using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using BRB5.Model;
using BRB5;

namespace BRB6.View
{
    public class BaseContentPage : ContentPage
    {
        public BaseContentPage()
        {
            this.On<iOS>().SetUseSafeArea(true);
        }
        protected void NokeyBoard()
        {
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("MyCustomization", (handler, view) =>
            {
                if (Config.TypeScaner != eTypeScaner.Camera) 
                {
                    if (view is CustomEntry)
                    {
#if ANDROID
                    handler.PlatformView.ShowSoftInputOnFocus = false;    
#endif
                    }
                }                    
            });
        }
    }
}