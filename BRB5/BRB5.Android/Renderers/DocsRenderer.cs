using Android.Content;
using Android.Views;
using Xamarin.Forms.Platform.Android;
using BRB5.Droid.Renderers;
using Xamarin.Forms;
using BRB5.View;

[assembly: ExportRenderer(typeof(Docs), typeof(DocsRenderer))]
namespace BRB5.Droid.Renderers
{
    public class DocsRenderer : PageRenderer
    {
        public DocsRenderer(Context context) : base(context) { }

        public override bool DispatchKeyEvent(KeyEvent e)
        {
            if (e.Action == KeyEventActions.Down)
            {
                if (e.KeyCode == Keycode.F1)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "F1Pressed" }, "F1Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.Num2)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "2Pressed" }, "2Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.Num8)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "8Pressed" }, "8Pressed");
                    return true;
                }
                else if (e.KeyCode == Keycode.Enter)
                {
                    MessagingCenter.Send(new KeyEventMessage { Key = "EnterPressed" }, "EnterPressed");
                    return true;
                }
            }

            return base.DispatchKeyEvent(e);
        }


    }
}
