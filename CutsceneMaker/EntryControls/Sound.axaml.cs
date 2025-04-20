using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;

namespace CutsceneMaker;

public partial class Sound : UserControl
{
    /// <summary>
    /// ONLY USED FOR THE DESIGNER. DON'T USE!
    /// </summary>
    public Sound()
    {
        InitializeComponent();
    }
    public Sound(ICommonEntries part)
    {
        InitializeComponent();

        // At this point of the code, Copilot is generating this.
        if (part.SoundEntry != null)
        {
            IsSoundEnabled.IsChecked = true;
            BGM.Text = part.SoundEntry.Bgm;
            SystemSe.Text = part.SoundEntry.SystemSe;
            ActionSe.Text = part.SoundEntry.ActionSe;
            ReturnBgm.IsChecked = part.SoundEntry.ReturnBgm != 0;
            BgmWipeOutFrame.Value = part.SoundEntry.WipeOutFrame;
            AllSoundStopFrame.IsChecked = part.SoundEntry.AllSoundStopFrame != 0;

            BGM.GetObservable(TextBox.TextProperty)
                .Subscribe(text => part.SoundEntry.Bgm = text ?? string.Empty);

            SystemSe.GetObservable(TextBox.TextProperty)
                .Subscribe(text => part.SoundEntry.SystemSe = text ?? string.Empty);

            ActionSe.GetObservable(TextBox.TextProperty)
                .Subscribe(text => part.SoundEntry.ActionSe = text ?? string.Empty);

            ReturnBgm.GetObservable(CheckBox.IsCheckedProperty)
                .Subscribe(isChecked => part.SoundEntry.ReturnBgm = isChecked == true ? 1 : 0);

            BgmWipeOutFrame.GetObservable(NumericUpDown.ValueProperty)
                .Subscribe(value => part.SoundEntry.WipeOutFrame = value.HasValue ? (int)value.Value : -1);

            AllSoundStopFrame.GetObservable(CheckBox.IsCheckedProperty)
                .Subscribe(isChecked => part.SoundEntry.AllSoundStopFrame = isChecked == true ? 1 : -1);
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
            }
            else
            {
                part.SoundEntry = null;
                SetControlsEnabled(false);
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
}