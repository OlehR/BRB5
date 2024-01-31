using Android.Content;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using BRB5.Droid.Renderers;
using Xamarin.Forms;
using BRB5.View;

[assembly: ExportRenderer(typeof(BRB5.PriceCheck), typeof(PriceCheckRenderer))]
namespace BRB5.Droid.Renderers
{
    public class PriceCheckRenderer : PageRenderer
    {
        public PriceCheckRenderer(Context context) : base(context) { }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                if (e.KeyCode == Keycode.F1)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F1Pressed" }, "F1Pressed");
                    return true; // Event handled
                }
                else if (e.KeyCode == Keycode.F2)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F2Pressed" }, "F2Pressed");
                    return true; // Event handled
                }
                else if (e.KeyCode == Keycode.F5)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F5Pressed" }, "F5Pressed");
                    return true; // Event handled
                }
                else if (e.KeyCode == Keycode.F6)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F6Pressed" }, "F6Pressed");
                    return true; // Event handled
                }
            }

            return base.DispatchKeyEvent(e);
        }


    }
}
