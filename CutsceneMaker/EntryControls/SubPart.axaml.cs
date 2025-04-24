using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;

namespace CutsceneMaker;

public partial class SubPart : UserControl
{
    private IDisposable? _subPartTotalStepSubscription;
    private IDisposable? _mainPartStepSubscription;

    public SubPart()
    {
        InitializeComponent();
    }

    public SubPart(Abacus.SubPart part)
    {
        InitializeComponent();

        SubPartTotalStep.Value = part.SubPartTotalStep;
        MainPartStep.Value = part.MainPartStep;

        SubscribeToChanges(part);
    }

    private void SubscribeToChanges(Abacus.SubPart part)
    {
        DisposeSubscriptions();

        _subPartTotalStepSubscription = SubPartTotalStep.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.SubPartTotalStep = (int)value!.Value);

        _mainPartStepSubscription = MainPartStep.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(value => part.MainPartStep = (int)value!.Value);
    }

    private void DisposeSubscriptions()
    {
        _subPartTotalStepSubscription?.Dispose();
        _mainPartStepSubscription?.Dispose();
    }
}
