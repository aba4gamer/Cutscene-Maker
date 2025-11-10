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
		CameraTargetName.AutoCompletion = Program.AutoCompletion.ObjDataTableEnglishNames;

		if (part.CameraEntry != null)
		{
			if (Program.AutoCompletion.ObjDataTableList.ContainsValue(part.CameraEntry.CameraTargetName) && Program.AutoCompletion.LoadedCanmObject != Program.AutoCompletion.ObjDataTableList.Inverse[part.CameraEntry.CameraTargetName])
			{
				Program.AutoCompletion.LoadRarc_ObjectCanm(Program.AutoCompletion.ObjDataTableList.Inverse[part.CameraEntry.CameraTargetName]);
				AnimCameraName.AutoCompletion = Program.AutoCompletion.ObjectCanmList;
			}

			IsCameraEnabled.IsChecked = true;
			CameraTargetName.Main.Text = Program.AutoCompletion.ObjDataTableList.ContainsValue(part.CameraEntry.CameraTargetName) ? Program.AutoCompletion.ObjDataTableList.Inverse[part.CameraEntry.CameraTargetName] : part.CameraEntry.CameraTargetName;
			CameraTargetCastID.Value = part.CameraEntry.CameraTargetCastID;
			AnimCameraName.Main.Text = part.CameraEntry.AnimCameraName;
			AnimCameraStartFrame.Value = part.CameraEntry.AnimCameraStartFrame;
			AnimCameraEndFrame.Value = part.CameraEntry.AnimCameraEndFrame;
			IsContinuous.IsChecked = part.CameraEntry.IsContinuous == 1;

			SubscribeToChanges(part);
		}

		IsCameraEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
		{
			if (isChecked != (part.CameraEntry != null))
				MainWindow.Instance!.AddEditedCutscene();

			if (isChecked == true)
			{
				part.CameraEntry ??= new Abacus.Camera();
				SetControlsEnabled(true);

				part.CameraEntry.CameraTargetName = CameraTargetName.Main.Text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(CameraTargetName.Main.Text) ? Program.AutoCompletion.ObjDataTableList[CameraTargetName.Main.Text] : CameraTargetName.Main.Text : "";
				part.CameraEntry.CameraTargetCastID = CameraTargetCastID.Value.HasValue ? (int)CameraTargetCastID.Value.Value : -1;
				part.CameraEntry.AnimCameraName = AnimCameraName.Main.Text ?? string.Empty;
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

			MainWindow.Instance!.ArchiveUI.CutsceneUI.TimelineUI.Entries_Changes(part);
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

		_cameraTargetNameSubscription = CameraTargetName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if ((text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(text) ? Program.AutoCompletion.ObjDataTableList[text] : text : "") != part.CameraEntry!.CameraTargetName)
					MainWindow.Instance!.AddEditedCutscene();

				Program.AutoCompletion.LoadRarc_ObjectCanm(text);
				AnimCameraName.AutoCompletion = Program.AutoCompletion.ObjectCanmList;

				part.CameraEntry!.CameraTargetName = text != null ? Program.AutoCompletion.ObjDataTableList.ContainsKey(text) ? Program.AutoCompletion.ObjDataTableList[text] : text : "";

			});

		_cameraTargetCastIDSubscription = CameraTargetCastID.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.CameraEntry!.CameraTargetCastID)
					MainWindow.Instance!.AddEditedCutscene();

				part.CameraEntry!.CameraTargetCastID = value.HasValue ? (int)value.Value : -1;
			});

		_animCameraNameSubscription = AnimCameraName.Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text != part.CameraEntry!.AnimCameraName)
					MainWindow.Instance!.AddEditedCutscene();

				part.CameraEntry!.AnimCameraName = text ?? string.Empty;
			});

		_animCameraStartFrameSubscription = AnimCameraStartFrame.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.CameraEntry!.AnimCameraStartFrame)
					MainWindow.Instance!.AddEditedCutscene();

				part.CameraEntry!.AnimCameraStartFrame = value.HasValue ? (int)value.Value : -1;
			});

		_animCameraEndFrameSubscription = AnimCameraEndFrame.GetObservable(NumericUpDown.ValueProperty)
			.Subscribe(value =>
			{
				if (value != part.CameraEntry!.AnimCameraEndFrame)
					MainWindow.Instance!.AddEditedCutscene();

				part.CameraEntry!.AnimCameraEndFrame = value.HasValue ? (int)value.Value : -1;
			});

		_isContinuousSubscription = IsContinuous.GetObservable(CheckBox.IsCheckedProperty)
			.Subscribe(isChecked =>
			{
				if (isChecked != (part.CameraEntry!.IsContinuous != -1))
					MainWindow.Instance!.AddEditedCutscene();

				part.CameraEntry!.IsContinuous = isChecked == true ? 1 : 0;
			});
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
