using Abacus;
using Hack.io.BCSV;

Cutscene cut = new("DemoWarpHoleEntrance");
cut.LoadAll("Test");

PrintAll(cut);
Console.ReadLine();
Console.Clear();
cut.Parts[0].PartName = "Start";
cut.SaveAll("Test");
cut.LoadAll("Test");
PrintAll(cut);

static void PrintAll(Cutscene cut)
{
    Console.WriteLine("---------------------------");

    foreach (var part in cut.Parts)
    {
        Console.WriteLine($"Part Name: {part.PartName}");

        if (part.TimeEntry != null)
        {
            Console.WriteLine($"- Time");
            Console.WriteLine($"  - TotalStep: {part.TimeEntry.TotalStep}");
            Console.WriteLine($"  - SuspendFlag: {part.TimeEntry.SuspendFlag}");
            Console.WriteLine($"  - WaitUserInputFlag: {part.TimeEntry.WaitUserInputFlag}");
        }

        PrintProperties(part, indentLevel: 1);

        if (part.SubPartEntries != null)
        {
            Console.WriteLine($"- SubParts:");
            foreach (SubPart subPart in part.SubPartEntries)
            {
                Console.WriteLine($"  - SubPartName: {subPart.SubPartName}");
                Console.WriteLine($"    - SubPartTotalStep: {subPart.SubPartTotalStep}");
                Console.WriteLine($"    - MainPartStep: {subPart.MainPartStep}");
                PrintProperties(subPart, indentLevel: 2);
            }
        }

        Console.WriteLine("---------------------------");
    }

    static void PrintProperties(ICommonEntries part, int indentLevel)
    {
        string indent = new string(' ', indentLevel * 2);

        if (part.PlayerEntry != null)
        {
            Console.WriteLine($"{indent}- Player");
            Console.WriteLine($"{indent}  - PosName: {part.PlayerEntry.PosName}");
            Console.WriteLine($"{indent}  - BckName: {part.PlayerEntry.BckName}");
            Console.WriteLine($"{indent}  - Visible: {part.PlayerEntry.Visible}");
        }

        if (part.WipeEntry != null)
        {
            Console.WriteLine($"{indent}- Wipe");
            Console.WriteLine($"{indent}  - WipeName: {part.WipeEntry.WipeName}");
            Console.WriteLine($"{indent}  - WipeType: {part.WipeEntry.WipeType}");
            Console.WriteLine($"{indent}  - WipeFrame: {part.WipeEntry.WipeFrame}");
        }

        if (part.ActionEntry != null)
        {
            Console.WriteLine($"{indent}- Action");
            Console.WriteLine($"{indent}  - CastName: {part.ActionEntry.CastName}");
            Console.WriteLine($"{indent}  - CastID: {part.ActionEntry.CastID}");
            Console.WriteLine($"{indent}  - ActionType: {part.ActionEntry.ActionType}");
            Console.WriteLine($"{indent}  - PosName: {part.ActionEntry.PosName}");
            Console.WriteLine($"{indent}  - AnimName: {part.ActionEntry.AnimName}");
        }

        if (part.CameraEntry != null)
        {
            Console.WriteLine($"{indent}- Camera");
            Console.WriteLine($"{indent}  - CameraTargetName: {part.CameraEntry.CameraTargetName}");
            Console.WriteLine($"{indent}  - CameraTargetCastID: {part.CameraEntry.CameraTargetCastID}");
            Console.WriteLine($"{indent}  - AnimCameraName: {part.CameraEntry.AnimCameraName}");
            Console.WriteLine($"{indent}  - AnimCameraStartFrame: {part.CameraEntry.AnimCameraStartFrame}");
            Console.WriteLine($"{indent}  - AnimCameraEndFrame: {part.CameraEntry.AnimCameraEndFrame}");
            Console.WriteLine($"{indent}  - IsContinuous: {part.CameraEntry.IsContinuous}");
        }
    }
}