using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Input;
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

	public void GridClick(object sender, PointerPressedEventArgs e)
	{
		if (e.Handled)
			return;

		if (e.Properties.IsLeftButtonPressed)
			SelectedCheck.IsChecked = !SelectedCheck.IsChecked;
	}

	public void WarnClick(object sender, PointerPressedEventArgs e)
	{
		e.Handled = true;
	}
}
