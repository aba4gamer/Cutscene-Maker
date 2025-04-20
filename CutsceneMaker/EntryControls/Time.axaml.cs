using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;

namespace CutsceneMaker;

public partial class Time : UserControl
{
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

        TotalStep.GetObservable(NumericUpDown.ValueProperty)
            .Subscribe(Observer.Create<decimal?>(value => part.TimeEntry.TotalStep = value.HasValue ? (int)value.Value : 0));
        SuspendFlag.GetObservable(CheckBox.IsCheckedProperty)
            .Subscribe(Observer.Create<bool?>(isChecked => part.TimeEntry.SuspendFlag = isChecked == true ? 1 : 0));
        WaitUserInputFlag.GetObservable(CheckBox.IsCheckedProperty)
            .Subscribe(Observer.Create<bool?>(isChecked => part.TimeEntry.WaitUserInputFlag = isChecked == true ? 1 : 0));
    }
}
