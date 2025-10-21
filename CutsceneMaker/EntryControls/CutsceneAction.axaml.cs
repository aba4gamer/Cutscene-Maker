using System;
using System.Linq;

using Abacus;

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

using CutsceneMaker;




namespace CutsceneMakerUI;

public partial class CutsceneAction : UserControl
{
	private IDisposable? _castNameBoxSubscription;
	private IDisposable? _castIDSubscription;
	private IDisposable? _actionTypeSubscription;
	private IDisposable? _posNameSubscription;
	private IDisposable? _animNameSubscription;


	public CutsceneAction()
	{
		InitializeComponent();
	}

	public CutsceneAction(ICommonEntries part)
	{
		InitializeComponent();

		PosName.AutoCompletion = Program.AutoCompletion.GeneralPosList;
		CastNameBox.AutoCompletion = Program.AutoCompletion.ObjDataTableEnglishNames;
		if (part.ActionEntry != null)
		{
			if (Program.AutoCompletion.ObjDataTableList.ContainsValue(part.ActionEntry.CastName) && Program.AutoCompletion.LoadedAnimObject != Program.AutoCompletion.ObjDataTableList.Inverse[part.ActionEntry.CastName])
			{
				Program.AutoCompletion.LoadRarc_ObjectAnim(Program.AutoCompletion.ObjDataTableList.Inverse[part.ActionEntry.CastName]);
				AnimName.AutoCompletion = Program.AutoCompletion.ObjectAnimList;
			}


			IsActionEnabled.IsChecked = true;
			CastNameBox.Main.Text = Program.AutoCompletion.ObjDataTableList.ContainsValue(part.ActionEntry.CastName) ? Program.AutoCompletion.ObjDataTableList.Inverse[part.ActionEntry.CastName] : part.ActionEntry.CastName;
			CastID.Value = part.ActionEntry.CastID;
			ActionType.SelectedIndex = part.ActionEntry.ActionType;
			PosName.Main.Text = part.ActionEntry.PosName;
			AnimName.Main.Text = part.ActionEntry.AnimName;

			SubscribeToChanges(part);
		}

		IsActionEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
		{
			if (isChecked != (part.ActionEntry != null))
				MainWindow.Instance!.AddEditedCutscene();

			if (isChecked == true)
			{
				part.ActionEntry ??= new Abacus.Action();
				SetControlsEnabled(true);

				AnimName.AutoCompletion = [];

				part.ActionEntry.CastName = CastNameBox.Main.Text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(CastNameBox.Main.Text) ? Program.AutoCompletion.ObjDataTableList[CastNameBox.Main.Text] : CastNameBox.Main.Text : "";
				part.ActionEntry.CastID = CastID.Value.HasValue ? (int)CastID.Value.Value : -1;
				part.ActionEntry.ActionType = ActionType.SelectedIndex;
				part.ActionEntry.PosName = PosName.Main.Text ?? string.Empty;
				part.ActionEntry.AnimName = AnimName.Main.Text ?? string.Empty;

				SubscribeToChanges(part);
			}
			else
			{
				part.ActionEntry = null;
				SetControlsEnabled(false);

				DisposeSubscriptions();
			}
		});
	}

	private void SetControlsEnabled(bool isEnabled)
	{
		CastNameBox.IsEnabled = isEnabled;
		CastID.IsEnabled = isEnabled;
		ActionType.IsEnabled = isEnabled;
		PosName.IsEnabled = isEnabled;
		AnimName.IsEnabled = isEnabled;
	}

	private void SubscribeToChanges(ICommonEntries part)
	{
		DisposeSubscriptions();

		_castNameBoxSubscription = CastNameBox.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if ((text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(text) ? Program.AutoCompletion.ObjDataTableList[text] : text : "") != part.ActionEntry!.CastName)
					MainWindow.Instance!.AddEditedCutscene();

				Program.AutoCompletion.LoadRarc_ObjectAnim(text);
				AnimName.AutoCompletion = Program.AutoCompletion.ObjectAnimList;

				part.ActionEntry!.CastName = text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(text) ? Program.AutoCompletion.ObjDataTableList[text] : text : "";
			});

		_castIDSubscription = CastID.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.ActionEntry!.CastID)
					MainWindow.Instance!.AddEditedCutscene();

				part.ActionEntry!.CastID = value.HasValue ? (int)value.Value : -1;
			});

		_actionTypeSubscription = ActionType.GetObservable(ComboBox.SelectedIndexProperty)
			.Subscribe(value =>
			{
				if (value != part.ActionEntry!.ActionType)
					MainWindow.Instance!.AddEditedCutscene();

				part.ActionEntry!.ActionType = ActionType.SelectedIndex;
			});

		_posNameSubscription = PosName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != part.ActionEntry!.PosName)
					MainWindow.Instance!.AddEditedCutscene();

				part.ActionEntry!.PosName = text ?? string.Empty;
			});

		_animNameSubscription = AnimName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != part.ActionEntry!.AnimName)
					MainWindow.Instance!.AddEditedCutscene();

				part.ActionEntry!.AnimName = text ?? string.Empty;
			});
	}

	private void DisposeSubscriptions()
	{
		_castNameBoxSubscription?.Dispose();
		_castIDSubscription?.Dispose();
		_actionTypeSubscription?.Dispose();
		_posNameSubscription?.Dispose();
		_animNameSubscription?.Dispose();
	}
}
