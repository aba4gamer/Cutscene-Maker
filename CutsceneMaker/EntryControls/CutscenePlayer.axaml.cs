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
			Visible.IsChecked = part.PlayerEntry.Visible == 1;

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

				if (PosName.Main.Text == null && BckName.Main.Text == null && Visible.IsChecked == false)
				{
					Visible.IsChecked = true;
				}

				part.PlayerEntry.PosName = PosName.Main.Text ?? string.Empty;
				part.PlayerEntry.BckName = BckName.Main.Text ?? string.Empty;
				part.PlayerEntry.Visible = Visible.IsChecked == true ? 1 : 0;

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

		_visibleSubscription = Visible.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked =>
			{
				if (isChecked != (PlayerEntry.Visible != 0))
					MainWindow.Instance!.AddEditedCutscene();

				PlayerEntry.Visible = isChecked == true ? 1 : 0;
			});
	}

	private void DisposeSubscriptions()
	{
		_posNameSubscription?.Dispose();
		_bckNameSubscription?.Dispose();
		_visibleSubscription?.Dispose();
	}
}
