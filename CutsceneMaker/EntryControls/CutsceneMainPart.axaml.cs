using System;
using System.Reactive;

using Abacus;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class CutsceneMainPart : UserControl
{
	private IDisposable? _totalStepSubscription;
	private IDisposable? _suspendFlagSubscription;
	private IDisposable? _waitUserInputFlagSubscription;
	private Cutscene.Part? Part;

	public Action<int> TotalStepChange = (int steps) => {};
	public Action<string> PartNameChange = (string name) => {};

	public CutsceneMainPart()
	{
		InitializeComponent();
	}

	public CutsceneMainPart(Cutscene.Part part)
	{
		InitializeComponent();

		Part = part;
		PartName.Text = part.PartName;
		TotalStep.Value = part.TimeEntry.TotalStep;
		SuspendFlag.IsChecked = part.TimeEntry.SuspendFlag != 0;
		WaitUserInputFlag.IsChecked = part.TimeEntry.WaitUserInputFlag != 0;

		SubscribeToChanges(part);
	}

	private void SubscribeToChanges(Cutscene.Part part)
	{
		DisposeSubscriptions();

		_totalStepSubscription = TotalStep.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (TotalStep.Value != null)
				{
					int step = (int) TotalStep.Value;

					part.TimeEntry.TotalStep = step;
					TotalStepChange(step);
				}
			});

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

	private void OnPartNameChange(object? sender, RoutedEventArgs e)
	{
		Part.PartName = PartName.Text;
		PartNameChange(PartName.Text);
	}
}
