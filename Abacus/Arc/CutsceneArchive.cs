using System;
using System.IO;
using System.Collections.Generic;

using Hack.io;
using Hack.io.BCSV;
using Hack.io.RARC;
using Hack.io.YAZ0;
using Hack.io.Class;
using Hack.io.Utility;




namespace Abacus;

public class CutsceneArchive {

	public List<string> CutsceneNames { get; private set; } = [];
	public Dictionary<string, Cutscene> LoadedCutscenes { get; private set; } = new();
	public string? SelectedCutsceneName { get; private set; } = null;
	public string FilePath { get; private set; } = "";
	public bool IsYazCompressed { get; private set; } = true;
	public bool IsSMG1 { get; private set; } = false;
	private RARC _rarc;


	private CutsceneArchive(string path)
	{
		_rarc = new();
		FilePath = path;
	}


	private void LoadArchiveCutscenes()
	{
		if (IsSMG1)
			LoadSMG1ArchvieCutscenes();
		else
			LoadSMG2ArchiveCutscenes();
	}

	private void LoadSMG2ArchiveCutscenes()
	{
		object? _rarcPath = _rarc["Stage/csv"];

		if (_rarcPath == null)
			return;
		RARC.Directory csv = (RARC.Directory) _rarcPath;

		foreach (string path in csv.Items.Keys)
		{
			if (path.EndsWith("Time.bcsv"))
			{
				string name = path.Substring(0, path.Length - 9);
				this.CutsceneNames.Add(name);
			}
		}
	}

	private void LoadSMG1ArchvieCutscenes()
	{
		foreach (string path in _rarc.Root!.Items.Keys)
		{
			if (_rarc.Root[path] is RARC.Directory)
			{
				this.CutsceneNames.Add(path);
			}
		}
	}

	private static CutsceneArchiveReadWrapper? LoadArchiveHandler(string path, CutsceneArchive ca)
	{
		try
		{
			StreamUtil.SetEndianBig();
			if (FileUtil.LoadFileWithDecompression(path, ca._rarc.Load, [(YAZ0.Check, YAZ0.Decompress)]) == -1)
			{
				ca.IsYazCompressed = false;
				FileStream stream = File.OpenRead(path);
				ca._rarc.Load(stream);
				stream.Close();
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"Exception while trying to load archive: {path}!");
			Console.WriteLine(e.ToString());
			return CutsceneArchiveReadWrapper.Error($"Invalid .arc file! Are you sure that it's a Nintendo Revolution Archive? Error:\n{e.GetType()}: {e.Message}");
		}

		return null;
	}


	public static CutsceneArchiveReadWrapper LoadArchive(string path)
	{
		CutsceneArchive ca = new(path);
		CutsceneArchiveReadWrapper? errored = LoadArchiveHandler(path, ca);
		if (errored != null)
			return errored;

		if (ca._rarc.Root == null)
			return CutsceneArchiveReadWrapper.Error("The file is not a valid archive!");

		// if (ca._rarc.Root.Name != "Stage")
		// 	return CutsceneArchiveReadWrapper.Error("The arc doesn't have 'Stage' in the root!");

		if (ca._rarc.Root.Items.Keys.Count == 1 && ca._rarc.Root.ItemExists("csv"))
			ca.IsSMG1 = false;
		else
			ca.IsSMG1 = true;


		ca.LoadArchiveCutscenes();
		return CutsceneArchiveReadWrapper.Ok(ca);
	}



	public void LoadCutsceneName(string cutsceneName)
	{
		if (!LoadedCutscenes.ContainsKey(cutsceneName))
		{
			if (!IsSMG1)
			{
				object? rarcFileObj = _rarc[$"Stage/csv/{cutsceneName}Time.bcsv"];
				if (rarcFileObj == null) return;
			}
			else
			{
				object? rarcFileObj = _rarc.Root![$"{cutsceneName.ToLower()}/{cutsceneName.ToLower()}time.bcsv"];
				if (rarcFileObj == null) return;
			}

			LoadedCutscenes[cutsceneName] = Cutscene.NewCutsceneFromRarc(_rarc, IsSMG1, cutsceneName);
		}
	}

	public void LoadCutscene(string cutsceneName)
	{
		LoadCutsceneName(cutsceneName);

		SelectedCutsceneName = cutsceneName;
	}

	public Cutscene GetCutsceneWithName(string cutsceneName)
	{
		if (!LoadedCutscenes.ContainsKey(cutsceneName))
		{
			object? rarcFileObj;
			if (IsSMG1)
				rarcFileObj = _rarc.Root![$"{cutsceneName.ToLower()}/{cutsceneName.ToLower()}time.bcsv"];
			else
				rarcFileObj = _rarc[$"Stage/csv/{cutsceneName}Time.bcsv"];
			if (rarcFileObj == null)
				throw new Exception($"No cutscene named '{cutsceneName}'!");

			LoadedCutscenes[cutsceneName] = Cutscene.NewCutsceneFromRarc(_rarc, IsSMG1, cutsceneName);
		}

		return LoadedCutscenes[cutsceneName];
	}

	public bool HasCutsceneLoaded()
	{
		return SelectedCutsceneName != null;
	}

	public Cutscene GetLoadedCutscene()
	{
		if (SelectedCutsceneName == null) throw new Exception("No selected cutscenes");

		return LoadedCutscenes[SelectedCutsceneName];
	}

	public void CreateNewCutscene(string cutsceneName)
	{
		CutsceneNames.Add(cutsceneName);
		LoadedCutscenes[cutsceneName] = Cutscene.NewCutsceneFromTemplate(cutsceneName);
	}

	public string GetSelectedCutsceneName()
	{
		if (SelectedCutsceneName == null)
			throw new Exception("No cutscene selected!");

		return SelectedCutsceneName;
	}

	public void RenameCutscene(string cutsceneName, string newCutsceneName)
	{
		LoadCutsceneName(cutsceneName);

		LoadedCutscenes[newCutsceneName] = LoadedCutscenes[cutsceneName];
		LoadedCutscenes.Remove(cutsceneName);
		LoadedCutscenes[newCutsceneName].CutsceneName = newCutsceneName;

		int i = CutsceneNames.IndexOf(cutsceneName);
		CutsceneNames.RemoveAt(i);
		CutsceneNames.Insert(i, newCutsceneName);

		if (IsSMG1)
			_rarc.Root!.RenameItem($"{cutsceneName.ToLower()}", $"{newCutsceneName.ToLower()}");

		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "Time");
		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "Player");
		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "Wipe");
		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "Sound");
		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "Action");
		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "Camera");
		RenameCutsceneBCSVName(cutsceneName, newCutsceneName, "SubPart");

		if (cutsceneName == SelectedCutsceneName)
			SelectedCutsceneName = newCutsceneName;
	}

	private void RenameCutsceneBCSVName(string cutsceneName, string newCutsceneName, string bcsvName)
	{
		if (IsSMG1)
			if (_rarc.Root!.ItemExists($"{newCutsceneName.ToLower()}/{cutsceneName.ToLower()}{bcsvName.ToLower()}.bcsv"))
				if (_rarc.Root![newCutsceneName.ToLower()]! is RARC.Directory dir)
					dir.RenameItem($"{cutsceneName.ToLower()}{bcsvName.ToLower()}.bcsv", $"{newCutsceneName.ToLower()}{bcsvName.ToLower()}.bcsv");
		else
			if (_rarc.Root!.ItemExists($"csv/{cutsceneName}{bcsvName}.bcsv"))
				_rarc.Root!.RenameItem($"csv/{cutsceneName}{bcsvName}.bcsv", "csv/{newCutsceneName}{bcsvName}.bcsv");
	}

	public void RenameSelectedCutscene(string newCutsceneName)
	{
		RenameCutscene(GetSelectedCutsceneName(), newCutsceneName);
		SelectedCutsceneName = newCutsceneName;
	}

	public void DeleteCutscene(string cutsceneName)
	{
		if (!CutsceneNames.Contains(cutsceneName))
			return;

		LoadedCutscenes.Remove(cutsceneName);
		CutsceneNames.RemoveAt(CutsceneNames.IndexOf(cutsceneName));

		DeleteCutsceneBCSV(cutsceneName, "Time");
		DeleteCutsceneBCSV(cutsceneName, "Player");
		DeleteCutsceneBCSV(cutsceneName, "Wipe");
		DeleteCutsceneBCSV(cutsceneName, "Sound");
		DeleteCutsceneBCSV(cutsceneName, "Action");
		DeleteCutsceneBCSV(cutsceneName, "Camera");
		DeleteCutsceneBCSV(cutsceneName, "SubPart");

		if (cutsceneName == SelectedCutsceneName)
			SelectedCutsceneName = null;
	}

	private void DeleteCutsceneBCSV(string cutsceneName, string bcsvName)
	{
		if (IsSMG1)
		{
			if (_rarc.Root!.ItemExists($"{cutsceneName.ToLower()}/{cutsceneName.ToLower()}{bcsvName.ToLower()}.bcsv"))
				_rarc.Root[$"{cutsceneName.ToLower()}"] = null;
		}
		else
		{
			if (_rarc.Root!.ItemExists($"csv/{cutsceneName}{bcsvName}.bcsv"))
				_rarc.Root![$"csv/{cutsceneName}{bcsvName}.bcsv"] = null;
		}
	}

	public void ExportCutscene(string cutsceneName, string folderPath)
	{
		LoadCutsceneName(cutsceneName);

		LoadedCutscenes[cutsceneName].ExportAll(folderPath, IsSMG1);
	}

	public void ImportCutscene(Cutscene cutscene, bool FromSMG1)
	{
		// Archive:  SMG1
		// Cutscene: SMG1
		// Or
		// Archive:  SMG2
		// Cutscene: SMG2

		// We do nothing since we're importing in the same version


		// Archive:  SMG2
		// Cutscene: SMG1
		if (!IsSMG1 && FromSMG1)
		{
			cutscene.CutsceneName = cutscene.CutsceneName.Replace("demo", "Demo");
			while (CutsceneNames.Contains(cutscene.CutsceneName))
				cutscene.CutsceneName = cutscene.CutsceneName + "_imported";
			foreach (Cutscene.Part part in cutscene.Parts)
			{
				if (part.ActionEntry != null)
				{
					if (part.ActionEntry.ActionType < 11)
						part.ActionEntry.ActionType++;
					else if (part.ActionEntry.ActionType == 11)
						part.ActionEntry.ActionType = 0;
				}

				if (part.SubPartEntries != null)
					foreach (SubPart sPart in part.SubPartEntries)
					{
						if (sPart.ActionEntry != null)
						{
							if (sPart.ActionEntry.ActionType < 11)
								sPart.ActionEntry.ActionType++;
							else if (sPart.ActionEntry.ActionType == 11)
								sPart.ActionEntry.ActionType = 0;
						}
					}
			}
		}

		// Archive:  SMG1
		// Cutscene: SMG2
		if (IsSMG1 && !FromSMG1)
		{
			cutscene.CutsceneName = cutscene.CutsceneName.ToLower();
			while (CutsceneNames.Contains(cutscene.CutsceneName))
				cutscene.CutsceneName = cutscene.CutsceneName + "_imported";
			foreach (Cutscene.Part part in cutscene.Parts)
			{
				if (part.TimeEntry.WaitUserInputFlag > 0)
					part.TimeEntry.WaitUserInputFlag = 0;

				if (part.ActionEntry != null)
				{
					if (part.ActionEntry.ActionType == 0)
						part.ActionEntry.ActionType = 11;
					else if (part.ActionEntry.ActionType < 11)
						part.ActionEntry.ActionType++;
					else
						part.ActionEntry.ActionType = 0;
				}

				if (part.SubPartEntries != null)
					foreach (SubPart sPart in part.SubPartEntries)
					{
						if (sPart.ActionEntry != null)
						{
							if (sPart.ActionEntry.ActionType == 0)
								sPart.ActionEntry.ActionType = 11;
							else if (sPart.ActionEntry.ActionType < 12)
								sPart.ActionEntry.ActionType--;
							else
								sPart.ActionEntry.ActionType = 0;
						}
					}
			}
		}

		LoadedCutscenes[cutscene.CutsceneName] = cutscene;
		CutsceneNames.Add(cutscene.CutsceneName);
	}

	public void DeleteSelectedCutscene()
	{
		string cutsceneName = GetSelectedCutsceneName();
		DeleteCutscene(cutsceneName);
		SelectedCutsceneName = null;
	}

	public void Save()
	{
		foreach (Cutscene cutscene in LoadedCutscenes.Values)
		{
			cutscene.SaveAll(_rarc, IsSMG1);
		}

		_rarc.KeepFileIDsSynced = true;

		StreamUtil.SetEndianBig();
		FileStream stream = File.OpenWrite(FilePath);
		_rarc.Save(stream);
		stream.Close();

		byte[] savedArc = File.ReadAllBytes(FilePath);
		byte[] compressedArc = YAZ0.Compress(savedArc);
		File.WriteAllBytes(FilePath, compressedArc);
	}

	public void SaveTo(string path)
	{
		foreach (Cutscene cutscene in LoadedCutscenes.Values)
		{
			cutscene.SaveAll(_rarc, IsSMG1);
		}

		_rarc.KeepFileIDsSynced = true;

		StreamUtil.SetEndianBig();
		FileStream stream = File.OpenWrite(path);
		_rarc.Save(stream);
		stream.Close();

		byte[] savedArc = File.ReadAllBytes(path);
		byte[] compressedArc = YAZ0.Compress(savedArc);
		File.WriteAllBytes(path, compressedArc);

		FilePath = path;
	}
}
