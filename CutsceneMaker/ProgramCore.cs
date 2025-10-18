using System;
using System.IO;
using System.Collections.Generic;

using Abacus;



namespace CutsceneMaker;

public class CutsceneCore
{

	private Data Core = new();

	public CutsceneCore()
	{

	}



	#region Checks
	public bool HasArchiveOpen()
	{
		return Core.HasArchiveOpen();
	}

	public bool HasCutsceneSelected()
	{
		return Core.HasCutsceneSelected();
	}

	public bool HasPartSelected()
	{
		return Core.HasPartSelected();
	}

	public bool HasSubPartSelected()
	{
		return Core.HasSubPartSelected();
	}
	#endregion



	#region Gets
	public CutsceneArchive GetArchive()
	{
		return Core.GetArchive();
	}

	public Cutscene GetCutscene()
	{
		return Core.GetArchive().GetLoadedCutscene();
	}

	public string GetSelectedPartName()
	{
		return Core.GetSelectedPartName();
	}

	public string GetSelectedSubPartName()
	{
		return Core.GetSelectedSubPartName();
	}
	#endregion



	#region Archive
	public void LoadArchive(CutsceneArchive arc)
	{
		Core.SetSelectedPartName(null);
		Core.SetSelectedSubPartName(null);
		Core.SetArchive(arc);
	}

	public void SaveArchive()
	{
		Core.GetArchive().Save();
	}

	public void SaveArchiveTo(string path)
	{
		if (!File.Exists(path))
		{
			FileStream stream = File.Create(path);
			stream.Close();
		}

		Core.GetArchive().SaveTo(path);
	}
	#endregion



	#region Cutscene
	public void LoadCutscene(string cutsceneName)
	{
		Core.GetArchive().LoadCutscene(cutsceneName);
		Core.SetSelectedPartName(null);
		Core.SetSelectedSubPartName(null);
	}

	public bool PartNameAlreadyExists(string partName)
	{
		foreach (Cutscene.Part part in Core.GetArchive().GetLoadedCutscene().Parts)
		{
			if (part.PartName == partName)
				return true;

			if (part.SubPartEntries != null)
			{
				foreach (SubPart subPart in part.SubPartEntries)
				{
					if (subPart.SubPartName == partName)
						return true;
				}
			}
		}

		return false;
	}

	public List<string> GetAllPartNames()
	{
		List<string> names = [];

		foreach (Cutscene.Part part in GetArchive().GetLoadedCutscene().Parts)
		{
			names.Add(part.PartName);
			if (part.SubPartEntries != null)
			{
				foreach(SubPart subPart in part.SubPartEntries)
				{
					names.Add(subPart.SubPartName);
				}
			}
		}

		return names;
	}

	public int GetStepUntilSelectedPart()
	{
		int step = 0;

		foreach (Cutscene.Part part in GetArchive().GetLoadedCutscene().Parts)
		{
			if (part.PartName == GetSelectedPartName())
				break;
			step += part.TimeEntry.TotalStep;
		}

		return step;
	}
	#endregion



	#region CutscenePart
	public void SetSelectedPart(string partName)
	{
		Core.SetSelectedPartName(partName);
	}

	public Cutscene.Part GetSelectedPart()
	{
		foreach (Cutscene.Part part in Core.GetArchive().GetLoadedCutscene().Parts)
		{
			if (part.PartName == Core.GetSelectedPartName())
				return part;
		}

		throw new Exception($"No parts found with the name '{Core.GetSelectedPartName()}'!");
	}
	#endregion



	#region CutsceneSubPart
	public void SetSelectedSubPart(string partName)
	{
		Core.SetSelectedSubPartName(partName);
	}

	public SubPart GetSelectedSubPart()
	{
		Cutscene.Part part = GetSelectedPart();
		if (part.SubPartEntries == null)
			throw new Exception($"{part.PartName} doesn't have any SubParts!");

		foreach (SubPart subPart in part.SubPartEntries)
		{
			if (subPart.SubPartName == Core.GetSelectedSubPartName())
				return subPart;
		}

		throw new Exception($"No subParts found with the name '{Core.GetSelectedSubPartName()}'!");
	}

	public void DeselectSubPart()
	{
		Core.SetSelectedSubPartName(null);
	}
	#endregion

}


public class Data
{
	private CutsceneArchive? Archive = null;
	private string? SelectedPartName = null;
	private string? SelectedSubPartName = null;

	public Data()
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
	#endregion



	#region Gets
	public CutsceneArchive GetArchive()
	{
		if (Archive == null)
			throw new Exception("Can't get Archive because it's null!");

		return Archive;
	}

	public string GetSelectedPartName()
	{
		if (Archive == null)
			throw new Exception("Can't get SelectedPartName because the Archive is null!");
		if (SelectedPartName == null)
			throw new Exception("Can't get the SelectedPartName because it's null!");

		return SelectedPartName;
	}

	public string GetSelectedSubPartName()
	{
		if (Archive == null)
			throw new Exception("Can't get SelectedSubPartName because the Archive is null!");
		if (SelectedPartName == null)
			throw new Exception("Can't get the SelectedSubPartName because the SelectedPartName is null!");
		if (SelectedSubPartName == null)
			throw new Exception("Can't get the SelectedSubPartName because it's null!");

		return SelectedSubPartName;
	}
	#endregion



	#region Sets
	public void SetArchive(CutsceneArchive archive)
	{
		Archive = archive;
	}

	public void SetSelectedPartName(string? selPartName)
	{
		SelectedPartName = selPartName;
	}

	public void SetSelectedSubPartName(string? selSubPartName)
	{
		SelectedSubPartName = selSubPartName;
	}
	#endregion
}
