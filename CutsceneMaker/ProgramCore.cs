using System;
using System.IO;

using Abacus;



namespace CutsceneMaker;

public class CutsceneCore
{

	public CutsceneArchive? Archive { get; private set; } = null;
	public string? SelectedPartName { get; private set; } = null;
	public string? SelectedSubPartName { get; private set; } = null;

	public CutsceneCore()
	{

	}



	#region Checks
	public bool HasArchiveOpen()
	{
		return Archive != null;
	}

	public bool HasCutsceneSelected()
	{
		return Archive != null && Archive.HasCutsceneLoaded();
	}

	public bool HasPartSelected()
	{
		return HasCutsceneSelected() && SelectedPartName != null;
	}

	public bool HasSubPartSelected()
	{
		return HasPartSelected() && SelectedSubPartName != null;
	}
	#endregion Checks



	#region Gets
	public Cutscene GetCutscene()
	{
		if (Archive == null) throw new Exception("No Archive selected!");
		return Archive.GetLoadedCutscene();
	}
	#endregion Gets



	#region Archive
	public void LoadArchive(CutsceneArchive arc)
	{
		SelectedPartName = null;
		SelectedSubPartName = null;
		Archive = arc;
	}

	public void SaveArchive()
	{
		if (!HasArchiveOpen())
			return;

		Archive.Save();
	}

	public void SaveArchiveTo(string path)
	{
		if (!HasArchiveOpen())
			return;

		if (!File.Exists(path))
		{
			FileStream stream = File.Create(path);
			stream.Close();
		}
		Archive.SaveTo(path);
	}
	#endregion Archive



	#region Cutscene
	public void RenameCutscene(string newName)
	{

	}
	#endregion Cutscene



	#region CutscenePart
	public void SetSelectedPart(string partName)
	{
		SelectedPartName = partName;
	}

	public Cutscene.Part GetSelectedPart()
	{
		if (!HasCutsceneSelected())
			throw new Exception("Cannot return a Part if there is no cutscene loaded!");

		foreach (Cutscene.Part part in Archive.GetLoadedCutscene().Parts)
		{
			if (part.PartName == SelectedPartName)
				return part;
		}

		throw new Exception($"No parts found with the name '{SelectedPartName}'!");
	}
	#endregion CutscenePart



	#region CutsceneSubPart
	public void SetSelectedSubPart(string partName)
	{
		SelectedSubPartName = partName;
	}

	public SubPart GetSelectedSubPart()
	{
		if (!HasPartSelected())
			throw new Exception("Cannot return a SubPart if there is no Part selected!");

		foreach (SubPart part in GetSelectedPart().SubPartEntries)
		{
			if (part.SubPartName == SelectedSubPartName)
				return part;
		}

		throw new Exception($"No subParts found with the name '{SelectedSubPartName}'!");
	}

	public void DeselectSubPart()
	{
		SelectedSubPartName = null;
	}
	#endregion CutsceneSubPart

}
