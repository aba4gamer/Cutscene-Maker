using System;
using System.Reactive;

using Abacus;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using CutsceneMaker;




namespace CutsceneMakerUI;

public partial class CutscenePlayer : UserControl
{
	private IDisposable? _posNameSubscription;
	private IDisposable? _bckNameSubscription;
	private IDisposable? _visibleSubscription;

	public CutscenePlayer()
	{
		InitializeComponent();
	}

	public CutscenePlayer(ICommonEntries part)
	{
		InitializeComponent();
		BckName.AutoCompletion = Program.AutoCompletion.MarioAnimeList;
		PosName.AutoCompletion = Program.AutoCompletion.GeneralPosList;


		if (part.PlayerEntry != null)
		{
			IsPlayerEnabled.IsChecked = true;
			PosName.Main.Text = part.PlayerEntry.PosName;
			BckName.Main.Text = part.PlayerEntry.BckName;
			Visible.SelectedIndex = part.PlayerEntry.Visible + 1;

			SubscribeToChanges(part);
		}

		IsPlayerEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
		{
			if (isChecked != (part.PlayerEntry != null))
				MainWindow.Instance!.AddEditedCutscene();

			if (isChecked == true)
			{
				part.PlayerEntry ??= new Abacus.Player();
				SetControlsEnabled(true);

				if (PosName.Main.Text == null && BckName.Main.Text == null && Visible.SelectedIndex == -1)
				{
					Visible.SelectedIndex = 0;
				}

				part.PlayerEntry.PosName = PosName.Main.Text ?? string.Empty;
				part.PlayerEntry.BckName = BckName.Main.Text ?? string.Empty;
				part.PlayerEntry.Visible = Visible.SelectedIndex - 1;

				SubscribeToChanges(part);
			}
			else
			{
				part.PlayerEntry = null;
				SetControlsEnabled(false);

				DisposeSubscriptions();
			}

			MainWindow.Instance!.ArchiveUI.CutsceneUI.TimelineUI.Entries_Changes(part);
		});
	}

	private void SetControlsEnabled(bool isEnabled)
	{
		PosName.IsEnabled = isEnabled;
		BckName.IsEnabled = isEnabled;
		Visible.IsEnabled = isEnabled;
	}

	private void SubscribeToChanges(ICommonEntries part) {

		if (part.PlayerEntry == null) return;
		Abacus.Player PlayerEntry = part.PlayerEntry;

		DisposeSubscriptions();

		_posNameSubscription = PosName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != PlayerEntry.PosName)
					MainWindow.Instance!.AddEditedCutscene();

				PlayerEntry.PosName = text ?? "";
			});

		_bckNameSubscription = BckName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != PlayerEntry.BckName)
					MainWindow.Instance!.AddEditedCutscene();

				PlayerEntry.BckName = text ?? "";
			});

		_visibleSubscription = Visible.GetObservable(ComboBox.SelectedIndexProperty)
			.Subscribe(index =>
			{
				index--; // The first index is "0" but we need to start at "-1"
				if (index != PlayerEntry.Visible)
					MainWindow.Instance!.AddEditedCutscene();

				PlayerEntry.Visible = index;
			});
	}

	private void DisposeSubscriptions()
	{
		_posNameSubscription?.Dispose();
		_bckNameSubscription?.Dispose();
		_visibleSubscription?.Dispose();
	}
}
