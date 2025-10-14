using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Abacus;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class ArchiveView : UserControl
{
	public Action<string> Click = (string name) => {};
	public ContextMenu? CtxMenu;
	public ContextMenu? PanelCtxMenu;
	public CutsceneView? CutsceneUI { get; private set; } = null;

	public ContextMenu? PartCtx = null;
	public ContextMenu? SubPartCtx = null;
	public ContextMenu? PartEditCtx = null;
	public ContextMenu? SubPartEditCtx = null;


	public ArchiveView()
	{
		InitializeComponent();
	}



	public void LoadCutsceneList(List<string> cutsceneNames)
	{
		Sidebar.Children.Clear();
		Main.Children.Clear();
		Main.Children.Add(new CutsceneViewEmpty());

		if (cutsceneNames.Count < 1)
		{
			CutscenePanelEmpty empty = new();
			empty.Ctx = PanelCtxMenu;
			empty.LoadContextMenu();

			Sidebar.Children.Add(empty);
			return;
		}

		CutscenePanel panel = new();
		panel.Click = Click;
		panel.CtxMenu = CtxMenu;
		panel.PanelCtxMenu = PanelCtxMenu;
		panel.LoadPanelCtxMenu();
		panel.LoadCutsceneList(cutsceneNames);
		Sidebar.Children.Add(panel);
	}

	public void LoadCutsceneListAndSelect(List<string> cutsceneNames, string selectedCutsceneName)
	{
		Sidebar.Children.Clear();
		Main.Children.Clear();
		Main.Children.Add(new CutsceneViewEmpty());

		if (cutsceneNames.Count < 1)
		{
			CutscenePanelEmpty empty = new();
			empty.Ctx = PanelCtxMenu;
			empty.LoadContextMenu();

			Sidebar.Children.Add(empty);
			return;
		}

		CutscenePanel panel = new();
		panel.Click = Click;
		panel.CtxMenu = CtxMenu;
		panel.PanelCtxMenu = PanelCtxMenu;
		panel.LoadPanelCtxMenu();
		panel.LoadCutsceneListAndSelect(cutsceneNames, selectedCutsceneName);
		Sidebar.Children.Add(panel);
	}

	public void LoadCutscene(Cutscene cutscene)
	{
		Main.Children.Clear();
		CutsceneUI = new();
		CutsceneUI.PartCtx = PartCtx;
		CutsceneUI.PartEditCtx = PartEditCtx;
		CutsceneUI.SubPartCtx = SubPartCtx;
		CutsceneUI.SubPartEditCtx = SubPartEditCtx;

		CutsceneUI.LoadCutscene(cutscene);
		Main.Children.Add(CutsceneUI);
	}
}
