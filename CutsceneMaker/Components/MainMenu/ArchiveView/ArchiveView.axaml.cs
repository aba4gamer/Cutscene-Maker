using System;
using System.Collections.Generic;

using Abacus;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class ArchiveView : UserControl
{
	public Action<string> Click { get; set; } = (string name) => {};
	public CutsceneView? CutsceneUI { get; private set; } = null;

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
			Sidebar.Children.Add(new CutscenePanelEmpty());
			return;
		}

		CutscenePanel panel = new();
		panel.Click = Click;
		panel.LoadCutsceneList(cutsceneNames);
		Sidebar.Children.Add(panel);
	}

	public void LoadCutscene(Cutscene cutscene)
	{
		Main.Children.Clear();
		CutsceneUI = new CutsceneView();
		CutsceneUI.LoadCutscene(cutscene);
		Main.Children.Add(CutsceneUI);
	}
}
