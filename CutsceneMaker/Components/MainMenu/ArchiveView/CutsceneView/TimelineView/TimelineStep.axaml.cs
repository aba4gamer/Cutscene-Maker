using System;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class TimelineStep : UserControl
{
	public TimelineStep()
	{
		InitializeComponent();
	}

	public TimelineStep(int i, int size)
	{
		InitializeComponent();
		StepCountLabel.Content = i;
		Main.Width = size;
	}
}
