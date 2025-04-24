using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;

namespace CutsceneMaker;

public partial class Time : UserControl
{
    private IDisposable? _totalStepSubscription;
    private IDisposable? _suspendFlagSubscription;
    private IDisposable? _waitUserInputFlagSubscription;

    public Time()
    {
        InitializeComponent();
    }

    public Time(Cutscene.Part part)
    {
        InitializeComponent();

        TotalStep.Value = part.TimeEntry.TotalStep;
        SuspendFlag.IsChecked = part.TimeEntry.SuspendFlag != 0;
        WaitUserInputFlag.IsChecked = part.TimeEntry.WaitUserInputFlag != 0;

        SubscribeToChanges(part);
    }

    private void SubscribeToChanges(Cutscene.Part part)
    {
        DisposeSubscriptions();

        _totalStepSubscription = TotalStep.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.TimeEntry.TotalStep = value.HasValue ? (int)value.Value : 0);

        _suspendFlagSubscription = SuspendFlag.GetObservable(CheckBox.IsCheckedProperty)
            .Subscribe(isChecked => part.TimeEntry.SuspendFlag = isChecked == true ? 1 : 0);

        _waitUserInputFlagSubscription = WaitUserInputFlag.GetObservable(CheckBox.IsCheckedProperty)
            .Subscribe(isChecked => part.TimeEntry.WaitUserInputFlag = isChecked == true ? 1 : 0);
    }

    private void DisposeSubscriptions()
    {
        _totalStepSubscription?.Dispose();
        _suspendFlagSubscription?.Dispose();
        _waitUserInputFlagSubscription?.Dispose();
    }
}
