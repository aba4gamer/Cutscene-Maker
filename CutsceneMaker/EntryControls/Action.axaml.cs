using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System;

namespace CutsceneMaker;

public partial class Action : UserControl
{
    private IDisposable? _castNameSubscription;
    private IDisposable? _castIDSubscription;
    private IDisposable? _actionTypeSubscription;
    private IDisposable? _posNameSubscription;
    private IDisposable? _animNameSubscription;

    public Action()
    {
        InitializeComponent();
    }

    public Action(ICommonEntries part)
    {
        InitializeComponent();

        if (part.ActionEntry != null)
        {
            IsActionEnabled.IsChecked = true;
            CastName.Text = part.ActionEntry.CastName;
            CastID.Value = part.ActionEntry.CastID;
            ActionType.Value = part.ActionEntry.ActionType;
            PosName.Text = part.ActionEntry.PosName;
            AnimName.Text = part.ActionEntry.AnimName;

            SubscribeToChanges(part);
        }

        IsActionEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
        {
            if (isChecked == true)
            {
                part.ActionEntry ??= new Abacus.Action();
                SetControlsEnabled(true);

                part.ActionEntry.CastName = CastName.Text ?? string.Empty;
                part.ActionEntry.CastID = CastID.Value.HasValue ? (int)CastID.Value.Value : -1;
                part.ActionEntry.ActionType = ActionType.Value.HasValue ? (int)ActionType.Value.Value : -1;
                part.ActionEntry.PosName = PosName.Text ?? string.Empty;
                part.ActionEntry.AnimName = AnimName.Text ?? string.Empty;

                SubscribeToChanges(part);
            }
            else
            {
                part.ActionEntry = null;
                SetControlsEnabled(false);

                DisposeSubscriptions();
            }
        });
    }

    private void SetControlsEnabled(bool isEnabled)
    {
        CastName.IsEnabled = isEnabled;
        CastID.IsEnabled = isEnabled;
        ActionType.IsEnabled = isEnabled;
        PosName.IsEnabled = isEnabled;
        AnimName.IsEnabled = isEnabled;
    }

    private void SubscribeToChanges(ICommonEntries part)
    {
        DisposeSubscriptions();

        _castNameSubscription = CastName.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.ActionEntry!.CastName = text ?? string.Empty);

        _castIDSubscription = CastID.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.ActionEntry!.CastID = value.HasValue ? (int)value.Value : -1);

        _actionTypeSubscription = ActionType.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.ActionEntry!.ActionType = value.HasValue ? (int)value.Value : -1);

        _posNameSubscription = PosName.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.ActionEntry!.PosName = text ?? string.Empty);

        _animNameSubscription = AnimName.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.ActionEntry!.AnimName = text ?? string.Empty);
    }

    private void DisposeSubscriptions()
    {
        _castNameSubscription?.Dispose();
        _castIDSubscription?.Dispose();
        _actionTypeSubscription?.Dispose();
        _posNameSubscription?.Dispose();
        _animNameSubscription?.Dispose();
    }
}
