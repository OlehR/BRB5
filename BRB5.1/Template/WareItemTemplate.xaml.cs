﻿using BRB6.View;

namespace BRB6.Template;

public partial class WareItemTemplate : ContentView
{
    public WareItemTemplate()
    {
        InitializeComponent();
    }

    public void BindData(object bindingContext)
    {
        BindingContext = bindingContext;

        //// Прив’язка даних до Label 
        //NameWaresLabel.SetBinding(Label.TextProperty, new Binding("NameWares"));
        //CodeWaresLabel.SetBinding(Label.TextProperty, new Binding("CodeWares"));
        //QuantityLabel.SetBinding(Label.TextProperty, new Binding("Quantity"));
        //QuantityInputLabel.SetBinding(Label.TextProperty, new Binding("QuantityInput"));
        //ExpirationDateInputLabel.SetBinding(Label.TextProperty, new Binding("ExpirationDateInput", stringFormat: "{0:dd.MM.yyyy}"));
        //ExpirationDateLabel.SetBinding(Label.TextProperty, new Binding("ExpirationDate", stringFormat: "{0:dd.MM.yyyy}"));


        //// Прив’язка кольору (новий Binding для кожної властивості)
        //NameWaresLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));
        //CodeWaresLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));
        //QuantityLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter())); 
        //QuantityInputLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));
        //ExpirationDateInputLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));
        //ExpirationDateLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));


        NameWaresLabel.SetBinding(Label.TextProperty, new Binding("NameWares"));
        QuantityLabel.SetBinding(Label.TextProperty, new Binding("DisplayedQuantity"));
        ExpirationDateLabel.SetBinding(Label.TextProperty, new Binding("DisplayedExpirationDate", stringFormat: "{0:dd.MM.yy}"));

        NameWaresLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetNameWareColor", converter: new ColorConverter()));
        QuantityLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));
        ExpirationDateLabel.SetBinding(Label.BackgroundColorProperty, new Binding("GetColor", converter: new ColorConverter()));

    }
}

