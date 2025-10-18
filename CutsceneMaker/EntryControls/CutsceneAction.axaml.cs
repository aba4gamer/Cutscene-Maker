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

		CastNameBox.AutoCompletion = Program.Utility.ObjDataTableEnglishNames;
		if (part.ActionEntry != null)
		{
			if (Program.Utility.ObjDataTableList.ContainsValue(part.ActionEntry.CastName) && Program.Utility.LoadedObject != Program.Utility.ObjDataTableList.Inverse[part.ActionEntry.CastName])
			{
				Program.Utility.LoadRarc_ObjectAnim(Program.Utility.ObjDataTableList.Inverse[part.ActionEntry.CastName]);
				AnimName.AutoCompletion = Program.Utility.ObjectAnimList;
			}


			IsActionEnabled.IsChecked = true;
			CastNameBox.Main.Text = Program.Utility.ObjDataTableList.ContainsValue(part.ActionEntry.CastName) ? Program.Utility.ObjDataTableList.Inverse[part.ActionEntry.CastName] : part.ActionEntry.CastName;
			CastID.Value = part.ActionEntry.CastID;
			ActionType.SelectedIndex = part.ActionEntry.ActionType;
			PosName.Text = part.ActionEntry.PosName;
			AnimName.Main.Text = part.ActionEntry.AnimName;

			SubscribeToChanges(part);
		}

		IsActionEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
		{
			if (isChecked == true)
			{
				part.ActionEntry ??= new Abacus.Action();
				SetControlsEnabled(true);

				AnimName.AutoCompletion = [];

				part.ActionEntry.CastName = CastNameBox.Main.Text != null ? Program.Utility.ObjDataTableList.ContainsKey(CastNameBox.Main.Text) ? Program.Utility.ObjDataTableList[CastNameBox.Main.Text] : CastNameBox.Main.Text : "";
				part.ActionEntry.CastID = CastID.Value.HasValue ? (int)CastID.Value.Value : -1;
				part.ActionEntry.ActionType = ActionType.SelectedIndex;
				part.ActionEntry.PosName = PosName.Text ?? string.Empty;
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
				if (text != null && Program.Utility.LoadedObject != text)
				{
					Program.Utility.LoadRarc_ObjectAnim(text);
					AnimName.AutoCompletion = Program.Utility.ObjectAnimList;
				}
				else
					AnimName.AutoCompletion = [];

				part.ActionEntry!.CastName = text != null ? Program.Utility.ObjDataTableList.ContainsKey(text) ? Program.Utility.ObjDataTableList[text] : text : "";
			});

		_castIDSubscription = CastID.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value => part.ActionEntry!.CastID = value.HasValue ? (int)value.Value : -1);

		_actionTypeSubscription = ActionType.GetObservable(ComboBox.SelectedIndexProperty)
			.Subscribe(value => part.ActionEntry!.ActionType = ActionType.SelectedIndex);

		_posNameSubscription = PosName.GetObservable(TextBox.TextProperty)
			.Subscribe(text => part.ActionEntry!.PosName = text ?? string.Empty);

		_animNameSubscription = AnimName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text => part.ActionEntry!.AnimName = text ?? string.Empty);
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
