using System;

using Abacus;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class CutsceneView : UserControl
{
	public TimelineView TimelineUI { get; private set; } = new();
	public CutsceneWorkstation? WorkstationUI { get; private set; } = new();

	public Action<int> Part_TotalStep = (int step) => {};
	public Action<int> SubPart_TotalStep = (int step) => {};
	public Action<string> Part_UpdateName = (string name) => {};
	public Action<string> SubPart_UpdateName = (string name) => {};


	public CutsceneView()
	{
		InitializeComponent();
	}


	public void LoadCutscene(Cutscene cutscene)
	{
		Timeline.Children.Clear();
		Main.Children.Clear();

		TimelineUI = new();
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

	public void LoadPart(Cutscene.Part? part, int parts)
	{
		if (WorkstationUI == null)
			return;

		if (part == null)
		{
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

		WorkstationUI.Part_TotalStep = TotalStepChange;
		WorkstationUI.Part_PartName = PartNameChange;
		WorkstationUI.LoadPart(part);

		Main.Children.Add(WorkstationUI);
	}

	public void LoadSubPart(SubPart subPart)
	{
		if (WorkstationUI == null)
			return;

		Main.Children.Clear();

		WorkstationUI.SubPart_TotalStep = TotalStepChangeSubPart;
		WorkstationUI.SubPart_PartName = SubPartNameChange;
		WorkstationUI.LoadSubPart(subPart);

		Main.Children.Add(WorkstationUI);
	}

	private void TotalStepChange(int step)
	{
		TimelineUI.UpdateSelectedPartStep(step);
		Part_TotalStep(step);
	}

	private void TotalStepChangeSubPart(int step)
	{
		TimelineUI.UpdateSelectedSubPartStep(step);
		SubPart_TotalStep(step);
	}

	private void PartNameChange(string name)
	{
		TimelineUI.UpdateSelectedPartName(name);
		Part_UpdateName(name);
	}

	private void SubPartNameChange(string name)
	{
		TimelineUI.UpdateSelectedSubPartName(name);
		SubPart_UpdateName(name);
	}
}
