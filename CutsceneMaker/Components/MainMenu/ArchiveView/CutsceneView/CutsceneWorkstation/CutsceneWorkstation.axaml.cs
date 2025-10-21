using System;

using Abacus;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class CutsceneWorkstation : UserControl
{
	public CutsceneWorkstation()
	{
		InitializeComponent();
	}

	public void LoadPart(Cutscene.Part part)
	{
		CutsceneMainPart partUI = new(part);
		CutscenePlayer player = new(part);
		CutsceneAction action = new(part);
		CutsceneCamera camera = new(part);
		CutsceneSound sound = new(part);
		CutsceneWipe wipe = new(part);

		PartTab.Header = "Main Part";
		PartTabContent.Children.Clear();
		PlayerTab.Children.Clear();
		ActionTab.Children.Clear();
		CameraTab.Children.Clear();
		SoundTab.Children.Clear();
		WipeTab.Children.Clear();

		PartTabContent.Children.Add(partUI);
		PlayerTab.Children.Add(player);
		ActionTab.Children.Add(action);
		CameraTab.Children.Add(camera);
		SoundTab.Children.Add(sound);
		WipeTab.Children.Add(wipe);
	}

	public void LoadSubPart(SubPart subPart)
	{
		CutsceneSubPart subPartUI = new(subPart);
		CutscenePlayer player = new(subPart);
		CutsceneAction action = new(subPart);
		CutsceneCamera camera = new(subPart);
		CutsceneSound sound = new(subPart);
		CutsceneWipe wipe = new(subPart);

		PartTab.Header = "Sub Part";
		PartTabContent.Children.Clear();
		PlayerTab.Children.Clear();
		ActionTab.Children.Clear();
		CameraTab.Children.Clear();
		SoundTab.Children.Clear();
		WipeTab.Children.Clear();

		PartTabContent.Children.Add(subPartUI);
		PlayerTab.Children.Add(player);
		ActionTab.Children.Add(action);
		CameraTab.Children.Add(camera);
		SoundTab.Children.Add(sound);
		WipeTab.Children.Add(wipe);
	}
}
