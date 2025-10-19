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
		BckName.AutoCompletion = Program.Utility.MarioAnimeList;
		PosName.AutoCompletion = Program.Utility.GeneralPosList;


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
			if (isChecked == true)
			{
				part.PlayerEntry ??= new Abacus.Player();
				SetControlsEnabled(true);

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
			.Subscribe(text => PlayerEntry.PosName = text ?? "");

		_bckNameSubscription = BckName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text => PlayerEntry.BckName = text ?? "");

		_visibleSubscription = Visible.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked => PlayerEntry.Visible = isChecked == true ? 1 : 0);
	}

	private void DisposeSubscriptions()
	{
		_posNameSubscription?.Dispose();
		_bckNameSubscription?.Dispose();
		_visibleSubscription?.Dispose();
	}
}
