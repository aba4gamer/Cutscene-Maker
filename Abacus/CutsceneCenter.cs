using static Abacus.Hashes;

using Hack.io.BCSV;
using Hack.io.RARC;
using Hack.io.Class;
using Hack.io.Utility;




namespace Abacus;
/*
	The idea is to give a lot of useful functions to make the UI development easier including the organization of all the BCSVs.
	This consists of that a Cutscene has a list of Parts that are connected to all the BCSVs using its PartName.

	Note for the UI designer: I think you can directly load every parameter from the respective class of each Part, you don't have to search inside the BCSVs.
*/
/// <summary>
/// The main Cutscene that contains a lot of different Parts. Use the static functions to create one.
/// </summary>
public class Cutscene
{
	protected Cutscene(string CutsceneName)
	{
		this.CutsceneName = CutsceneName;
	}
	/// <summary>
	/// Idk if in-game does but here the name doesn't require "Demo" before the CutsceneName... But just in case add "Demo" to the name.
	/// </summary>
	public string CutsceneName;
	// public string FolderPath; Hmmm, not right now.

	/// <summary>
	/// Feel free to use this list however you want.
	/// </summary>
	public List<Part> Parts = [];

	/// <summary>
	/// The most important BCSV stored here.
	/// </summary>
	public BCSV TimeBCSV = new();
	public BCSV SubPartBCSV = new();
	public BCSV PlayerBCSV = new();
	public BCSV WipeBCSV = new();
	public BCSV SoundBCSV = new();
	public BCSV ActionBCSV = new();
	public BCSV CameraBCSV = new();
	/// <summary>
	/// Load all BCSVs from an Arc file. You can use this function to reload a Cutscene if you know the folder where it is.
	/// </summary>
	public void LoadAllFromRarc(RARC rarc, bool IsSMG1) // Since NewCutsceneFromFiles() exists idk which accesibility modificator use so I'll keep it in public.
	{
		try
		{
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "Time", TimeBCSV);
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "Player", PlayerBCSV);
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "Wipe", WipeBCSV);
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "Sound", SoundBCSV);
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "Action", ActionBCSV);
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "Camera", CameraBCSV);
			LoadOrDefault(rarc, IsSMG1, CutsceneName, "SubPart", SubPartBCSV);

            Console.WriteLine($"[Abacus] '{CutsceneName}' loaded succesfully!");
		}
		catch (Exception e)
		{
			Console.WriteLine($"[Abacus] Something went wrong with loading!\nCheck this: {e.Message}");
		}

		Parts.Clear();
		for (int i = 0; i < TimeBCSV.EntryCount; i++)
		{
			// This is easy, TimeBCSV is the most important one and I'll get the Parts from there. This means that the parts that aren't here won't be loaded to the program.
			string PartName = (string)TimeBCSV[i][TimeBCSV[PART_NAME]];

			Parts.Add(new Part(PartName));
			Parts[i].TimeEntry.TotalStep = (int)TimeBCSV[i][TimeBCSV[TimeHashes.TOTAL_STEP]];
			Parts[i].TimeEntry.SuspendFlag = (int)TimeBCSV[i][TimeBCSV[TimeHashes.SUSPEND_FLAG]];
			if (!IsSMG1)
				Parts[i].TimeEntry.WaitUserInputFlag = (int)TimeBCSV[i][TimeBCSV[TimeHashes.WAIT_USER_INPUT_FLAG]];

			FillProperties(Parts[i], PartName); // Load every property into every Entry into every Part.

			for (int j = 0; j < SubPartBCSV.EntryCount; j++)
			{
				string SubPartName = (string)SubPartBCSV[j][SubPartBCSV[SubPartHashes.SUB_PART_NAME]];
				string MainPartName = (string)SubPartBCSV[j][SubPartBCSV[SubPartHashes.MAIN_PART_NAME]];

				if (MainPartName == PartName)
				{
					if (Parts[i].SubPartEntries == null)
						Parts[i].SubPartEntries = [];
					SubPart subPart = new(SubPartName)
					{
						SubPartTotalStep = (int)SubPartBCSV[j][SubPartBCSV[SubPartHashes.SUB_PART_TOTAL_STEP]],
						MainPartStep = (int)SubPartBCSV[j][SubPartBCSV[SubPartHashes.MAIN_PART_STEP]]
					};
					FillProperties(subPart, SubPartName);
					Parts[i].SubPartEntries!.Add(subPart);
				}
			}
		}
	}

    /// <summary>
    /// Load all BCSVs from a folder. You can use this function to reload a Cutscene if you know the folder where it is.
    /// </summary>
    public void LoadAll(string folderPath) // Since NewCutsceneFromFiles() exists idk which accesibility modificator use so I'll keep it in public.
	{
		try
		{
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "Time.bcsv"), TimeBCSV);
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "Player.bcsv"), PlayerBCSV);
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "Wipe.bcsv"), WipeBCSV);
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "Sound.bcsv"), SoundBCSV);
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "Action.bcsv"), ActionBCSV);
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "Camera.bcsv"), CameraBCSV);
			LoadBCSV(Path.Combine(folderPath, CutsceneName + "SubPart.bcsv"), SubPartBCSV);

			Console.WriteLine("Files loaded succesfully!");
		}
		catch (Exception e)
		{
			Console.WriteLine($"Something went wrong with loading!\nCheck this: {e.Message}");
		}

		Parts.Clear();
		for (int i = 0; i < TimeBCSV.EntryCount; i++)
		{
			// This is easy, TimeBCSV is the most important one and I'll get the Parts from there. This means that the parts that aren't here won't be loaded to the program.
			string PartName = (string)TimeBCSV[i][TimeBCSV[PART_NAME]];

			Parts.Add(new Part(PartName));
			Parts[i].TimeEntry.TotalStep = (int)TimeBCSV[i][TimeBCSV[TimeHashes.TOTAL_STEP]];
			Parts[i].TimeEntry.SuspendFlag = (int)TimeBCSV[i][TimeBCSV[TimeHashes.SUSPEND_FLAG]];
			Parts[i].TimeEntry.WaitUserInputFlag = (int)TimeBCSV[i][TimeBCSV[TimeHashes.WAIT_USER_INPUT_FLAG]];

			FillProperties(Parts[i], PartName); // Load every property into every Entry into every Part.

			for (int j = 0; j < SubPartBCSV.EntryCount; j++)
			{
				string SubPartName = (string)SubPartBCSV[j][SubPartBCSV[SubPartHashes.SUB_PART_NAME]];
				string MainPartName = (string)SubPartBCSV[j][SubPartBCSV[SubPartHashes.MAIN_PART_NAME]];

				if (MainPartName == PartName)
				{
					if (Parts[i].SubPartEntries == null)
						Parts[i].SubPartEntries = [];
					SubPart subPart = new(SubPartName)
					{
						SubPartTotalStep = (int)SubPartBCSV[j][SubPartBCSV[SubPartHashes.SUB_PART_TOTAL_STEP]],
						MainPartStep = (int)SubPartBCSV[j][SubPartBCSV[SubPartHashes.MAIN_PART_STEP]]
					};
					FillProperties(subPart, SubPartName);
					Parts[i].SubPartEntries!.Add(subPart);
				}
			}
		}
	}

	protected void FillProperties(ICommonEntries part, string PartName)
	{
		int iPlayer = GetPartNameIndex(PlayerBCSV, PartName);
		if (iPlayer != -1)
		{
			part.PlayerEntry = new()
			{
				PosName = PlayerBCSV.ContainsField(PlayerHashes.POS_NAME) ? (string)PlayerBCSV[iPlayer][PlayerBCSV[PlayerHashes.POS_NAME]] : "",
				BckName = PlayerBCSV.ContainsField(PlayerHashes.BCK_NAME) ? (string)PlayerBCSV[iPlayer][PlayerBCSV[PlayerHashes.BCK_NAME]] : "",
				Visible = PlayerBCSV.ContainsField(PlayerHashes.VISIBLE) ? (int)PlayerBCSV[iPlayer][PlayerBCSV[PlayerHashes.VISIBLE]] : -1
			};
		}

		int iWipe = GetPartNameIndex(WipeBCSV, PartName);
		if (iWipe != -1)
		{
			part.WipeEntry = new()
			{
				WipeName = WipeBCSV.ContainsField(WipeHashes.WIPE_NAME) ? (string)WipeBCSV[iWipe][WipeBCSV[WipeHashes.WIPE_NAME]] : "",
				WipeType = WipeBCSV.ContainsField(WipeHashes.WIPE_TYPE) ? (int)WipeBCSV[iWipe][WipeBCSV[WipeHashes.WIPE_TYPE]] : 0,
				WipeFrame = WipeBCSV.ContainsField(WipeHashes.WIPE_FRAME) ? (int)WipeBCSV[iWipe][WipeBCSV[WipeHashes.WIPE_FRAME]] : 0
			};
		}

		int iSound = GetPartNameIndex(SoundBCSV, PartName);
		if (iSound != -1)
		{
			part.SoundEntry = new()
			{
				Bgm = SoundBCSV.ContainsField(SoundHashes.BGM_NAME) ? (string)SoundBCSV[iSound][SoundBCSV[SoundHashes.BGM_NAME]] : "",
				SystemSe = SoundBCSV.ContainsField(SoundHashes.SYSTEM_SE) ? (string)SoundBCSV[iSound][SoundBCSV[SoundHashes.SYSTEM_SE]] : "",
				ActionSe = SoundBCSV.ContainsField(SoundHashes.ACTION_SE) ? (string)SoundBCSV[iSound][SoundBCSV[SoundHashes.ACTION_SE]] : "",
				ReturnBgm = SoundBCSV.ContainsField(SoundHashes.RETURN_BGM) ? (int)SoundBCSV[iSound][SoundBCSV[SoundHashes.RETURN_BGM]] : -1,
				WipeOutFrame = SoundBCSV.ContainsField(SoundHashes.BGM_WIPEOUT_FRAME) ? (int)SoundBCSV[iSound][SoundBCSV[SoundHashes.BGM_WIPEOUT_FRAME]] : -1,
				AllSoundStopFrame = SoundBCSV.ContainsField(SoundHashes.ALL_SOUND_STOP_FRAME) ? (int)SoundBCSV[iSound][SoundBCSV[SoundHashes.ALL_SOUND_STOP_FRAME]] : -1
			};
		}

		int iAction = GetPartNameIndex(ActionBCSV, PartName);
		if (iAction != -1)
		{
			part.ActionEntry = new()
			{
				CastName = ActionBCSV.ContainsField(ActionHashes.CAST_NAME) ? (string)ActionBCSV[iAction][ActionBCSV[ActionHashes.CAST_NAME]] : "",
				CastID = ActionBCSV.ContainsField(ActionHashes.CAST_ID) ? (int)ActionBCSV[iAction][ActionBCSV[ActionHashes.CAST_ID]] : -1,
				ActionType = ActionBCSV.ContainsField(ActionHashes.ACTION_TYPE) ? (int)ActionBCSV[iAction][ActionBCSV[ActionHashes.ACTION_TYPE]] : -1,
				PosName = ActionBCSV.ContainsField(ActionHashes.POS_NAME) ? (string)ActionBCSV[iAction][ActionBCSV[ActionHashes.POS_NAME]] : "",
				AnimName = ActionBCSV.ContainsField(ActionHashes.ANIM_NAME) ? (string)ActionBCSV[iAction][ActionBCSV[ActionHashes.ANIM_NAME]] : ""
			};
		}

		int iCamera = GetPartNameIndex(CameraBCSV, PartName);
		if (iCamera != -1)
		{
			part.CameraEntry = new()
			{
				CameraTargetName = CameraBCSV.ContainsField(CameraHashes.CAMERA_TARGET_NAME) ? (string)CameraBCSV[iCamera][CameraBCSV[CameraHashes.CAMERA_TARGET_NAME]] : "",
				CameraTargetCastID = CameraBCSV.ContainsField(CameraHashes.CAMERA_TARGET_CAST_ID) ? (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.CAMERA_TARGET_CAST_ID]] : -1,
				AnimCameraName = CameraBCSV.ContainsField(CameraHashes.ANIM_CAMERA_NAME) ? (string)CameraBCSV[iCamera][CameraBCSV[CameraHashes.ANIM_CAMERA_NAME]] : "",
				AnimCameraStartFrame = CameraBCSV.ContainsField(CameraHashes.ANIM_CAMERA_START_FRAME) ? (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.ANIM_CAMERA_START_FRAME]] : -1,
				AnimCameraEndFrame = CameraBCSV.ContainsField(CameraHashes.ANIM_CAMERA_END_FRAME) ? (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.ANIM_CAMERA_END_FRAME]] : 0,
				IsContinuous = CameraBCSV.ContainsField(CameraHashes.IS_CONTINUOUS) ? (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.IS_CONTINUOUS]] : -1
			};
		}
	}

    protected static void LoadOrDefault(Archive rarc, bool IsSMG1, string DemoName, string DemoType, BCSV Result)
    {
		if (!IsSMG1)
		{
			if (rarc[$"Stage/csv/{DemoName}{DemoType}.bcsv"] is ArchiveFile af)
				Result.Load((MemoryStream)af);
			else
				LoadBCSV(Path.Combine("Templates", $"DemoTemplate{DemoType}.bcsv"), Result);
		}
		else
		{
			if (rarc.Root![$"{DemoName.ToLower()}/{DemoName.ToLower()}{DemoType.ToLower()}.bcsv"] is ArchiveFile af)
				Result.Load((MemoryStream)af);
			else
			{
				if (DemoType.ToLower() == "Time")
					LoadBCSV(Path.Combine("Templates", $"DemoSMG1TemplateTime.bcsv"), Result);
				else
					LoadBCSV(Path.Combine("Templates", $"DemoTemplate{DemoType}.bcsv"), Result);
			}
		}
    }

    protected static void LoadBCSV(string filePath, BCSV bcsv)
	{
		StreamUtil.SetEndianBig(); // StreamUtil my beloved. This saved me from manually reverse every byte.
		using FileStream stream = File.OpenRead(filePath);
		bcsv.Load(stream);
	}

	protected static void SaveBCSV(string filePath, BCSV bcsv)
	{
		StreamUtil.SetEndianBig();
		using FileStream stream = File.Create(filePath);
		bcsv.Save(stream);
	}

	/// <summary>
	/// Oh wow, this makes everything easier.
	/// Returns the index that matches with PartName on a BCSV. Otherwise -1.
	/// </summary>
	/// <param name="bcsv"></param>
	/// <param name="PartName"></param>
	/// <returns></returns>
	public static int GetPartNameIndex(BCSV bcsv, string PartName)
	{
		for (int i = 0; i < bcsv.EntryCount; i++)
		{
			if ((string)bcsv[i][bcsv[PART_NAME]] == PartName) // Yoo, this works!
			{
				return i;
			}
		}
		return -1;
	}

	public void ExportAll(string folderPath)
	{
			Directory.CreateDirectory(folderPath);

			WriteBCSVs(false);

			SaveBCSV(Path.Combine(folderPath, CutsceneName + "Time.bcsv"), TimeBCSV);
			SaveBCSV(Path.Combine(folderPath, CutsceneName + "Player.bcsv"), PlayerBCSV);
			SaveBCSV(Path.Combine(folderPath, CutsceneName + "Wipe.bcsv"), WipeBCSV);
			SaveBCSV(Path.Combine(folderPath, CutsceneName + "Sound.bcsv"), SoundBCSV);
			SaveBCSV(Path.Combine(folderPath, CutsceneName + "Action.bcsv"), ActionBCSV);
			SaveBCSV(Path.Combine(folderPath, CutsceneName + "Camera.bcsv"), CameraBCSV);
			SaveBCSV(Path.Combine(folderPath, CutsceneName + "SubPart.bcsv"), SubPartBCSV);

			Console.WriteLine($"[Abacus] '{CutsceneName}' exported to '{folderPath}'!");
	}

	private void WriteBCSVs(bool IsSMG1)
	{
		TimeBCSV.Clear();
		SubPartBCSV.Clear();
		PlayerBCSV.Clear();
		WipeBCSV.Clear();
		SoundBCSV.Clear();
		ActionBCSV.Clear();
		CameraBCSV.Clear();

		if (!IsSMG1 && !TimeBCSV.ContainsField(TimeHashes.WAIT_USER_INPUT_FLAG))
		{
			TimeBCSV = new BCSV();

			BCSV.Field PartNameField = new();
			PartNameField.HashName = PART_NAME;
			PartNameField.DataType = BCSV.DataTypes.STRING;
			PartNameField.AutoRecalc = true;
			TimeBCSV.Add(PartNameField);

			BCSV.Field TotalStepField = new();
			TotalStepField.HashName = TimeHashes.TOTAL_STEP;
			TotalStepField.DataType = BCSV.DataTypes.INT32;
			TotalStepField.AutoRecalc = true;
			TimeBCSV.Add(TotalStepField);

			BCSV.Field SuspendFlagField = new();
			SuspendFlagField.HashName = TimeHashes.SUSPEND_FLAG;
			SuspendFlagField.DataType = BCSV.DataTypes.INT32;
			SuspendFlagField.AutoRecalc = true;
			TimeBCSV.Add(SuspendFlagField);

			BCSV.Field WaitUserInputFlagField = new();
			WaitUserInputFlagField.HashName = TimeHashes.WAIT_USER_INPUT_FLAG;
			WaitUserInputFlagField.DataType = BCSV.DataTypes.INT32;
			WaitUserInputFlagField.AutoRecalc = true;
			TimeBCSV.Add(WaitUserInputFlagField);
		}
		else if (IsSMG1 && TimeBCSV.ContainsField(TimeHashes.WAIT_USER_INPUT_FLAG))
		{
			TimeBCSV = new BCSV();

			BCSV.Field PartNameField = new();
			PartNameField.HashName = PART_NAME;
			PartNameField.DataType = BCSV.DataTypes.STRING;
			PartNameField.AutoRecalc = true;
			TimeBCSV.Add(PartNameField);

			BCSV.Field TotalStepField = new();
			TotalStepField.HashName = TimeHashes.TOTAL_STEP;
			TotalStepField.DataType = BCSV.DataTypes.INT32;
			TotalStepField.AutoRecalc = true;
			TimeBCSV.Add(TotalStepField);

			BCSV.Field SuspendFlagField = new();
			SuspendFlagField.HashName = TimeHashes.SUSPEND_FLAG;
			SuspendFlagField.DataType = BCSV.DataTypes.INT32;
			SuspendFlagField.AutoRecalc = true;
			TimeBCSV.Add(SuspendFlagField);
		}

		foreach (Part part in Parts)
		{
			TimeBCSV.Add(part.TimeEntry.CreateEntryAndSave(part.PartName, IsSMG1));
			SaveProperties(part, part.PartName);
			if (part.SubPartEntries != null)
				foreach (SubPart subPart in part.SubPartEntries)
				{
					SubPartBCSV.Add(subPart.CreateEntryAndSave(part.PartName));
					SaveProperties(subPart, subPart.SubPartName);
				}
		}
	}

	public void SaveAll(RARC rarc, bool IsSMG1)
	{
		WriteBCSVs(IsSMG1);

		try
		{
			MemoryStream msTime = new();
			MemoryStream msPlayer = new();
			MemoryStream msWipe = new();
			MemoryStream msSound = new();
			MemoryStream msAction = new();
			MemoryStream msCamera = new();
			MemoryStream msSubPart = new();

			RARC.File rarcTime = new();
			RARC.File rarcPlayer = new();
			RARC.File rarcWipe = new();
			RARC.File rarcSound = new();
			RARC.File rarcAction = new();
			RARC.File rarcCamera = new();
			RARC.File rarcSubPart = new();

			TimeBCSV.Save(msTime);
			PlayerBCSV.Save(msPlayer);
			WipeBCSV.Save(msWipe);
			SoundBCSV.Save(msSound);
			ActionBCSV.Save(msAction);
			CameraBCSV.Save(msCamera);
			SubPartBCSV.Save(msSubPart);

			rarcTime.Load(msTime);
			rarcPlayer.Load(msPlayer);
			rarcWipe.Load(msWipe);
			rarcSound.Load(msSound);
			rarcAction.Load(msAction);
			rarcCamera.Load(msCamera);
			rarcSubPart.Load(msSubPart);

			if (!IsSMG1)
			{
				rarc[$"Stage/csv/{CutsceneName}Time.bcsv"] = rarcTime;
				rarc[$"Stage/csv/{CutsceneName}Player.bcsv"] = rarcPlayer;
				rarc[$"Stage/csv/{CutsceneName}Wipe.bcsv"] = rarcWipe;
				rarc[$"Stage/csv/{CutsceneName}Sound.bcsv"] = rarcSound;
				rarc[$"Stage/csv/{CutsceneName}Action.bcsv"] = rarcAction;
				rarc[$"Stage/csv/{CutsceneName}Camera.bcsv"] = rarcCamera;
				rarc[$"Stage/csv/{CutsceneName}SubPart.bcsv"] = rarcSubPart;
			}
			else
			{
				rarc.Root![$"{CutsceneName}/{CutsceneName}time.bcsv"] = rarcTime;
				rarc.Root![$"{CutsceneName}/{CutsceneName}player.bcsv"] = rarcPlayer;
				rarc.Root![$"{CutsceneName}/{CutsceneName}wipe.bcsv"] = rarcWipe;
				rarc.Root![$"{CutsceneName}/{CutsceneName}sound.bcsv"] = rarcSound;
				rarc.Root![$"{CutsceneName}/{CutsceneName}action.bcsv"] = rarcAction;
				rarc.Root![$"{CutsceneName}/{CutsceneName}camera.bcsv"] = rarcCamera;
				rarc.Root![$"{CutsceneName}/{CutsceneName}subpart.bcsv"] = rarcSubPart;
			}

			Console.WriteLine($"[Abacus] '{CutsceneName}' saved in the archive succesfully!");
		}
		catch (Exception e)
		{
			Console.WriteLine($"[Abacus] Something went wrong with saving!\nCheck this: {e.Message}");
		}
	}

	private void SaveProperties(ICommonEntries part, string PartName)
	{
		if (part.PlayerEntry != null)
			PlayerBCSV.Add(part.PlayerEntry.CreateEntryAndSave(PartName));
		if (part.WipeEntry != null)
			WipeBCSV.Add(part.WipeEntry.CreateEntryAndSave(PartName));
		if (part.SoundEntry != null)
			SoundBCSV.Add(part.SoundEntry.CreateEntryAndSave(PartName));
		if (part.ActionEntry != null)
			ActionBCSV.Add(part.ActionEntry.CreateEntryAndSave(PartName));
		if (part.CameraEntry != null)
			CameraBCSV.Add(part.CameraEntry.CreateEntryAndSave(PartName));
	}

	/// <summary>
	/// Sums the total steps of each parts
	/// </summary>
	/// <returns></returns>
	public int GetMaxTotalSteps() {
		int steps = 0;

		foreach (Part part in Parts) {
			steps += part.TimeEntry.TotalStep;
		}

		return steps;
	}

	/// <summary>
	/// Gets all the total steps up to part (without counting it)
	/// </summary>
	/// <param name="Part"></param>
	/// <returns></returns>
	public int GetTotalStepsUpToPart(Part chosenPart) {
		int steps = 0;

		foreach (Part part in Parts) {
			if (part.PartName == chosenPart.PartName) break;
			steps += part.TimeEntry.TotalStep;
		}

		return steps;
	}

	/// <summary>
	/// Gets the part by name
	/// </summary>
	/// <returns></returns>
	public Part GetPartByName(string partName) {
		foreach (Part part in Parts) {
			if (part.PartName == partName)
				return part;
		}

		throw new Exception($"No part with name '{partName}'!");
	}

	/// <summary>
	/// Gets the sub part by name
	/// </summary>
	/// <returns></returns>
	public SubPart GetSubPartByName(string subPartName)
	{
		foreach (Part part in Parts) {

			foreach (SubPart subPart in part.SubPartEntries ?? [])
			{
				if (subPart.SubPartName == subPartName)
					return subPart;
			}
		}

		throw new Exception($"No sub part with name '{subPartName}'!");
	}

	/// <summary>
	/// Creates a new Cutscene from empty BCSVs.
	/// </summary>
	/// <param name="CutsceneName"></param>
	/// <returns></returns>
	public static Cutscene NewCutsceneFromTemplate(string CutsceneName)
	{
		Cutscene cut = new("DemoTemplate");
		cut.LoadAll("Templates");
		cut.CutsceneName = CutsceneName;
		return cut;
	}
	/// <summary>
	/// Returns a Cutscene from bcsv files on a folder.
	/// </summary>
	/// <param name="TimeBCSVPath"></param>
	/// <returns></returns>
	public static Cutscene NewCutsceneFromFiles(string TimeBCSVPath)
	{
		string CutsceneName = Path.GetFileName(TimeBCSVPath).Replace("Time.bcsv", "");
		string folderPath = Path.GetDirectoryName(TimeBCSVPath)!;
		Cutscene cut = new(CutsceneName);
		cut.LoadAll(folderPath);
		return cut;
	}
	/// <summary>
	/// Returns a Cutscene from bcsv files on an rarc file.
	/// </summary>
	/// <param name="TimeBCSVPath"></param>
	/// <returns></returns>
	public static Cutscene NewCutsceneFromRarc(RARC rarc, bool IsSMG1, string CutsceneName)
	{
		Cutscene cut = new(CutsceneName);
		cut.LoadAllFromRarc(rarc, IsSMG1);
		return cut;
	}

	/// <summary>
	/// This connects the same PartName across all the BCSVs with the idea of each Part being an independent object with multiple entries.
	/// </summary>
	/// <param name="PartName"></param>
	public class Part(string PartName) : ICommonEntries
	{
		public string PartName = PartName;

		// This means, don't have more than one entry per BCSV with the same PartName. Otherwise... this would be a bunch of work that I don't want to do.
		public Time TimeEntry = new();
		public Player? PlayerEntry { get; set; }
		public Wipe? WipeEntry { get; set; }
		public Sound? SoundEntry { get; set; }
		public Action? ActionEntry { get; set; }
		public Camera? CameraEntry { get; set; }
		public List<SubPart>? SubPartEntries; // I'll make an exception with SubPart.
	}
}
// You maybe ask why aren't these entries objects from BCSV.Entry. With this you can easily access every property instead of getting it using a hash for every property like I did before.
#region EntryClasses

public class Time
{
	public int TotalStep = 0;
	public int SuspendFlag = 0;
	public int WaitUserInputFlag = 0;
	public BCSV.Entry CreateEntryAndSave(string PartName, bool IsSMG1)
	{
		BCSV.Entry entry = new();
		entry.Add(PART_NAME, PartName);
		entry.Add(TimeHashes.TOTAL_STEP, TotalStep);
		entry.Add(TimeHashes.SUSPEND_FLAG, SuspendFlag);
		if (!IsSMG1)
			entry.Add(TimeHashes.WAIT_USER_INPUT_FLAG, WaitUserInputFlag);
		return entry;
	}
}

/// <summary>
/// This is the best way to manage this class
/// </summary>
public class SubPart(string SubPartName) : ICommonEntries
{

	public string SubPartName = SubPartName;
	public int SubPartTotalStep = 0; // Ehh, this maybe should be -1 instead.
	internal string MainPartName = ""; // This is a problem. This will be specified when you save but you can't get this value normally.
	public int MainPartStep = 0;

	public Player? PlayerEntry { get; set; }
	public Wipe? WipeEntry { get; set; }
	public Sound? SoundEntry { get; set; }
	public Action? ActionEntry { get; set; }
	public Camera? CameraEntry { get; set; }
	public BCSV.Entry CreateEntryAndSave(string MainPartName)
	{
		this.MainPartName = MainPartName;
		BCSV.Entry entry = new();
		entry.Add(SubPartHashes.SUB_PART_NAME, SubPartName);
		entry.Add(SubPartHashes.SUB_PART_TOTAL_STEP, SubPartTotalStep);
		entry.Add(SubPartHashes.MAIN_PART_NAME, MainPartName);
		entry.Add(SubPartHashes.MAIN_PART_STEP, MainPartStep);
		return entry;
	}
}

public class Player
{
	public string PosName = "0";
	public string BckName = "0";
	public int Visible = -1;
	public BCSV.Entry CreateEntryAndSave(string PartName)
	{
		BCSV.Entry entry = new();
		entry.Add(PART_NAME, PartName);
		entry.Add(PlayerHashes.POS_NAME, PosName);
		entry.Add(PlayerHashes.BCK_NAME, BckName);
		entry.Add(PlayerHashes.VISIBLE, Visible);
		return entry;
	}
}

public class Wipe
{
	public string WipeName = "0";
	public int WipeType = 0;
	public int WipeFrame = 0;
	public BCSV.Entry CreateEntryAndSave(string PartName)
	{
		BCSV.Entry entry = new();
		entry.Add(PART_NAME, PartName);
		entry.Add(WipeHashes.WIPE_NAME, WipeName);
		entry.Add(WipeHashes.WIPE_TYPE, WipeType);
		entry.Add(WipeHashes.WIPE_FRAME, WipeFrame);
		return entry;
	}
}

public class Sound
{
	public string Bgm = "0";
	public string SystemSe = "0";
	public string ActionSe = "0";
	public int ReturnBgm = 0;
	public int WipeOutFrame = 0;
	public int AllSoundStopFrame = 0;
	public BCSV.Entry CreateEntryAndSave(string PartName)
	{
		BCSV.Entry entry = new();
		entry.Add(PART_NAME, PartName);
		entry.Add(SoundHashes.BGM_NAME, Bgm);
		entry.Add(SoundHashes.SYSTEM_SE, SystemSe);
		entry.Add(SoundHashes.ACTION_SE, ActionSe);
		entry.Add(SoundHashes.RETURN_BGM, ReturnBgm);
		entry.Add(SoundHashes.BGM_WIPEOUT_FRAME, WipeOutFrame);
		entry.Add(SoundHashes.ALL_SOUND_STOP_FRAME, AllSoundStopFrame);
		return entry;
	}
}

public class Action
{
	public string CastName = "0";
	public int CastID = 0;
	public int ActionType = 0;
	public string PosName = "0";
	public string AnimName = "0";
	public BCSV.Entry CreateEntryAndSave(string PartName)
	{
		BCSV.Entry entry = new();
		entry.Add(PART_NAME, PartName);
		entry.Add(ActionHashes.CAST_NAME, CastName);
		entry.Add(ActionHashes.CAST_ID, CastID);
		entry.Add(ActionHashes.ACTION_TYPE, ActionType);
		entry.Add(ActionHashes.POS_NAME, PosName);
		entry.Add(ActionHashes.ANIM_NAME, AnimName);
		return entry;
	}
}

public class Camera
{
	public string CameraTargetName = "0";
	public int CameraTargetCastID = 0;
	public string AnimCameraName = "0";
	public int AnimCameraStartFrame = 0;
	public int AnimCameraEndFrame = 0;
	public int IsContinuous = 0;
	public BCSV.Entry CreateEntryAndSave(string PartName)
	{
		BCSV.Entry entry = new();
		entry.Add(PART_NAME, PartName);
		entry.Add(CameraHashes.CAMERA_TARGET_NAME, CameraTargetName);
		entry.Add(CameraHashes.CAMERA_TARGET_CAST_ID, CameraTargetCastID);
		entry.Add(CameraHashes.ANIM_CAMERA_NAME, AnimCameraName);
		entry.Add(CameraHashes.ANIM_CAMERA_START_FRAME, AnimCameraStartFrame);
		entry.Add(CameraHashes.ANIM_CAMERA_END_FRAME, AnimCameraEndFrame);
		entry.Add(CameraHashes.IS_CONTINUOUS, IsContinuous);
		return entry;
	}
}
public interface ICommonEntries // Before using SubParts I didn't have to use this...
{
	public Player? PlayerEntry { get; set; }
	public Wipe? WipeEntry { get; set; }
	public Sound? SoundEntry { get; set; }
	public Action? ActionEntry { get; set; }
	public Camera? CameraEntry { get; set; }
}
#endregion EntryClasses
