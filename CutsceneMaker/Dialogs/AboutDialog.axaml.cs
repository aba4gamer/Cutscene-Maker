using System;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMaker;

public partial class AboutDialog : Window
{
	public AboutDialog()
	{
		InitializeComponent();
		Version.Text = $"Version: {Program.VERSION}";
	}
}
