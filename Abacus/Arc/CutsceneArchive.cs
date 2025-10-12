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
	public Dictionary<string, Cutscene> LoadedCutscenes { get; private set; } = new Dictionary<string, Cutscene>();
	public string? SelectedCutsceneName { get; private set; } = null;
	public string FilePath { get; private set; } = "";
	public bool IsYazCompressed { get; private set; } = true;
	private RARC _rarc;


	private CutsceneArchive(string path)
	{
		_rarc = new();
		FilePath = path;
	}


	private void LoadArchiveCutscenes()
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

		if (ca._rarc.Root.Name != "Stage")
			return CutsceneArchiveReadWrapper.Error("The arc doesn't have 'Stage' in the root!");
		if (!ca._rarc.ItemExists("Stage/csv"))
			return CutsceneArchiveReadWrapper.Error("The arc doesn't have 'Stage/csv'!");

		ca.LoadArchiveCutscenes();
		return CutsceneArchiveReadWrapper.Ok(ca);
	}



	public void LoadCutscene(string cutsceneName)
	{
		if (!LoadedCutscenes.ContainsKey(cutsceneName))
		{
			object? rarcFileObj = _rarc[$"Stage/csv/{cutsceneName}Time.bcsv"];
			if (rarcFileObj == null) return;

			LoadedCutscenes[cutsceneName] = Cutscene.NewCutsceneFromRarc(_rarc, cutsceneName);
		}

		SelectedCutsceneName = cutsceneName;
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

	public void RenameSelectedCutscene(string newCutsceneName)
	{
		string oldCutsceneName = GetSelectedCutsceneName();

		LoadedCutscenes[newCutsceneName] = LoadedCutscenes[oldCutsceneName];
		LoadedCutscenes.Remove(oldCutsceneName);
		LoadedCutscenes[newCutsceneName].CutsceneName = newCutsceneName;

		int i = CutsceneNames.IndexOf(oldCutsceneName);
		CutsceneNames.RemoveAt(i);
		CutsceneNames.Insert(i, newCutsceneName);


		object? _rarcPath = _rarc["Stage/csv"];

		if (_rarcPath == null)
			throw new Exception("Couldn't get the csv path in the arc file!");

		RARC.Directory csv = (RARC.Directory) _rarcPath;

		csv.RenameItem($"{oldCutsceneName}Time.bcsv", $"{newCutsceneName}Time.bcsv");
		csv.RenameItem($"{oldCutsceneName}Player.bcsv", $"{newCutsceneName}Player.bcsv");
		csv.RenameItem($"{oldCutsceneName}Wipe.bcsv", $"{newCutsceneName}Wipe.bcsv");
		csv.RenameItem($"{oldCutsceneName}Sound.bcsv", $"{newCutsceneName}Sound.bcsv");
		csv.RenameItem($"{oldCutsceneName}Action.bcsv", $"{newCutsceneName}Action.bcsv");
		csv.RenameItem($"{oldCutsceneName}Camera.bcsv", $"{newCutsceneName}Camera.bcsv");
		csv.RenameItem($"{oldCutsceneName}SubPart.bcsv", $"{newCutsceneName}SubPart.bcsv");


		SelectedCutsceneName = newCutsceneName;
	}

	public void DeleteSelectedCutscene()
	{
		string cutsceneName = GetSelectedCutsceneName();

		LoadedCutscenes.Remove(cutsceneName);
		CutsceneNames.RemoveAt(CutsceneNames.IndexOf(cutsceneName));


		object? _rarcPath = _rarc["Stage/csv"];

		if (_rarcPath == null)
			throw new Exception("Couldn't get the csv path in the arc file!");

		RARC.Directory csv = (RARC.Directory) _rarcPath;

		csv.Items.Remove($"{cutsceneName}Time.bcsv");
		csv.Items.Remove($"{cutsceneName}Player.bcsv");
		csv.Items.Remove($"{cutsceneName}Wipe.bcsv");
		csv.Items.Remove($"{cutsceneName}Sound.bcsv");
		csv.Items.Remove($"{cutsceneName}Action.bcsv");
		csv.Items.Remove($"{cutsceneName}Camera.bcsv");
		csv.Items.Remove($"{cutsceneName}SubPart.bcsv");


		SelectedCutsceneName = null;
	}

	public void Save()
	{
		foreach (Cutscene cutscene in LoadedCutscenes.Values)
		{
			cutscene.SaveAll(_rarc);
		}

		FileStream stream = File.OpenWrite(FilePath);
		_rarc.Save(stream);
		stream.Close();
	}

	public void SaveTo(string path)
	{


		foreach (Cutscene cutscene in LoadedCutscenes.Values)
		{
			cutscene.SaveAll(_rarc);
		}

		StreamUtil.SetEndianBig();
		FileStream stream = File.OpenWrite(path);
		_rarc.Save(stream);
		stream.Close();
	}
}
