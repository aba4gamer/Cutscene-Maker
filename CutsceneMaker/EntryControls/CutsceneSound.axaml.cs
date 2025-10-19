using System;

using Abacus;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

using CutsceneMaker;




namespace CutsceneMakerUI;

public partial class CutsceneSound : UserControl
{
	private IDisposable? _bgmSubscription;
	private IDisposable? _systemSeSubscription;
	private IDisposable? _actionSeSubscription;
	private IDisposable? _returnBgmSubscription;
	private IDisposable? _bgmWipeOutFrameSubscription;
	private IDisposable? _allSoundStopFrameSubscription;

	public CutsceneSound()
	{
		InitializeComponent();
	}

	public CutsceneSound(ICommonEntries part)
	{
		InitializeComponent();
		BGM.AutoCompletion = Program.Utility.MusicList;
		SystemSe.AutoCompletion = SoundEffectsList.PREFIX;
		ActionSe.AutoCompletion = SoundEffectsList.PREFIX;

		if (part.SoundEntry != null)
		{
			IsSoundEnabled.IsChecked = true;
			BGM.Main.Text = part.SoundEntry.Bgm;
			SystemSe.Main.Text = part.SoundEntry.SystemSe;
			ActionSe.Main.Text = part.SoundEntry.ActionSe;
			ReturnBgm.IsChecked = part.SoundEntry.ReturnBgm != 0;
			BgmWipeOutFrame.Value = part.SoundEntry.WipeOutFrame;
			AllSoundStopFrame.IsChecked = part.SoundEntry.AllSoundStopFrame == 1;


			SubscribeToChanges(part);
		}

		IsSoundEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
		{
			if (isChecked == true)
			{
				part.SoundEntry ??= new Abacus.Sound();
				SetControlsEnabled(true);

				part.SoundEntry.Bgm = BGM.Main.Text ?? "";
				part.SoundEntry.SystemSe = SystemSe.Main.Text ?? string.Empty;
				part.SoundEntry.ActionSe = ActionSe.Main.Text ?? string.Empty;
				part.SoundEntry.ReturnBgm = ReturnBgm.IsChecked == true ? 1 : 0;
				part.SoundEntry.WipeOutFrame = BgmWipeOutFrame.Value.HasValue ? (int)BgmWipeOutFrame.Value.Value : -1;
				part.SoundEntry.AllSoundStopFrame = AllSoundStopFrame.IsChecked == true ? 1 : 0;

				SubscribeToChanges(part);
			}
			else
			{
				part.SoundEntry = null;
				SetControlsEnabled(false);

				DisposeSubscriptions();
			}
		});
	}

	private void SetControlsEnabled(bool isEnabled)
	{
		BGM.IsEnabled = isEnabled;
		SystemSe.IsEnabled = isEnabled;
		ActionSe.IsEnabled = isEnabled;
		ReturnBgm.IsEnabled = isEnabled;
		BgmWipeOutFrame.IsEnabled = isEnabled;
		AllSoundStopFrame.IsEnabled = isEnabled;
	}

	private void SubscribeToChanges(ICommonEntries part)
	{
		DisposeSubscriptions();

		_bgmSubscription = BGM.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text => part.SoundEntry!.Bgm = text ?? "");

		_systemSeSubscription = SystemSe.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != null)
				{
					if (text.StartsWith(SoundEffectsList.PREFIX[0]))
						SystemSe.AutoCompletion = SoundEffectsList.SYSTEM;
					else if (text.StartsWith(SoundEffectsList.PREFIX[1]))
						SystemSe.AutoCompletion = SoundEffectsList.PLAYER_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[2]))
						SystemSe.AutoCompletion = SoundEffectsList.PLAYER_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[3]))
						SystemSe.AutoCompletion = SoundEffectsList.BOSS_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[4]))
						SystemSe.AutoCompletion = SoundEffectsList.BOSS_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[5]))
						SystemSe.AutoCompletion = SoundEffectsList.OBJECT;
					else if (text.StartsWith(SoundEffectsList.PREFIX[6]))
						SystemSe.AutoCompletion = SoundEffectsList.ATMOSPHERE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[7]))
						SystemSe.AutoCompletion = SoundEffectsList.DEMO;
					else if (text.StartsWith(SoundEffectsList.PREFIX[8]))
						SystemSe.AutoCompletion = SoundEffectsList.ENEMY_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[9]))
						SystemSe.AutoCompletion = SoundEffectsList.ENEMY_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[10]))
						SystemSe.AutoCompletion = SoundEffectsList.SUPPORTER_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[11]))
						SystemSe.AutoCompletion = SoundEffectsList.SUPPORTER_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[12]))
						SystemSe.AutoCompletion = SoundEffectsList.REMIX_SEQ;
					else if (text.StartsWith(SoundEffectsList.PREFIX[13]))
						SystemSe.AutoCompletion = SoundEffectsList.HOME_BUTTON_MENU;
					else
						SystemSe.AutoCompletion = SoundEffectsList.PREFIX;
				}

				part.SoundEntry!.SystemSe = text ?? string.Empty;
			});

		_actionSeSubscription = ActionSe.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != null)
				{
					if (text.StartsWith(SoundEffectsList.PREFIX[0]))
						ActionSe.AutoCompletion = SoundEffectsList.SYSTEM;
					else if (text.StartsWith(SoundEffectsList.PREFIX[1]))
						ActionSe.AutoCompletion = SoundEffectsList.PLAYER_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[2]))
						ActionSe.AutoCompletion = SoundEffectsList.PLAYER_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[3]))
						ActionSe.AutoCompletion = SoundEffectsList.BOSS_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[4]))
						ActionSe.AutoCompletion = SoundEffectsList.BOSS_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[5]))
						ActionSe.AutoCompletion = SoundEffectsList.OBJECT;
					else if (text.StartsWith(SoundEffectsList.PREFIX[6]))
						ActionSe.AutoCompletion = SoundEffectsList.ATMOSPHERE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[7]))
						ActionSe.AutoCompletion = SoundEffectsList.DEMO;
					else if (text.StartsWith(SoundEffectsList.PREFIX[8]))
						ActionSe.AutoCompletion = SoundEffectsList.ENEMY_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[9]))
						ActionSe.AutoCompletion = SoundEffectsList.ENEMY_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[10]))
						ActionSe.AutoCompletion = SoundEffectsList.SUPPORTER_VOICE;
					else if (text.StartsWith(SoundEffectsList.PREFIX[11]))
						ActionSe.AutoCompletion = SoundEffectsList.SUPPORTER_MOTION;
					else if (text.StartsWith(SoundEffectsList.PREFIX[12]))
						ActionSe.AutoCompletion = SoundEffectsList.REMIX_SEQ;
					else if (text.StartsWith(SoundEffectsList.PREFIX[13]))
						ActionSe.AutoCompletion = SoundEffectsList.HOME_BUTTON_MENU;
					else
						ActionSe.AutoCompletion = SoundEffectsList.PREFIX;
				}

				part.SoundEntry!.ActionSe = text ?? string.Empty;
			});

		_returnBgmSubscription = ReturnBgm.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked => part.SoundEntry!.ReturnBgm = isChecked == true ? 1 : 0);

		_bgmWipeOutFrameSubscription = BgmWipeOutFrame.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value => part.SoundEntry!.WipeOutFrame = value.HasValue ? (int)value.Value : -1);

		_allSoundStopFrameSubscription = AllSoundStopFrame.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked => part.SoundEntry!.AllSoundStopFrame = isChecked == true ? 1 : -1);
	}

	private void DisposeSubscriptions()
	{
		_bgmSubscription?.Dispose();
		_systemSeSubscription?.Dispose();
		_actionSeSubscription?.Dispose();
		_returnBgmSubscription?.Dispose();
		_bgmWipeOutFrameSubscription?.Dispose();
		_allSoundStopFrameSubscription?.Dispose();
	}
}
