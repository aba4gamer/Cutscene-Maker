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

	public TimelineStep(int i)
	{
		InitializeComponent();
		StepCountLabel.Content = i;
	}
}
