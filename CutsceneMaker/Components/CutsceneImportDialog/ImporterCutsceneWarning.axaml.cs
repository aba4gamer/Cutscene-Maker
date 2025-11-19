using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class ImporterCutsceneWarning : UserControl
{
	public ImporterCutsceneWarning()
	{
		InitializeComponent();
	}

	public ImporterCutsceneWarning(string name, string warn)
	{
		InitializeComponent();
		CutsceneName.Text = name;
		ToolTip.SetTip(Warning, warn);
	}
}
