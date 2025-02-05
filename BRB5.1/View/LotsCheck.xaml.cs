using BL.Connector;
using BL;
using BRB5;
using BRB5.Model;
using System.Collections.ObjectModel;

namespace BRB6.View;

public partial class LotsCheck : ContentPage
{
    private Connector c = ConnectorBase.GetInstance();
    private TypeDoc TypeDoc;
    DB db = DB.GetDB();

    private ObservableCollection<DocVM> _MyDocsR;
    public ObservableCollection<DocVM> MyDocsR { get { return _MyDocsR; } set { _MyDocsR = value; OnPropertyChanged(nameof(MyDocsR)); } }
    public LotsCheck()
	{
		InitializeComponent();
	}

    private void OpenDoc(object sender, TappedEventArgs e)
    {

    }
}