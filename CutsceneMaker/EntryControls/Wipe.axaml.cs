using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;

namespace CutsceneMaker;

public partial class Wipe : UserControl
{
    private IDisposable? _nameSubscription;
    private IDisposable? _typeSubscription;
    private IDisposable? _frameSubscription;

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

            SubscribeToChanges(part);
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

                SubscribeToChanges(part);
            }
            else
            {
                part.WipeEntry = null;
                SetControlsEnabled(false);

                DisposeSubscriptions();
            }
        }));
    }

    private void SetControlsEnabled(bool isEnabled)
    {
        WipeName.IsEnabled = isEnabled;
        WipeType.IsEnabled = isEnabled;
        WipeFrame.IsEnabled = isEnabled;
    }

    private void SubscribeToChanges(ICommonEntries part)
    {
        DisposeSubscriptions();
        _nameSubscription = WipeName.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.WipeEntry!.WipeName = text ?? string.Empty);

        _typeSubscription = WipeType.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.WipeEntry!.WipeType = value.HasValue ? (int)value.Value : -1);

        _frameSubscription = WipeFrame.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.WipeEntry!.WipeFrame = value.HasValue ? (int)value.Value : -1);
    }

    private void DisposeSubscriptions()
    {
        _nameSubscription?.Dispose();
        _typeSubscription?.Dispose();
        _frameSubscription?.Dispose();
    }
}
