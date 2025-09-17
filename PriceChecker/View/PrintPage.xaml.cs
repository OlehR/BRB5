using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using System;
using System.Collections.ObjectModel;

namespace PriceChecker.View;

public partial class PrintPage : ContentPage
{

    DB db = DB.GetDB();
    Connector c;
    private TypeDoc TypeDoc;

    private ObservableCollection<DocVM> _docsToPrint = new ObservableCollection<DocVM>();
    public ObservableCollection<DocVM> DocsToPrint
    {
        get => _docsToPrint;
        set
        {
            if (_docsToPrint != value)
            {
                _docsToPrint = value;
                OnPropertyChanged(nameof(DocsToPrint));
            }
        }
    }

    public PrintPage()
    {
        InitializeComponent();
        c = ConnectorBase.GetInstance();
        TypeDoc = new TypeDoc() { Group = eGroup.Doc, CodeDoc = 51, NameDoc = "Друк", KindDoc = eKindDoc.Normal };

        DocsToPrint = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));

        Task.Run(async () =>
        {
            var r = await c.LoadDocsDataAsync(51, null, false);

            // Оновлення на головному потоці
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DocsToPrint = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));
            });
        });

        BindingContext = this;
    }

    private void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }

    private void OnPrintTapped(object sender, TappedEventArgs e)
    {

    }
}