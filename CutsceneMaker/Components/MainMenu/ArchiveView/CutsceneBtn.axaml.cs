using System;
using System.Threading.Tasks;
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
	public ContextMenu? CtxMenu = null;

	public CutsceneBtn()
	{
		InitializeComponent();
	}

	public void LoadName(string cutsceneName)
	{
		CutsceneNameTxt.Text = CutsceneName = cutsceneName;
		ToolTip.SetTip(ToolTipPanel, CutsceneName);

		if (CtxMenu != null)
			ToolTipPanel.ContextMenu = CtxMenu;
	}



	public void OnClickCutscene(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn)
			Click(btn, CutsceneName);
	}

	public void OnPointerPressed(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("Hey");
		if (CtxMenu != null)
			CtxMenu.Open(ToolTipPanel);
	}
}
