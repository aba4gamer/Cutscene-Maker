using System;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class CutscenePanelEmpty : UserControl
{
	public ContextMenu? Ctx = null;


	public CutscenePanelEmpty()
	{
		InitializeComponent();
	}


	public void LoadContextMenu()
	{
		CtxHolder.ContextMenu = MainWindow.Instance!.FindResource("CutscenePanelCtx")! as ContextMenu;
	}
}
