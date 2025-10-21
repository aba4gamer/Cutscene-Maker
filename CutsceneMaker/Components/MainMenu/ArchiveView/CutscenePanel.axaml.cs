using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class CutscenePanel : UserControl
{
	public CutscenePanel()
	{
		InitializeComponent();
	}

	public void LoadPanelCtxMenu()
	{
		Scroller.ContextMenu = MainWindow.Instance!.FindResource("CutscenePanelCtx")! as ContextMenu;
	}

	public void LoadCutsceneList(List<string> cutsceneNames)
	{
		CutsceneBtns.Children.Clear();

		foreach(string name in cutsceneNames)
		{
			CutsceneBtn btn = new();
			btn.Click = CutsceneSelected;
			btn.LoadName(name);

			CutsceneBtns.Children.Add(btn);
		}
	}

	public void LoadCutsceneListAndSelect(List<string> cutsceneNames, string selectedCutsceneName)
	{
		CutsceneBtns.Children.Clear();

		foreach(string name in cutsceneNames)
		{
			CutsceneBtn btn = new();
			btn.Click = CutsceneSelected;
			btn.LoadName(name);

			CutsceneBtns.Children.Add(btn);

			if (name == selectedCutsceneName)
				btn.Click(btn.MainBtn, name);
		}
	}



	private void ActivateAllButtons()
	{
		foreach (object obj in CutsceneBtns.Children)
		{
			if (obj is CutsceneBtn)
			{
				CutsceneBtn cBtn = (CutsceneBtn) obj;
				cBtn.MainBtn.IsEnabled = true;
			}
		}
	}

	private void CutsceneSelected(Button btn, string cutsceneName)
	{
		ActivateAllButtons();
		btn.IsEnabled = false;

		MainWindow.Instance!.SelectCutscene(cutsceneName);
	}
}
