namespace Abacus;

/// <summary>
/// All the Hashes of all BCSVs cutscene-related stored here.
/// </summary>
public static class Hashes
{
    public const uint PART_NAME = 0x4AAA853E;

    public static class TimeHashes
    {
        public const uint TOTAL_STEP = 0x55093410;
        public const uint SUSPEND_FLAG = 0x99253DA8;
        public const uint WAIT_USER_INPUT_FLAG = 0x1FE4C336;
    }

    public static class PlayerHashes
    {
        public const uint POS_NAME = 0x4BD5EEDF;
        public const uint BCK_NAME = 0x5253CDD5;
        public const uint VISIBLE = 0x7F0A8852;
    }

    public static class SubPartHashes
    {
        public const uint SUB_PART_NAME = 0xF893AB5E;
        public const uint SUB_PART_TOTAL_STEP = 0x71E36F3D;
        public const uint MAIN_PART_NAME = 0xED481617;
        public const uint MAIN_PART_STEP = 0xED4AA258;
    }

    public static class WipeHashes
    {
        public const uint WIPE_NAME = 0xC4C78792;
        public const uint WIPE_TYPE = 0xC4CA9C41;
        public const uint WIPE_FRAME = 0xD3C03D46;

        // TODO: Add WipeNames and WipeTypes
    }

    public static class SoundHashes
    {
        public const uint BGM_NAME = 0x000104A8; // BGM = BGM_NAME More understandable
        public const uint SYSTEM_SE = 0x79A31101;
        public const uint ACTION_SE = 0xA1233748;
        public const uint RETURN_BGM = 0x40359958;
        public const uint BGM_WIPEOUT_FRAME = 0xA8C280CE;
        public const uint ALL_SOUND_STOP_FRAME = 0xF554121D;
    }

    public static class ActionHashes
    {
        public const uint CAST_NAME = 0x05F0168A;
        public const uint CAST_ID = 0x77E19CBA;
        public const uint ACTION_TYPE = 0xE53352B0;
        public const uint POS_NAME = 0x4BD5EEDF;
        public const uint ANIM_NAME = 0xD46BA45C;
    }

    public static class CameraHashes
    {
        public const uint CAMERA_TARGET_NAME = 0xC60428E1;
        public const uint CAMERA_TARGET_CAST_ID = 0x42DB2170;
        public const uint ANIM_CAMERA_NAME = 0x61BA6061;
        public const uint ANIM_CAMERA_START_FRAME = 0x71570F81;
        public const uint ANIM_CAMERA_END_FRAME = 0x3ACC8C28;
        public const uint IS_CONTINUOUS = 0xA1CF23F9;
    }
}
