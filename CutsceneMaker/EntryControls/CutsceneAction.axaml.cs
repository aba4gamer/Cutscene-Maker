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
	private IDisposable? _actionTypeSMG1Subscription;
	private IDisposable? _actionTypeSMG2Subscription;
	private IDisposable? _posNameSubscription;
	private IDisposable? _animNameSubscription;


	public CutsceneAction()
	{
		InitializeComponent();
	}

	public CutsceneAction(ICommonEntries part)
	{
		InitializeComponent();

		if (MainWindow.Instance!.Core.GetArchive().IsSMG1)
		{
			ActionTypeSMG2.IsVisible = false;
			ActionTypeSMG1.IsVisible = true;
		}
		else
		{
			ActionTypeSMG2.IsVisible = true;
			ActionTypeSMG1.IsVisible = false;
		}

		PosName.AutoCompletion = Program.AutoCompletion.GeneralPosList;
		CastNameBox.AutoCompletion = Program.AutoCompletion.ObjDataTableEnglishNames;
		if (part.ActionEntry != null)
		{
			if (Program.AutoCompletion.ObjDataTableList.ContainsValue(part.ActionEntry.CastName) && Program.AutoCompletion.LoadedAnimObject != Program.AutoCompletion.ObjDataTableList.Inverse[part.ActionEntry.CastName])
			{
				Program.AutoCompletion.LoadRarc_ObjectAnim(Program.AutoCompletion.ObjDataTableList.Inverse[part.ActionEntry.CastName], MainWindow.Instance!.Core.GetArchive().IsSMG1);
				AnimName.AutoCompletion = Program.AutoCompletion.ObjectAnimList;
			}


			IsActionEnabled.IsChecked = true;
			CastNameBox.Main.Text = Program.AutoCompletion.ObjDataTableList.ContainsValue(part.ActionEntry.CastName) ? Program.AutoCompletion.ObjDataTableList.Inverse[part.ActionEntry.CastName] : part.ActionEntry.CastName;
			CastID.Value = part.ActionEntry.CastID;
			if (MainWindow.Instance!.Core.GetArchive().IsSMG1)
				ActionTypeSMG1.SelectedIndex = part.ActionEntry.ActionType;
			else
				ActionTypeSMG2.SelectedIndex = part.ActionEntry.ActionType;
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

				// Defaults
				if (CastNameBox.Main.Text == null && !CastID.Value.HasValue && (MainWindow.Instance!.Core.GetArchive().IsSMG1 ? ActionTypeSMG1.SelectedIndex == -1 : ActionTypeSMG2.SelectedIndex == -1) && PosName.Main.Text == null && AnimName.Main.Text == null)
				{
					CastID.Value = -1;
					if (MainWindow.Instance!.Core.GetArchive().IsSMG1)
						ActionTypeSMG1.SelectedIndex = 0;
					else
						ActionTypeSMG2.SelectedIndex = 1;
				}

				part.ActionEntry.CastName = CastNameBox.Main.Text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(CastNameBox.Main.Text) ? Program.AutoCompletion.ObjDataTableList[CastNameBox.Main.Text] : CastNameBox.Main.Text : "";
				part.ActionEntry.CastID = CastID.Value.HasValue ? (int)CastID.Value.Value : -1;
				part.ActionEntry.ActionType = MainWindow.Instance!.Core.GetArchive().IsSMG1 == true ? ActionTypeSMG1.SelectedIndex : ActionTypeSMG2.SelectedIndex;
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

			MainWindow.Instance!.ArchiveUI.CutsceneUI.TimelineUI.Entries_Changes(part);
		});
	}

	private void SetControlsEnabled(bool isEnabled)
	{
		CastNameBox.IsEnabled = isEnabled;
		CastID.IsEnabled = isEnabled;
		if (MainWindow.Instance!.Core.GetArchive().IsSMG1)
			ActionTypeSMG1.IsEnabled = isEnabled;
		else
			ActionTypeSMG2.IsEnabled = isEnabled;
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

				Program.AutoCompletion.LoadRarc_ObjectAnim(text, MainWindow.Instance!.Core.GetArchive().IsSMG1);
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

		if (MainWindow.Instance!.Core.GetArchive().IsSMG1)
			_actionTypeSMG1Subscription = ActionTypeSMG1.GetObservable(ComboBox.SelectedIndexProperty)
				.Subscribe(value =>
				{
					if (value != part.ActionEntry!.ActionType)
						MainWindow.Instance!.AddEditedCutscene();

					part.ActionEntry!.ActionType = value;
				});
		else
			_actionTypeSMG2Subscription = ActionTypeSMG2.GetObservable(ComboBox.SelectedIndexProperty)
				.Subscribe(value =>
				{
					if (value != part.ActionEntry!.ActionType)
						MainWindow.Instance!.AddEditedCutscene();

					part.ActionEntry!.ActionType = value;
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
		_actionTypeSMG1Subscription?.Dispose();
		_actionTypeSMG2Subscription?.Dispose();
		_posNameSubscription?.Dispose();
		_animNameSubscription?.Dispose();
	}
}
