using System;
using System.Reactive;

using Abacus;

using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class CutsceneMainPart : UserControl
{
	private IDisposable? _totalStepSubscription;
	private IDisposable? _partNameSubscription;
	private IDisposable? _partNameFocusSubscription;
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

		_partNameSubscription = PartName.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text == null || part.PartName == text)
				{
					PartNameWarn.Foreground = Brush.Parse("#0000");
					return;
				}

				text = text.Trim();
				if (text == "")
				{
					PartNameWarn.Text = "Name can't be empty!";
					PartNameWarn.Foreground = Brush.Parse("#FA8072");
				}
				else if (MainWindow.Instance!.Core.GetAllPartNames().Contains(text))
				{
					PartNameWarn.Text = "This name is already used!";
					PartNameWarn.Foreground = Brush.Parse("#FA8072");
				}
				else
				{
					PartNameWarn.Foreground = Brush.Parse("#0000");
					MainWindow.Instance!.Part_UpdateName(text);
					part.PartName = text;
				}
			});

		_partNameFocusSubscription = PartName.GetObservable(TextBox.IsFocusedProperty)
			.Subscribe(isFocused =>
			{
				if (!isFocused)
					PartName.Text = part.PartName;
			});

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
		_partNameSubscription?.Dispose();
		_partNameFocusSubscription?.Dispose();
		_suspendFlagSubscription?.Dispose();
		_waitUserInputFlagSubscription?.Dispose();
	}
}
