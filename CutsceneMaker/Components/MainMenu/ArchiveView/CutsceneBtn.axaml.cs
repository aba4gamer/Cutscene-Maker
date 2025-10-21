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
	public ContextMenu CtxMenu;

	public CutsceneBtn()
	{
		InitializeComponent();
		CtxMenu = (ContextMenu) MainWindow.Instance!.FindResource("CutsceneMenuCtx")!;
	}

	public void LoadName(string cutsceneName)
	{
		CutsceneNameTxt.Text = CutsceneName = cutsceneName;
		ToolTip.SetTip(ToolTipPanel, CutsceneName);

		ToolTipPanel.ContextMenu = CtxMenu;
	}



	public void OnClickCutscene(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn)
			Click(btn, CutsceneName);
	}

	public void OnPointerPressed(object sender, RoutedEventArgs e)
	{
		CtxMenu.Open(ToolTipPanel);
	}
}
