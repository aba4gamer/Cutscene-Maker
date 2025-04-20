using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;
using System.Reactive.Linq;

namespace CutsceneMaker;

public partial class Camera : UserControl
{
    /// <summary>
    /// ONLY USED FOR THE DESIGNER. DON'T USE!
    /// </summary>
    public Camera()
    {
        InitializeComponent();
    }
    public Camera(ICommonEntries part)
    {
        InitializeComponent();
        if (part.CameraEntry != null)
        {
            IsCameraEnabled.IsChecked = true;
            CameraTargetName.Text = part.CameraEntry.CameraTargetName;
            CameraTargetCastID.Value = part.CameraEntry.CameraTargetCastID;
            AnimCameraName.Text = part.CameraEntry.AnimCameraName;
            AnimCameraStartFrame.Value = part.CameraEntry.AnimCameraStartFrame;
            AnimCameraEndFrame.Value = part.CameraEntry.AnimCameraEndFrame;
            IsContinuous.IsChecked = part.CameraEntry.IsContinuous == 1;

            // I really have NO IDEA of how does this work... but if it works it's useful.
            CameraTargetName.GetObservable(TextBox.TextProperty).Subscribe(text => part.CameraEntry.CameraTargetName = text ?? string.Empty);
            CameraTargetCastID.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => part.CameraEntry.CameraTargetCastID = value.HasValue ? (int)value.Value : -1);
            AnimCameraName.GetObservable(TextBox.TextProperty).Subscribe(text => part.CameraEntry.AnimCameraName = text ?? string.Empty);
            AnimCameraStartFrame.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => part.CameraEntry.AnimCameraStartFrame = value.HasValue ? (int)value.Value : -1);
            AnimCameraEndFrame.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => part.CameraEntry.AnimCameraEndFrame = value.HasValue ? (int)value.Value : -1);
            IsContinuous.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked => part.CameraEntry.IsContinuous = isChecked == true ? 1 : 0);
        }

        IsCameraEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(Observer.Create<bool?>(isChecked =>
        {
            if (isChecked == true)
            {
                part.CameraEntry ??= new Abacus.Camera();
                SetControlsEnabled(true);

                part.CameraEntry.CameraTargetName = CameraTargetName.Text ?? string.Empty;
                part.CameraEntry.CameraTargetCastID = CameraTargetCastID.Value.HasValue ? (int)CameraTargetCastID.Value.Value : -1;
                part.CameraEntry.AnimCameraName = AnimCameraName.Text ?? string.Empty;
                part.CameraEntry.AnimCameraStartFrame = AnimCameraStartFrame.Value.HasValue ? (int)AnimCameraStartFrame.Value.Value : -1;
                part.CameraEntry.AnimCameraEndFrame = AnimCameraEndFrame.Value.HasValue ? (int)AnimCameraEndFrame.Value.Value : -1;
                part.CameraEntry.IsContinuous = IsContinuous.IsChecked == true ? 1 : 0;
            }
            else
            {
                part.CameraEntry = null;
                SetControlsEnabled(false);
            }
        }));
    }
    private void SetControlsEnabled(bool isEnabled)
    {
        CameraTargetName.IsEnabled = isEnabled;
        CameraTargetCastID.IsEnabled = isEnabled;
        AnimCameraName.IsEnabled = isEnabled;
        AnimCameraStartFrame.IsEnabled = isEnabled;
        AnimCameraEndFrame.IsEnabled = isEnabled;
        IsContinuous.IsEnabled = isEnabled;
    }
}
