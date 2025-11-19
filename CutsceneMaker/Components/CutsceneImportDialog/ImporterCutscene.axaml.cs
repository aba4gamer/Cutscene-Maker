using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class ImporterCutscene : UserControl
{
	public ImporterCutscene()
	{
		InitializeComponent();
	}

	public ImporterCutscene(string name)
	{
		InitializeComponent();
		CutsceneName.Text = name;
	}
}
