using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;

namespace CutsceneMaker;

public partial class SubPart : UserControl
{
    public SubPart()
    {
        InitializeComponent();
    }

    public SubPart(Abacus.SubPart part)
    {
        InitializeComponent();
        SubPartTotalStep.Value = part.SubPartTotalStep; 
        MainPartStep.Value = part.MainPartStep;

        SubPartTotalStep.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(Observer.Create<decimal?>(value => part.SubPartTotalStep = (int)value!.Value));
        MainPartStep.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(Observer.Create<decimal?>(value => part.MainPartStep = (int)value!.Value));
    }
}
