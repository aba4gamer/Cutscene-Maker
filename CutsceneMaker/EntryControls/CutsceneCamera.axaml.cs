using System;

using Abacus;

using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;

using CutsceneMaker;




namespace CutsceneMakerUI;

public partial class CutsceneCamera : UserControl
{
	private IDisposable? _cameraTargetNameSubscription;
	private IDisposable? _cameraTargetCastIDSubscription;
	private IDisposable? _animCameraNameSubscription;
	private IDisposable? _animCameraStartFrameSubscription;
	private IDisposable? _animCameraEndFrameSubscription;
	private IDisposable? _isContinuousSubscription;

	public CutsceneCamera()
	{
		InitializeComponent();
	}

	public CutsceneCamera(ICommonEntries part)
	{
		InitializeComponent();
		CameraTargetName.AutoCompletion = Program.Utility.ObjDataTableEnglishNames;

		if (part.CameraEntry != null)
		{
			IsCameraEnabled.IsChecked = true;
			CameraTargetName.Main.Text = Program.Utility.ObjDataTableList.ContainsValue(part.CameraEntry.CameraTargetName) ? Program.Utility.ObjDataTableList.Inverse[part.CameraEntry.CameraTargetName] : part.CameraEntry.CameraTargetName;
			CameraTargetCastID.Value = part.CameraEntry.CameraTargetCastID;
			AnimCameraName.Text = part.CameraEntry.AnimCameraName;
			AnimCameraStartFrame.Value = part.CameraEntry.AnimCameraStartFrame;
			AnimCameraEndFrame.Value = part.CameraEntry.AnimCameraEndFrame;
			IsContinuous.IsChecked = part.CameraEntry.IsContinuous == 1;

			SubscribeToChanges(part);
		}

		IsCameraEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
		{
			if (isChecked == true)
			{
				part.CameraEntry ??= new Abacus.Camera();
				SetControlsEnabled(true);

				part.CameraEntry.CameraTargetName = CameraTargetName.Main.Text != null ? Program.Utility.ObjDataTableList.ContainsKey(CameraTargetName.Main.Text) ? Program.Utility.ObjDataTableList[CameraTargetName.Main.Text] : CameraTargetName.Main.Text : "";
				part.CameraEntry.CameraTargetCastID = CameraTargetCastID.Value.HasValue ? (int)CameraTargetCastID.Value.Value : -1;
				part.CameraEntry.AnimCameraName = AnimCameraName.Text ?? string.Empty;
				part.CameraEntry.AnimCameraStartFrame = AnimCameraStartFrame.Value.HasValue ? (int)AnimCameraStartFrame.Value.Value : -1;
				part.CameraEntry.AnimCameraEndFrame = AnimCameraEndFrame.Value.HasValue ? (int)AnimCameraEndFrame.Value.Value : -1;
				part.CameraEntry.IsContinuous = IsContinuous.IsChecked == true ? 1 : 0;

				SubscribeToChanges(part);
			}
			else
			{
				part.CameraEntry = null;
				SetControlsEnabled(false);

				DisposeSubscriptions();
			}
		});
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

	private void SubscribeToChanges(ICommonEntries part)
	{
		DisposeSubscriptions();

		_cameraTargetNameSubscription = CameraTargetName.GetObservable(TextBox.TextProperty)
			.Subscribe(text => part.CameraEntry!.CameraTargetName = text != null ? Program.Utility.ObjDataTableList.ContainsKey(text) ? Program.Utility.ObjDataTableList[text] : text : "");

		_cameraTargetCastIDSubscription = CameraTargetCastID.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value => part.CameraEntry!.CameraTargetCastID = value.HasValue ? (int)value.Value : -1);

		_animCameraNameSubscription = AnimCameraName.GetObservable(TextBox.TextProperty)
			.Subscribe(text => part.CameraEntry!.AnimCameraName = text ?? string.Empty);

		_animCameraStartFrameSubscription = AnimCameraStartFrame.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value => part.CameraEntry!.AnimCameraStartFrame = value.HasValue ? (int)value.Value : -1);

		_animCameraEndFrameSubscription = AnimCameraEndFrame.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value => part.CameraEntry!.AnimCameraEndFrame = value.HasValue ? (int)value.Value : -1);

		_isContinuousSubscription = IsContinuous.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked => part.CameraEntry!.IsContinuous = isChecked == true ? 1 : 0);
	}

	private void DisposeSubscriptions()
	{
		_cameraTargetNameSubscription?.Dispose();
		_cameraTargetCastIDSubscription?.Dispose();
		_animCameraNameSubscription?.Dispose();
		_animCameraStartFrameSubscription?.Dispose();
		_animCameraEndFrameSubscription?.Dispose();
		_isContinuousSubscription?.Dispose();
	}
}
