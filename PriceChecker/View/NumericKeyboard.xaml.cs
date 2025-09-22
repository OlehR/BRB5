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

    public event EventHandler? UserInteracted;
    void Digit_Clicked(object sender, EventArgs e)
    {
        if (sender is Button b && !string.IsNullOrEmpty(b.Text))
        {
            // append digit
            TargetText = (TargetText ?? string.Empty) + b.Text;

            // raise event so the parent page knows interaction happened
            UserInteracted?.Invoke(this, EventArgs.Empty);
        }
    }

    DateTime _backspacePressStart;
    private void Backspace_Pressed(object sender, EventArgs e) { _backspacePressStart = DateTime.UtcNow; }
    private void Backspace_Released(object sender, EventArgs e)
    {
        var duration = DateTime.UtcNow - _backspacePressStart;

        if (duration.TotalMilliseconds >= 1500) TargetText = string.Empty;
        else if (!string.IsNullOrEmpty(TargetText))
                TargetText = TargetText.Substring(0, TargetText.Length - 1);
        
    }

    void Ok_Clicked(object sender, EventArgs e)  { OkPressed?.Invoke(this, EventArgs.Empty); }

}