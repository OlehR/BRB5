using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace BRB5.ViewModel
{
    public static class DocWaresEx
    {    
       static public Keyboard Keyboard(this BRB5.Model.DocWaresEx DWE) { 
            return DWE.CodeUnit == 7 ? Xamarin.Forms.Keyboard.Telephone : Xamarin.Forms.Keyboard.Numeric;  }
    }
    
}
