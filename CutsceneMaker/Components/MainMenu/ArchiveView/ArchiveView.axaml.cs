using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Abacus;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class ArchiveView : UserControl
{
	public CutsceneView CutsceneUI;


	public ArchiveView()
	{
		InitializeComponent();
		CutsceneUI = new();
	}



	public void LoadCutsceneList(List<string> cutsceneNames)
	{
		Sidebar.Children.Clear();
		Main.Children.Clear();
		Main.Children.Add(new CutsceneViewEmpty());

		if (cutsceneNames.Count < 1)
		{
			CutscenePanelEmpty empty = new();
			empty.LoadContextMenu();
			Sidebar.Children.Add(empty);
			return;
		}

		CutscenePanel panel = new();
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
			empty.LoadContextMenu();
			Sidebar.Children.Add(empty);
			return;
		}

		CutscenePanel panel = new();
		panel.LoadPanelCtxMenu();
		panel.LoadCutsceneListAndSelect(cutsceneNames, selectedCutsceneName);
		Sidebar.Children.Add(panel);
	}

	public void LoadCutscene(Cutscene cutscene)
	{
		Main.Children.Clear();
		CutsceneUI.LoadCutscene(cutscene);
		Main.Children.Add(CutsceneUI);
	}
}
