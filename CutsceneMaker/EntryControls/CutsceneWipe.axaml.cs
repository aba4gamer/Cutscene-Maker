using System;
using System.Reactive;
using System.Collections.Generic;

using Abacus;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;




namespace CutsceneMakerUI;

public partial class CutsceneWipe : UserControl
{
	private IDisposable? _nameSubscription;
	private IDisposable? _nameFocusSubscription;
	private IDisposable? _typeSubscription;
	private IDisposable? _frameSubscription;

	public static BidirectionalDictionary<string, string> WipeTypes = new()
	{
		["Circle Wipe"] = "円ワイプ",
		["Black Fade"] = "フェードワイプ",
		["White Fade"] = "白フェードワイプ",
		["Game Over"] = "ゲームオーバー",
		["Bowser Face"] = "クッパ"
	};

	public static List<string> WipeTypesList = ["Circle Wipe", "Black Fade", "White Fade", "Game Over", "Bowser Face"];


	public CutsceneWipe()
	{
		InitializeComponent();
	}

	public CutsceneWipe(ICommonEntries part)
	{
		InitializeComponent();

		WipeName.AutoCompletion = WipeTypesList;
		if (part.WipeEntry != null)
		{
			IsWipeEnabled.IsChecked = true;
			WipeName.Main.Text = WipeTypes.ContainsValue(part.WipeEntry.WipeName) ? WipeTypes.Inverse[part.WipeEntry.WipeName] : part.WipeEntry.WipeName;
			WipeType.SelectedIndex = part.WipeEntry.WipeType;
			WipeFrame.Value = part.WipeEntry.WipeFrame;

			SubscribeToChanges(part);
		}
		IsWipeEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(Observer.Create<bool?>(isChecked =>
		{
			if (isChecked != (part.WipeEntry != null))
				MainWindow.Instance!.AddEditedCutscene();

			if (isChecked == true)
			{
				part.WipeEntry ??= new Abacus.Wipe();
				SetControlsEnabled(true);

				part.WipeEntry.WipeName = WipeName.Main.Text != null ? WipeTypes.ContainsKey(WipeName.Main.Text) ? WipeTypes[WipeName.Main.Text] : WipeName.Main.Text : "";
				part.WipeEntry.WipeType = WipeType.SelectedIndex;
				part.WipeEntry.WipeFrame = WipeFrame.Value.HasValue ? (int)WipeFrame.Value.Value : -1;

				SubscribeToChanges(part);
			}
			else
			{
				part.WipeEntry = null;
				SetControlsEnabled(false);

				DisposeSubscriptions();
			}
		}));
	}

	private void SetControlsEnabled(bool isEnabled)
	{
		WipeName.IsEnabled = isEnabled;
		WipeType.IsEnabled = isEnabled;
		WipeFrame.IsEnabled = isEnabled;
	}

	private void SubscribeToChanges(ICommonEntries part)
	{
		DisposeSubscriptions();
		_nameSubscription = WipeName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != part.WipeEntry!.WipeName)
					MainWindow.Instance!.AddEditedCutscene();

				part.WipeEntry!.WipeName = text != null ? WipeTypes.ContainsKey(text) ? WipeTypes[text] : text : "";
			});

		_nameFocusSubscription = WipeName.Main.GetObservable(TextBox.IsFocusedProperty)
			.Subscribe(isFocused => {
				if (isFocused)
					WipeName.ShowFlyout();
			});

		_typeSubscription = WipeType.GetObservable(ComboBox.SelectedIndexProperty)
			.Subscribe(value =>
			{
				if (value != part.WipeEntry!.WipeType)
					MainWindow.Instance!.AddEditedCutscene();

				part.WipeEntry!.WipeType = value;
			});

		_frameSubscription = WipeFrame.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.WipeEntry!.WipeFrame)
					MainWindow.Instance!.AddEditedCutscene();

				part.WipeEntry!.WipeFrame = value.HasValue ? (int)value.Value : -1;
			});
	}

	private void DisposeSubscriptions()
	{
		_nameSubscription?.Dispose();
		_nameFocusSubscription?.Dispose();
		_typeSubscription?.Dispose();
		_frameSubscription?.Dispose();
	}
}
