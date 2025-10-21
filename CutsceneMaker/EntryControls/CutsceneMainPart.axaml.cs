using System;
using System.Reactive;

using Abacus;

using Avalonia;
using Avalonia.Input;
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
				if (value != part.TimeEntry.TotalStep)
					MainWindow.Instance!.AddEditedCutscene();

				if (TotalStep.Value != null)
				{
					int step = (int) TotalStep.Value;

					part.TimeEntry.TotalStep = step;
					MainWindow.Instance!.Part_UpdateStep(step);
				}
			});

		_suspendFlagSubscription = SuspendFlag.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked =>
			{
				if (isChecked != (part.TimeEntry.SuspendFlag != 0))
					MainWindow.Instance!.AddEditedCutscene();

				part.TimeEntry.SuspendFlag = isChecked == true ? 1 : 0;
			});

		_waitUserInputFlagSubscription = WaitUserInputFlag.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked =>
			{
				if (isChecked != (part.TimeEntry.WaitUserInputFlag != 0))
					MainWindow.Instance!.AddEditedCutscene();

				part.TimeEntry.WaitUserInputFlag = isChecked == true ? 1 : 0;
			});
	}

	private void DisposeSubscriptions()
	{
		_totalStepSubscription?.Dispose();
		_suspendFlagSubscription?.Dispose();
		_waitUserInputFlagSubscription?.Dispose();
	}

	private void OnPartNameChange(object? sender, RoutedEventArgs e)
	{
		if (Part == null || PartName.Text == null)
			return;

		Part.PartName = PartName.Text;
		MainWindow.Instance!.Part_UpdateName(PartName.Text);
	}

	private void PartName_OnKeyDown(object? sender, RoutedEventArgs e)
	{
		if (Part == null || PartName.Text == null)
			return;

		switch (((KeyEventArgs) e).Key)
		{
			case Key.Enter:
				PartName.IsEnabled = false;
				PartName.IsEnabled = true;
				MainWindow.Instance!.Part_UpdateName(PartName.Text);
				Part.PartName = PartName.Text;
				break;
		}
	}
}
