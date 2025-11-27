using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Input;
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

	public void GridClick(object sender, PointerPressedEventArgs e)
	{
		if (e.Properties.IsLeftButtonPressed)
			SelectedCheck.IsChecked = !SelectedCheck.IsChecked;
	}
}
