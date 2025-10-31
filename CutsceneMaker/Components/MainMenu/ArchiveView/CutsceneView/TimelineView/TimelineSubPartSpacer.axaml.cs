using System;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class TimelineSubPartSpacer : UserControl
{
	public TimelineSubPartSpacer()
	{
		InitializeComponent();
	}

	public TimelineSubPartSpacer(int space)
	{
		InitializeComponent();
		Spacer.Width = space;
	}
}
