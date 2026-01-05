using BL;
using BL.Connector;
using BRB5;
using BRB5.Model;
using BRB5.Model.DB;
using CommunityToolkit.Maui.Core;
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Alerts;

namespace PriceChecker.View;

public partial class PrintPage : ContentPage
{

    DB db = DB.GetDB();
    BL.BL bl = BL.BL.GetBL();
    private TypeDoc TypeDoc;
    public bool IsEnabledPrint { get { return Config.CodeWarehouse != 0; } }

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
        BindingContext = this;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();

        var route = Shell.Current?.CurrentItem?.Route;
        if (Config.TypeDoc == null)
            return;

        switch (route)
        {
            case "Print1":
                TypeDoc = Config.TypeDoc.FirstOrDefault(t => t.CodeDoc == 51);
                break;

            case "Print2":
                TypeDoc = Config.TypeDoc.FirstOrDefault(t => t.CodeDoc == 52);
                break;
        }

        Task.Run(async () =>
        {
            var r = await bl.c.LoadDocsDataAsync(TypeDoc.CodeDoc, null, false);
            if (TypeDoc != null)
            {
                // Оновлення на головному потоці
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    DocsToPrint = new ObservableCollection<DocVM>(db.GetDoc(TypeDoc));
                });
            }
        });
    }

    private void OnSwipedRight(object sender, SwipedEventArgs e)
    {
        if (e.Direction == SwipeDirection.Right)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }

    private async void OnPrintTapped(object sender, TappedEventArgs e)
    {
        if (sender is Label label && label.BindingContext is DocVM doc)
        {
            var r = db.GetDocWares(doc, 1, eTypeOrder.Scan);
            var codes = r.Select(x => x.CodeWares);

            if (IsEnabledPrint)
            {
                var result = bl.c.PrintHTTP(codes);

                if (result.StartsWith("Print=>"))
                {
                    doc.State = 1; // позначаємо, що документ надрукований

                    // 🔹 шукаємо Grid (батько Label 🖨️)
                    if (label.Parent is Grid grid)
                    {
                        // шукаємо "✔" у колонці 1
                        foreach (var child in grid.Children)
                        {
                            if (child is Label checkLabel && checkLabel.Text == "✔")
                            {
                                checkLabel.IsVisible = true;
                            }
                        }
                    }
                }

                await DisplayAlert("Друк", result, "OK");
            }
        }
    }

    private void OnDocTapped(object sender, TappedEventArgs e)
    {
        if (sender is Grid grid && grid.BindingContext is DocVM doc)
        {
            Navigation.PushAsync(new PrintWares(doc, TypeDoc));
        }
    }
}