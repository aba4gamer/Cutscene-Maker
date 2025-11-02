using System;

using Avalonia;
using Avalonia.Controls;

using CutsceneMakerUI;



namespace CutsceneMaker;

public partial class SettingsDialog : Window
{
	public SettingsDialog()
	{
		InitializeComponent();
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		MainWindow.Instance!.IsSettingsOpen = false;
	}
}
