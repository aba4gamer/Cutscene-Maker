using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System;

namespace CutsceneMaker;

public partial class Sound : UserControl
{
    private IDisposable? _bgmSubscription;
    private IDisposable? _systemSeSubscription;
    private IDisposable? _actionSeSubscription;
    private IDisposable? _returnBgmSubscription;
    private IDisposable? _bgmWipeOutFrameSubscription;
    private IDisposable? _allSoundStopFrameSubscription;

    public Sound()
    {
        InitializeComponent();
    }

    public Sound(ICommonEntries part)
    {
        InitializeComponent();

        if (part.SoundEntry != null)
        {
            IsSoundEnabled.IsChecked = true;
            BGM.Text = part.SoundEntry.Bgm;
            SystemSe.Text = part.SoundEntry.SystemSe;
            ActionSe.Text = part.SoundEntry.ActionSe;
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

                part.SoundEntry.Bgm = BGM.Text ?? string.Empty;
                part.SoundEntry.SystemSe = SystemSe.Text ?? string.Empty;
                part.SoundEntry.ActionSe = ActionSe.Text ?? string.Empty;
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

        _bgmSubscription = BGM.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.SoundEntry!.Bgm = text ?? string.Empty);

        _systemSeSubscription = SystemSe.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.SoundEntry!.SystemSe = text ?? string.Empty);

        _actionSeSubscription = ActionSe.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.SoundEntry!.ActionSe = text ?? string.Empty);

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
