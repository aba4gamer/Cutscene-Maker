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

public partial class CutsceneSubPart : UserControl
{
	private IDisposable? _subPartNameSubscription;
	private IDisposable? _subPartNameFocusSubscription;
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

		_subPartNameSubscription = SubPartName.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text == null || part.SubPartName == text)
				{
					SubPartNameWarn.Foreground = Brush.Parse("#0000");
					return;
				}

				text = text.Trim();
				if (text == "")
				{
					SubPartNameWarn.Text = "Name can't be empty!";
					SubPartNameWarn.Foreground = Brush.Parse("#FA8072");
				}
				else if (MainWindow.Instance!.Core.GetAllPartNames().Contains(text))
				{
					SubPartNameWarn.Text = "This name is already used!";
					SubPartNameWarn.Foreground = Brush.Parse("#FA8072");
				}
				else
				{
					SubPartNameWarn.Foreground = Brush.Parse("#0000");
					MainWindow.Instance!.SubPart_UpdateName(text);
					part.SubPartName = text;
				}
			});

		_subPartNameFocusSubscription = SubPartName.GetObservable(TextBox.IsFocusedProperty)
			.Subscribe(isFocused =>
			{
				if (!isFocused)
					SubPartName.Text = part.SubPartName;
			});

		_subPartTotalStepSubscription = SubPartTotalStep.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.SubPartTotalStep)
					MainWindow.Instance!.AddEditedCutscene();

				if (value != null)
				{
					part.SubPartTotalStep = (int)value.Value;
					MainWindow.Instance!.SubPart_UpdateStep((int)value.Value);

				}
			});

		_mainPartStepSubscription = MainPartStep.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.MainPartStep)
					MainWindow.Instance!.AddEditedCutscene();

				if (value != null)
				{
					part.MainPartStep = (int)value.Value;
					MainWindow.Instance!.SubPart_UpdateSpaceSteps((int)value.Value);
				}
			});
	}

	private void DisposeSubscriptions()
	{
		_subPartNameSubscription?.Dispose();
		_subPartNameFocusSubscription?.Dispose();
		_subPartTotalStepSubscription?.Dispose();
		_mainPartStepSubscription?.Dispose();
	}
}
