using System;

using Abacus;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class CutsceneView : UserControl
{
	public TimelineView TimelineUI { get; private set; } = new();
	public CutsceneWorkstation? WorkstationUI { get; private set; } = new();


	public CutsceneView()
	{
		InitializeComponent();
	}


	public void LoadCutscene(Cutscene cutscene)
	{
		Timeline.Children.Clear();
		Main.Children.Clear();

		Timeline.Children.Add(TimelineUI);

		if (cutscene.Parts.Count < 1)
		{
			CutsceneWorkstationEmptyPart emptyWorkPart = new();
			Main.Children.Add(emptyWorkPart);
			return;
		}

		CutsceneWorkstationEmpty workEmpty = new();
		Main.Children.Add(workEmpty);
	}

	public void LoadPart(Cutscene.Part? part)
	{
		int parts = MainWindow.Instance!.Core.GetCutscene().Parts.Count;
		if (WorkstationUI == null)
			return;

		if (part == null)
		{
			TimelineUI.Part_Selected_Deselect();
			if (parts < 1)
			{
				CutsceneWorkstationEmptyPart emptyWorkPart = new();
				Main.Children.Clear();
				Main.Children.Add(emptyWorkPart);
				return;
			}
			CutsceneWorkstationEmpty emptyWork = new();
			Main.Children.Clear();
			Main.Children.Add(emptyWork);
			return;
		}

		Main.Children.Clear();

		WorkstationUI.LoadPart(part);

		Main.Children.Add(WorkstationUI);
	}

	public void LoadSubPart(SubPart subPart)
	{
		if (WorkstationUI == null)
			return;

		Main.Children.Clear();

		WorkstationUI.LoadSubPart(subPart);

		Main.Children.Add(WorkstationUI);
	}
}
