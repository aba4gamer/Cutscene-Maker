using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;

namespace CutsceneMaker;

public partial class Wipe : UserControl
{
    /// <summary>
    /// ONLY USED FOR THE DESIGNER. DON'T USE!
    /// </summary>
    public Wipe()
    {
        InitializeComponent();
    }
    public Wipe(ICommonEntries part)
    {
        InitializeComponent();

        if (part.WipeEntry != null)
        {
            IsWipeEnabled.IsChecked = true;
            WipeName.Text = part.WipeEntry.WipeName;
            WipeType.Value = part.WipeEntry.WipeType;
            WipeFrame.Value = part.WipeEntry.WipeFrame;

            WipeName.GetObservable(TextBox.TextProperty)
                .Subscribe(Observer.Create<string?>(text => part.WipeEntry.WipeName = text ?? string.Empty));

            WipeType.GetObservable(NumericUpDown.ValueProperty)
                .Subscribe(Observer.Create<decimal?>(value => part.WipeEntry.WipeType = value.HasValue ? (int)value.Value : -1));

            WipeFrame.GetObservable(NumericUpDown.ValueProperty)
                .Subscribe(Observer.Create<decimal?>(value => part.WipeEntry.WipeFrame = value.HasValue ? (int)value.Value : -1));
        }

        IsWipeEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(Observer.Create<bool?>(isChecked =>
        {
            if (isChecked == true)
            {
                part.WipeEntry ??= new Abacus.Wipe();
                SetControlsEnabled(true);

                part.WipeEntry.WipeName = WipeName.Text ?? string.Empty;
                part.WipeEntry.WipeType = WipeType.Value.HasValue ? (int)WipeType.Value.Value : -1;
                part.WipeEntry.WipeFrame = WipeFrame.Value.HasValue ? (int)WipeFrame.Value.Value : -1;
            }
            else
            {
                part.WipeEntry = null;
                SetControlsEnabled(false);
            }
        }));
    }
    private void SetControlsEnabled(bool isEnabled)
    {
        WipeName.IsEnabled = isEnabled;
        WipeType.IsEnabled = isEnabled;
        WipeFrame.IsEnabled = isEnabled;
    }
}
