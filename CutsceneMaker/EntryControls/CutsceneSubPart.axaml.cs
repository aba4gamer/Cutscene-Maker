using System;
using System.Reactive;

using Abacus;

using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Interactivity;




namespace CutsceneMakerUI;

public partial class CutsceneSubPart : UserControl
{
	private IDisposable? _subPartTotalStepSubscription;
	private IDisposable? _mainPartStepSubscription;
	private SubPart? Part;


	public CutsceneSubPart()
	{
		InitializeComponent();
	}

	public CutsceneSubPart(Abacus.SubPart part)
	{
		InitializeComponent();

		Part = part;
		SubPartName.Text = part.SubPartName;
		SubPartTotalStep.Value = part.SubPartTotalStep;
		MainPartStep.Value = part.MainPartStep;

		SubscribeToChanges(part);
	}

	private void SubscribeToChanges(Abacus.SubPart part)
	{
		DisposeSubscriptions();

		_subPartTotalStepSubscription = SubPartTotalStep.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.SubPartTotalStep)
					MainWindow.Instance!.AddEditedCutscene();

				if (SubPartTotalStep.Value != null)
				{
					int step = (int) SubPartTotalStep.Value;

					part.SubPartTotalStep = step;
					MainWindow.Instance!.SubPart_UpdateStep(step);
				}
			});

		_mainPartStepSubscription = MainPartStep.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.MainPartStep)
					MainWindow.Instance!.AddEditedCutscene();

				part.MainPartStep = (int)value!.Value;
			});
	}

	private void DisposeSubscriptions()
	{
		_subPartTotalStepSubscription?.Dispose();
		_mainPartStepSubscription?.Dispose();
	}

	private void OnSubPartNameChange(object? sender, RoutedEventArgs e)
	{
		if (Part == null || SubPartName.Text == null)
			return;

		Part.SubPartName = SubPartName.Text;
	}

	private void PartName_OnKeyDown(object? sender, RoutedEventArgs e)
	{
		if (Part == null || SubPartName.Text == null)
			return;

		switch (((KeyEventArgs) e).Key)
		{
			case Key.Enter:
				SubPartName.IsEnabled = false;
				SubPartName.IsEnabled = true;
				MainWindow.Instance!.SubPart_UpdateName(SubPartName.Text);
				Part.SubPartName = SubPartName.Text;
				break;
		}
	}
}
