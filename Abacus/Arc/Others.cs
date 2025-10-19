using System;
using System.Collections.Generic;

using Hack.io;
using Hack.io.BCSV;
using Hack.io.RARC;
using Hack.io.YAZ0;
using Hack.io.Class;
using Hack.io.Utility;



namespace Abacus;

public class OtherUtility
{
	private static uint ODT_FIELD_ENGLISH = 0x9FF8A861;
	private static uint ODT_FIELD_JAPANESE = 0xABE181E4;

	private static uint PMODT_FIELD_MODELNAME = 0xFF974D34;

	private static uint MBI_MULTIBGMNAME = 0x8F913C1A;
	private static uint MBI_BGMNAME = 0x59435F53;
	private static uint MBI_STREAMNAME = 0x2F985A4B;

	private static uint MAP_GP_POSNAME = 0x4BD5EEDF;


	public string GamePath { get; private set; } = "";
	public string GalaxyPath { get; private set; } = "";
	public string LoadedObject { get; private set; } = "";

	public BidirectionalDictionary<string, string> ObjDataTableList { get; private set; } = new();
	public List<string> ObjDataTableEnglishNames { get; private set; } = [];
	public List<string> ProductMapObjDataTableList { get; private set; } = [];

	public List<string> MusicList { get; private set; } = [];

	public List<string> MarioAnimeList { get; private set; } = [];

	public List<string> ObjectAnimList { get; private set; } = [];

	public List<string> GeneralPosList { get; private set; } = [];



	public OtherUtility()
	{

	}

	public static RARC? TryLoadRarcYAZ0(string path)
	{
		RARC rarc = new();
		try
		{
			StreamUtil.SetEndianBig();
			if (FileUtil.LoadFileWithDecompression(path, rarc.Load, [(YAZ0.Check, YAZ0.Decompress)]) == -1)
			{
				FileStream stream = File.OpenRead(path);
				rarc.Load(stream);
				stream.Close();
			}
		}
		catch (Exception e)
		{
			Console.WriteLine($"Exception while trying to load archive: {path}!");
			Console.WriteLine(e.ToString());
			// throw new Exception($"Invalid .arc file! Are you sure that it's a Nintendo Revolution Archive? Error:\n{e.GetType()}: {e.Message}");
		}
		return rarc;
	}

	public void LoadRarcs(string path)
	{
		// Console.WriteLine(BCSV.StringToHash_JGadget("PosName"));
		string gamePath = Path.Combine(path, "..", "..", "..");
		string galaxyPath = Path.Combine(path, "..");

		string galaxyNameDemo = Path.GetFileName(path);
		string galaxyName = galaxyNameDemo.Substring(0, galaxyNameDemo.Length - 8);

		string objDataTablePath = Path.Combine(gamePath, "SystemData", "ObjNameTable.arc");
		string productMapObjDataTablePath = Path.Combine(gamePath, "ObjectData", "ProductMapObjDataTable.arc");
		string multiBgmInfoPath = Path.Combine(gamePath, "AudioRes", "Info", "MultiBgmInfo.arc");
		string marioAnimePath = Path.Combine(gamePath, "ObjectData", "MarioAnime.arc");
		string galaxyMapPath = Path.Combine(galaxyPath, galaxyName + "Map.arc");

		GamePath = gamePath;
		GalaxyPath = galaxyPath;

		LoadRarc_ObjDataTable(File.Exists(objDataTablePath) ? objDataTablePath : "./Templates/ObjNameTable.arc");
		LoadRarc_ProductMapObjDataTable(File.Exists(productMapObjDataTablePath) ? productMapObjDataTablePath : "./Templates/ProductMapObjDataTable.arc");
		LoadRarc_MultiBgmInfo(File.Exists(multiBgmInfoPath) ? multiBgmInfoPath : "./Templates/MultiBgmInfo.arc");
		LoadRarc_MarioAnime(File.Exists(marioAnimePath) ? marioAnimePath : "./Templates/MarioAnime.arc");
		if (File.Exists(galaxyMapPath))
			LoadRarc_GeneralPos(galaxyMapPath);
	}

	public void LoadRarc_ObjDataTable(string path)
	{
		ObjDataTableList = new();
		ObjDataTableEnglishNames = [];

		RARC? objDataTable = TryLoadRarcYAZ0(path);
		if (objDataTable == null)
			return;

		object? tbl_obj = objDataTable[$"ObjNameTable/ObjNameTable.tbl"];
		if (tbl_obj == null)
		{
			Console.WriteLine($"The archive {path} doesn't contain 'ObjNameTable/ObjNameTable.tbl', are you sure that it's the correct 'ObjNameTable.arc'?");
			return;
		}

		BCSV tbl = new();
		tbl.Load((MemoryStream) ((ArchiveFile) tbl_obj));

		for (int i = 0; i < tbl.EntryCount; i++)
		{
			string eng_name = (string) tbl[i][tbl[ODT_FIELD_ENGLISH]];
			string jap_name = (string) tbl[i][tbl[ODT_FIELD_JAPANESE]];
			if (eng_name != "TestStarLightReceiveSwitch") // Filter TestStarLightReceiveSwitch because it has the same japanese name and isn't allowed to have multiple keys with the same values in BidirectionalDictionary
			{
				ObjDataTableList[eng_name] = jap_name;
				ObjDataTableEnglishNames.Add(eng_name);
			}
		}
	}

	public void LoadRarc_ProductMapObjDataTable(string path)
	{
		ProductMapObjDataTableList = new();

		RARC? ProductMapObjDataTable = TryLoadRarcYAZ0(path);
		if (ProductMapObjDataTable == null)
			return;

		object? mbcsv_obj = ProductMapObjDataTable[$"ProductMapObjDataTable/ProductMapObjDataTable.bcsv"];
		if (mbcsv_obj == null)
		{
			Console.WriteLine($"The archive {path} doesn't contain 'ProductMapObjDataTable/ProductMapObjDataTable.bcsv', are you sure that it's the correct 'ProductMapObjDataTable.arc'?");
			return;
		}

		BCSV mbcsv = new();
		mbcsv.Load((MemoryStream) ((ArchiveFile) mbcsv_obj));

		for (int i = 0; i < mbcsv.EntryCount; i++)
		{
			ProductMapObjDataTableList.Add((string) mbcsv[i][mbcsv[PMODT_FIELD_MODELNAME]]);
		}
	}

	public void LoadRarc_MultiBgmInfo(string path)
	{
		MusicList = [];

		RARC? MultiBgmInfo = TryLoadRarcYAZ0(path);
		if (MultiBgmInfo == null)
			return;

		object? music_obj = MultiBgmInfo[$"MultiBgmInfo/MultiBgmInfo.bcsv"];
		if (music_obj == null)
		{
			Console.WriteLine($"The archive {path} doesn't contain 'MultiBgmInfo/MultiBgmInfo.bcsv', are you sure that it's the correct 'MultiBgmInfo.arc'?");
			return;
		}

		BCSV msic = new();
		msic.Load((MemoryStream) ((ArchiveFile) music_obj));

		for (int i = 0; i < msic.EntryCount; i++)
		{
			MusicList.Add((string) msic[i][msic[MBI_MULTIBGMNAME]]);
			MusicList.Add((string) msic[i][msic[MBI_BGMNAME]]);
			MusicList.Add((string) msic[i][msic[MBI_STREAMNAME]]);
		}
	}

	public void LoadRarc_MarioAnime(string path)
	{
		MarioAnimeList = [];

		RARC? MarioAnime = TryLoadRarcYAZ0(path);
		if (MarioAnime == null)
			return;

		foreach (string filePath in MarioAnime.Root!.Items.Keys)
		{
			if (MarioAnime.Root[filePath]! is RARC.File && (filePath.EndsWith(".bck") || filePath.EndsWith(".bca") || filePath.EndsWith(".btk") || filePath.EndsWith(".brk") || filePath.EndsWith(".btp") || filePath.EndsWith(".bpk") || filePath.EndsWith(".bpa") || filePath.EndsWith(".bva") || filePath.EndsWith(".blk") || filePath.EndsWith(".bxk") || filePath.EndsWith(".bxa")))
			{
				string name = filePath.Split(".")[0];
				if (!MarioAnimeList.Contains(name))
				{
					MarioAnimeList.Add(name);
				}
			}
		}
	}

	public void LoadRarc_ObjectAnim(string objectName)
	{
		ObjectAnimList = [];

		string path = Path.Combine(GamePath, "ObjectData", objectName + ".arc");
		string animPath = Path.Combine(GamePath, "ObjectData", objectName + "Anim.arc");

		if (!File.Exists(path))
			return;

		RARC? ObjectArc = TryLoadRarcYAZ0(path);
		if (ObjectArc == null)
			return;

		foreach (string filePath in ObjectArc.Root!.Items.Keys)
		{
			if (ObjectArc.Root[filePath]! is RARC.File && (filePath.EndsWith(".bck") || filePath.EndsWith(".bca") || filePath.EndsWith(".btk") || filePath.EndsWith(".brk") || filePath.EndsWith(".btp") || filePath.EndsWith(".bpk") || filePath.EndsWith(".bpa") || filePath.EndsWith(".bva") || filePath.EndsWith(".blk") || filePath.EndsWith(".bxk") || filePath.EndsWith(".bxa")))
			{
				string name = filePath.Split(".")[0];
				if (!ObjectAnimList.Contains(name))
				{
					ObjectAnimList.Add(name);
				}
			}
		}


		LoadedObject = objectName;
		if (!File.Exists(animPath))
			return;

		RARC? ObjectAnim = TryLoadRarcYAZ0(animPath);
		if (ObjectAnim == null)
			return;

		foreach (string filePath in ObjectAnim.Root!.Items.Keys)
		{
			if (ObjectAnim.Root[filePath]! is RARC.File && (filePath.EndsWith(".bck") || filePath.EndsWith(".bca") || filePath.EndsWith(".btk") || filePath.EndsWith(".brk") || filePath.EndsWith(".btp") || filePath.EndsWith(".bpk") || filePath.EndsWith(".bpa") || filePath.EndsWith(".bva") || filePath.EndsWith(".blk") || filePath.EndsWith(".bxk") || filePath.EndsWith(".bxa")))
			{
				string name = filePath.Split(".")[0];
				if (!ObjectAnimList.Contains(name))
				{
					ObjectAnimList.Add(name);
				}
			}
		}
	}

	private void LoadGeneralPos_ExtractValues(BCSV genPos)
	{
		for (int i = 0; i < genPos.EntryCount; i++)
		{
			string posName = (string) genPos[i][genPos[MAP_GP_POSNAME]];
			if (!GeneralPosList.Contains(posName))
				GeneralPosList.Add(posName);
		}
	}

	private void LoadGeneralPos_ExtractBCSV(RARC map, string layer)
	{
		BCSV loader = new();

		object? posInfo = map[$"Stage/jmp/GeneralPos/{layer}/GeneralPosInfo"];
		if (posInfo != null)
		{
			loader.Load((MemoryStream) ((ArchiveFile) posInfo));
			LoadGeneralPos_ExtractValues(loader);
		}
	}

	private void LoadGeneralPos_LoadAllBCSVFromRarc(RARC map)
	{
		LoadGeneralPos_ExtractBCSV(map, "Common");
		LoadGeneralPos_ExtractBCSV(map, "LayerA");
		LoadGeneralPos_ExtractBCSV(map, "LayerB");
		LoadGeneralPos_ExtractBCSV(map, "LayerC");
		LoadGeneralPos_ExtractBCSV(map, "LayerD");
		LoadGeneralPos_ExtractBCSV(map, "LayerE");
		LoadGeneralPos_ExtractBCSV(map, "LayerF");
		LoadGeneralPos_ExtractBCSV(map, "LayerG");
		LoadGeneralPos_ExtractBCSV(map, "LayerH");
		LoadGeneralPos_ExtractBCSV(map, "LayerI");
		LoadGeneralPos_ExtractBCSV(map, "LayerJ");
		LoadGeneralPos_ExtractBCSV(map, "LayerK");
		LoadGeneralPos_ExtractBCSV(map, "LayerL");
		LoadGeneralPos_ExtractBCSV(map, "LayerM");
		LoadGeneralPos_ExtractBCSV(map, "LayerN");
		LoadGeneralPos_ExtractBCSV(map, "LayerO");
		LoadGeneralPos_ExtractBCSV(map, "LayerP");
	}

	public void LoadRarc_GeneralPos(string path)
	{
		GeneralPosList = [];

		RARC? map = TryLoadRarcYAZ0(path);
		if (map == null)
			return;


		LoadGeneralPos_LoadAllBCSVFromRarc(map);
	}
}
