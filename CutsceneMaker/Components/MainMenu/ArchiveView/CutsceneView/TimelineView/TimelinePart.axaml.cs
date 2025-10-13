using System;

using Abacus;

using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class TimelinePart : UserControl
{
	public Action<TimelinePart> Click = (TimelinePart timelinePart) => {};
	public string PartName { get; private set; } = "";
	public bool Selected { get; private set; } = false;
	public bool IsSubPart { get; private set; } = false;


	public TimelinePart()
	{
		InitializeComponent();
	}

	public TimelinePart(ICommonEntries part, string partName, int totalStep, bool isSubPart, int zoom)
	{
		InitializeComponent();
		NameLabel.Content = PartName = partName;

		Border.Width = totalStep * zoom;
		Border.AddHandler(PointerPressedEvent, OnClick, RoutingStrategies.Tunnel);
		ToolTip.SetTip(Border, partName);

		IsSubPart = isSubPart;

		// TODO: Add icons for what's enabled in this part.
	}

	public void Select(bool select)
	{
		if (!select)
			if (IsSubPart)
				Grid.Background = Brush.Parse("#595979");
			else
				Grid.Background = Brush.Parse("#668");
		else
			if (IsSubPart)
				Grid.Background = Brush.Parse("#393959");
			else
				Grid.Background = Brush.Parse("#446");
		Selected = select;
	}

	public void SelectedSubPart()
	{
		Selected = false;
		Grid.Background = Brush.Parse("#484880");
	}


	private void OnClick(object? sender, RoutedEventArgs e)
	{
		if (Selected)
			return;

		Click(this);
	}

	public void ChangeName(string newName)
	{
		NameLabel.Content = PartName = newName;
		ToolTip.SetTip(Border, newName);
	}
}
