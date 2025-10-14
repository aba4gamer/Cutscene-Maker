using System;
using System.Reactive.Subjects;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class CutsceneBtn : UserControl
{
	public string CutsceneName { get; private set; } = "";
	public Action<Button, string> Click { get; set; } = (Button btn, string name) => {};

	public CutsceneBtn()
	{
		InitializeComponent();
	}

	public CutsceneBtn(string cutsceneName)
	{
		InitializeComponent();
		CutsceneNameTxt.Text = CutsceneName = cutsceneName;
		ToolTip.SetTip(ToolTipPanel, CutsceneName);
	}



	public void OnClickCutscene(object sender, RoutedEventArgs e)
	{
		if (sender is Button)
		{
			Click((Button) sender, CutsceneName);
		}
	}
}
