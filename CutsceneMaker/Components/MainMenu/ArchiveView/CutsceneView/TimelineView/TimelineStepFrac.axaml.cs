using System;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class TimelineStepFrac : UserControl
{
	public TimelineStepFrac()
	{
		InitializeComponent();
	}

	public TimelineStepFrac(int size)
	{
		InitializeComponent();
		Main.Width = size;
	}
}
