using Android.Content;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using BRB5.Droid.Renderers;
using Xamarin.Forms;
using BRB5.View;
using System.Runtime.Remoting.Lifetime;

[assembly: ExportRenderer(typeof(DocScan), typeof(DocScanRenderer))]
namespace BRB5.Droid.Renderers
{
    public class DocScanRenderer : PageRenderer
    {
        public DocScanRenderer(Context context) : base(context) { }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                if (e.KeyCode == Keycode.F1)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F1Pressed" }, "F1Pressed");
                    return true;
                } 
                else if (e.KeyCode == Keycode.F2)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F2Pressed" }, "F2Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.F3)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F3Pressed" }, "F3Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.F8)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F8Pressed" }, "F8Pressed");
                    return true;
                }
            }

            return base.DispatchKeyEvent(e);
        }


    }
}
