using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace BRB5.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Settings : TabbedPage, INotifyPropertyChanged
    {
        public string Ver { get { return "Ver:"+ Assembly.GetExecutingAssembly().GetName().Version; } }

        public string SN { get { return "SN:"; } }
        public Settings()
        {
            InitializeComponent();

            this.BindingContext = this;
        }

        private void OnClickLoad(object sender, EventArgs e)
        {

        }

        private void OnClickLoadDoc(object sender, EventArgs e)
        {

        }

        private void OnTestCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

        }

        private void OnAutoCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

        }

        private void OnCopyDB(object sender, EventArgs e)
        {

        }

        private void OnRestoreDB(object sender, EventArgs e)
        {

        }

        private void OnClickGen(object sender, EventArgs e)
        {

        }

        private void OnClickIP(object sender, EventArgs e)
        {

        }

        private void OnVibrationCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

        }

        private void OnSoundCheckedChanged(object sender, CheckedChangedEventArgs e)
        {

        }
    }
}