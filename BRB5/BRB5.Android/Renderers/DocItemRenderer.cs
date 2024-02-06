using Android.Content;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using BRB5.Droid.Renderers;
using Xamarin.Forms;
using BRB5.View;
using Android.Runtime;

[assembly: ExportRenderer(typeof(DocItem), typeof(DocItemRenderer))]
namespace BRB5.Droid.Renderers
{
    public class DocItemRenderer : PageRenderer
    {
        public DocItemRenderer(Context context) : base(context) {
        }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                if (e.KeyCode == Keycode.F2)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F2Pressed" }, "F2Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.F3)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F3Pressed" }, "F3Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.F4)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F4Pressed" }, "F4Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.F6)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F6Pressed" }, "F6Pressed");
                    return true;
                }
            }

            return base.DispatchKeyEvent(e);
        }
        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            // Handle key down event
            HandleKeyEvent(keyCode);
            return base.OnKeyDown(keyCode, e);
        }
        public override bool OnKeyUp([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            // Handle key up event
            HandleKeyEvent(keyCode);
            return base.OnKeyUp(keyCode, e);
        }
        private void HandleKeyEvent(Keycode keyCode)
        {
            switch (keyCode)
            {
                case Keycode.F1:
                    // Handle F1 key press for DocScan page
                    // Add your code here
                    break;

                case Keycode.F2:
                    // Handle F2 key press for DocScan page
                    // Add your code here
                    break;

                // Add cases for other keys as needed

                default:
                    break;
            }
        }

    }
}
