using System;
using Microsoft.Maui.Controls;

namespace PriceChecker.View;

public partial class NumericKeyboard : ContentView
{
    public NumericKeyboard()
    {
        InitializeComponent();
    }

    // Bindable property to connect keyboard to Entry.Text (two-way)
    public static readonly BindableProperty TargetTextProperty =
        BindableProperty.Create(
            nameof(TargetText),
            typeof(string),
            typeof(NumericKeyboard),
            string.Empty,
            BindingMode.TwoWay);

    public string TargetText
    {
        get => (string)GetValue(TargetTextProperty);
        set => SetValue(TargetTextProperty, value);
    }

    // Optional event when OK pressed
    public event EventHandler OkPressed;

    void Digit_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && !string.IsNullOrEmpty(b.Text))
        {
            // append digit
            TargetText = (TargetText ?? string.Empty) + b.Text;
        }
    }

    void Backspace_Clicked(object sender, EventArgs e)
    {
        if (!string.IsNullOrEmpty(TargetText))
        {
            TargetText = TargetText.Substring(0, Math.Max(0, TargetText.Length - 1));
        }
    }
    private void Clear_Clicked(object sender, EventArgs e)
    {
        TargetText = string.Empty;
    }

    void Ok_Clicked(object sender, EventArgs e)
    {
        OkPressed?.Invoke(this, EventArgs.Empty);
        // Optionally hide keyboard by controlling IsVisible externally
    }

}