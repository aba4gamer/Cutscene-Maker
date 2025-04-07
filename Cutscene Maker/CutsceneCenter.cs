using static Abacus.Hashes;
using Hack.io.BCSV;
using System.Buffers.Binary;
using Hack.io.Utility;
using Microsoft.VisualBasic; // From Hack.io libraries

namespace Abacus;
/*
    The idea is to give a lot of useful functions to make the UI development easier including the organization of all the BCSVs.
    This consists of that a Cutscene has a list of Parts that are connected to all the BCSVs using its PartName.
    
    Note for the UI designer: I think you can directly load every parameter from the respective class of each Part, you don't have to search inside the BCSVs.
*/
/// <summary>
/// Idk if this should be static.
/// </summary>
public class Cutscene(string CutsceneName)
{
    /// <summary>
    /// Idk if in-game does but here the name doesn't require "Demo" before the CutsceneName... But just in case add "Demo" to the name.
    /// </summary>
    public string CutsceneName = CutsceneName;
    /// <summary>
    /// Feel free to use this list.
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
    /// Load all BCSVs from a folder (NOT RARC!), only when you're sure that they exist!!!
    /// </summary>
    public void LoadAll(string folderPath)
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

            // Suffering starts here.
            for (int j = 0; j < SubPartBCSV.EntryCount; j++)
            {
                string SubPartName = (string)SubPartBCSV[j][SubPartBCSV[SubPartHashes.SUB_PART_NAME]];
                string MainPartName = (string)SubPartBCSV[j][SubPartBCSV[SubPartHashes.MAIN_PART_NAME]];

                if (MainPartName == PartName)
                {
                    if (Parts[i].SubPartEntries == null)
                        Parts[i].SubPartEntries = [];
                    SubPart subPart = new(SubPartName, MainPartName)
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
        if (GetPartNameIndex(PlayerBCSV, PartName) is int iPlayer && iPlayer != -1) // Do I need the "&& iPlayer != -1"?)
        {
            part.PlayerEntry = new()
            {
                PosName = (string)PlayerBCSV[iPlayer][PlayerBCSV[PlayerHashes.POS_NAME]],
                BckName = (string)PlayerBCSV[iPlayer][PlayerBCSV[PlayerHashes.BCK_NAME]],
                Visible = (int)PlayerBCSV[iPlayer][PlayerBCSV[PlayerHashes.VISIBLE]]
            };
        }
        if (GetPartNameIndex(WipeBCSV, PartName) is int iWipe && iWipe != -1)
        {
            part.WipeEntry = new()
            {
                WipeName = (string)WipeBCSV[iWipe][WipeBCSV[WipeHashes.WIPE_NAME]],
                WipeType = (int)WipeBCSV[iWipe][WipeBCSV[WipeHashes.WIPE_TYPE]],
                WipeFrame = (int)WipeBCSV[iWipe][WipeBCSV[WipeHashes.WIPE_FRAME]]
            };
        }
        if (GetPartNameIndex(SoundBCSV, PartName) is int iSound && iSound != -1)
        {
            part.SoundEntry = new()
            {
                Bgm = (string)SoundBCSV[iSound][SoundBCSV[SoundHashes.BGM_NAME]],
                SystemSe = (string)SoundBCSV[iSound][SoundBCSV[SoundHashes.SYSTEM_SE]],
                ActionSe = (string)SoundBCSV[iSound][SoundBCSV[SoundHashes.ACTION_SE]],
                ReturnBgm = (int)SoundBCSV[iSound][SoundBCSV[SoundHashes.RETURN_BGM]],
                WipeOutFrame = (int)SoundBCSV[iSound][SoundBCSV[SoundHashes.BGM_WIPEOUT_FRAME]],
                AllSoundStopFrame = (int)SoundBCSV[iSound][SoundBCSV[SoundHashes.ALL_SOUND_STOP_FRAME]]
            };
        }
        if (GetPartNameIndex(ActionBCSV, PartName) is int iAction && iAction != -1)
        {
            part.ActionEntry = new()
            {
                CastName = (string)ActionBCSV[iAction][ActionBCSV[ActionHashes.CAST_NAME]],
                CastID = (int)ActionBCSV[iAction][ActionBCSV[ActionHashes.CAST_ID]],
                ActionType = (int)ActionBCSV[iAction][ActionBCSV[ActionHashes.ACTION_TYPE]],
                PosName = (string)ActionBCSV[iAction][ActionBCSV[ActionHashes.POS_NAME]],
                AnimName = (string)ActionBCSV[iAction][ActionBCSV[ActionHashes.ANIM_NAME]]
            };
        }
        if (GetPartNameIndex(CameraBCSV, PartName) is int iCamera && iCamera != -1)
        {
            part.CameraEntry = new()
            {
                CameraTargetName = (string)CameraBCSV[iCamera][CameraBCSV[CameraHashes.CAMERA_TARGET_NAME]],
                CameraTargetCastID = (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.CAMERA_TARGET_CAST_ID]],
                AnimCameraName = (string)CameraBCSV[iCamera][CameraBCSV[CameraHashes.ANIM_CAMERA_NAME]],
                AnimCameraStartFrame = (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.ANIM_CAMERA_START_FRAME]],
                AnimCameraEndFrame = (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.ANIM_CAMERA_END_FRAME]],
                IsContinuous = (int)CameraBCSV[iCamera][CameraBCSV[CameraHashes.IS_CONTINUOUS]]
            };
        }
    }

    protected static void LoadBCSV(string filePath, BCSV bcsv)
    {
        byte[] fileBytes = File.ReadAllBytes(filePath);
        StreamUtil.SetEndianBig(); // StreamUtil my beloved. This saved me from manually reverse every byte.
        using MemoryStream stream = new(fileBytes);
        bcsv.Load(stream);
    }
    protected static void SaveBCSV(string filePath, BCSV bcsv)
    {
        StreamUtil.SetEndianBig();
        using MemoryStream stream = new();
        bcsv.Save(stream);
        File.WriteAllBytes(filePath, stream.ToArray());
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
    public void SaveAll(string folderPath)
    {
        TimeBCSV.Clear();
        SubPartBCSV.Clear();
        PlayerBCSV.Clear();
        WipeBCSV.Clear();
        SoundBCSV.Clear();
        ActionBCSV.Clear();
        CameraBCSV.Clear();

        foreach (Part part in Parts)
        {
            TimeBCSV.Add(part.TimeEntry.CreateEntryAndSave(part.PartName));
            if (part.PlayerEntry != null)
                PlayerBCSV.Add(part.PlayerEntry.CreateEntryAndSave(part.PartName));
            if (part.WipeEntry != null)
                WipeBCSV.Add(part.WipeEntry.CreateEntryAndSave(part.PartName));
            if (part.SoundEntry != null)
                SoundBCSV.Add(part.SoundEntry.CreateEntryAndSave(part.PartName));
            if (part.ActionEntry != null)
                ActionBCSV.Add(part.ActionEntry.CreateEntryAndSave(part.PartName));
            if (part.CameraEntry != null)
                CameraBCSV.Add(part.CameraEntry.CreateEntryAndSave(part.PartName));
            if (part.SubPartEntries != null)
            {
                SubPartBCSV.AddRange(part.SubPartEntries.Select(p => p.CreateEntryAndSave(part.PartName)).ToList()); // Yipee, first time using LINQ.
            }
        }
        try
        {
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "Time.bcsv"), TimeBCSV);
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "Player.bcsv"), PlayerBCSV);
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "Wipe.bcsv"), WipeBCSV);
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "Sound.bcsv"), SoundBCSV);
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "Action.bcsv"), ActionBCSV);
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "Camera.bcsv"), CameraBCSV);
            SaveBCSV(Path.Combine(folderPath, CutsceneName + "SubPart.bcsv"), SubPartBCSV);

            Console.WriteLine("Files saved succesfully");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Something went wrong with saving!\nCheck this: {e.Message}");
        }
    }
    public void CreateAllNewFromTemplate()
    {

    }
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

// You maybe ask why aren't these entries objects from BCSV.Entry. With this you can easily access every property instead of getting it using a hash for every property like I did before. 
#region EntryClasses 

public class Time
{
    public int TotalStep = 0;
    public int SuspendFlag = 0;
    public int WaitUserInputFlag = 0;
    public BCSV.Entry CreateEntryAndSave(string PartName)
    {
        BCSV.Entry entry = new();
        entry.Add(PART_NAME, PartName);
        entry.Add(TimeHashes.TOTAL_STEP, TotalStep);
        entry.Add(TimeHashes.SUSPEND_FLAG, SuspendFlag);
        entry.Add(TimeHashes.WAIT_USER_INPUT_FLAG, WaitUserInputFlag);
        return entry;
    }
}

/// <summary>
/// This is the best way to manage this class
/// </summary>
public class SubPart(string SubPartName, string MainPartName) : ICommonEntries
{

    public string SubPartName = SubPartName;
    public int SubPartTotalStep = 0; // Ehh, this maybe should be -1 instead.
    public string MainPartName = MainPartName;
    public int MainPartStep = 0;

    public Player? PlayerEntry { get; set; }
    public Wipe? WipeEntry { get; set; }
    public Sound? SoundEntry { get; set; }
    public Action? ActionEntry { get; set; }
    public Camera? CameraEntry { get; set; }
    public BCSV.Entry CreateEntryAndSave(string PartName)
    {
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
    public int Visible = 0;
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