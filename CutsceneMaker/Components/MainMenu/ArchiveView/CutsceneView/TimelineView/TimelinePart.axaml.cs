using System;

using Abacus;

using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class TimelinePart : UserControl
{
	public string PartName { get; private set; } = "";
	public bool IsSelected { get; private set; } = false;
	public bool IsSubPart { get; private set; } = false;


	public TimelinePart()
	{
		InitializeComponent();
	}

	public TimelinePart(ICommonEntries part, string partName, int totalStep, bool isSubPart, int zoom)
	{
		InitializeComponent();
		NameLabel.Content = PartName = partName;

		if (isSubPart)
			Border.ContextMenu = (ContextMenu) MainWindow.Instance!.FindResource("SubPartMenuCtx")!;
		else
			Border.ContextMenu = (ContextMenu) MainWindow.Instance!.FindResource("PartMenuCtx")!;

		Border.Width = totalStep * zoom;
		Border.AddHandler(PointerPressedEvent, OnClick, RoutingStrategies.Tunnel);

		IsSubPart = isSubPart;

		ChangePartEnabledIcons(part);
	}

	public void Select(bool select)
	{
		if (!select)
			if (IsSubPart)
				Border.Background = Brush.Parse("#393959");
			else
				Border.Background = Brush.Parse("#446");
		else
			if (IsSubPart)
				Border.Background = Brush.Parse("#595979");
			else
				Border.Background = Brush.Parse("#668");

		IsSelected = select;
	}

	public void SelectedSubPart()
	{
		IsSelected = false;
		Border.Background = Brush.Parse("#5e5e7e");
	}


	private void OnClick(object? sender, RoutedEventArgs e)
	{
		if (IsSelected)
			return;

		if (IsSubPart)
		{
			MainWindow.Instance!.ArchiveUI!.CutsceneUI!.TimelineUI!.TimelineSubPart_SetSelected(this);
		}
		else
			MainWindow.Instance!.ArchiveUI!.CutsceneUI!.TimelineUI!.TimelinePart_SetSelected(this);
	}

	public void ChangeName(string newName)
	{
		NameLabel.Content = PartName = newName;
		ToolTip.SetTip(Border, newName);
	}

	public void ChangePartEnabledIcons(ICommonEntries part)
	{
		string toolTip = $"Name: \"{PartName}\"\n\nRoles: ";

		if (part.PlayerEntry != null)
		{
			PlayerIcon.IsVisible = true;
			toolTip += "Player, ";
		}
		else
			PlayerIcon.IsVisible = false;

		if (part.ActionEntry != null)
		{
			ActionIcon.IsVisible = true;
			toolTip += "Action, ";
		}
		else
			ActionIcon.IsVisible = false;

		if (part.CameraEntry != null)
		{
			CameraIcon.IsVisible = true;
			toolTip += "Camera, ";
		}
		else
			CameraIcon.IsVisible = false;

		if (part.SoundEntry != null)
		{
			SoundIcon.IsVisible = true;
			toolTip += "Sound, ";
		}
		else
			SoundIcon.IsVisible = false;

		if (part.WipeEntry != null)
		{
			WipeIcon.IsVisible = true;
			toolTip += "Wipe  ";
		}
		else
			WipeIcon.IsVisible = false;

		if (toolTip.Length == 17 + PartName.Length)
			toolTip += "Nothing.  ";

		ToolTip.SetTip(Border, toolTip.Substring(0, toolTip.Length - 2));
	}
}
