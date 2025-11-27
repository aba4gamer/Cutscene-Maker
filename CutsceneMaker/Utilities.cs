using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

using Avalonia.Platform.Storage;

using CutsceneMakerUI;




namespace CutsceneMaker;

class MsgBox
{

	public static async Task<bool> AskLoseUnsavedChanges(MainWindow Win)
	{
		return await SendMessage(Win, "Are you sure?", "Are you sure you want to create a new cutscene? All unsaved changes will be lost.", ButtonEnum.YesNo) == ButtonResult.Yes;
	}

	public static async Task<string?> AskOpenArcFile(IStorageProvider sp)
	{
		FilePickerFileType ff = new("*.arc (RARC Revolution Archive)") { Patterns = ["*.arc"] };
		IReadOnlyList<IStorageFile> filePaths = await sp.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Select a Demo.arc file to edit", FileTypeFilter = [ff], AllowMultiple = false });

		if (filePaths == null || filePaths.Count < 1) return null;
		string? localPath = filePaths[0].TryGetLocalPath();

		if (localPath == null || localPath == "") return null;
		return localPath;
	}

	public static async Task<string?> AskSaveArcFile(IStorageProvider sp, string? folderPath)
	{
		IStorageFile? file = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = "Save Demo .arc file",
			SuggestedFileName = folderPath,
			DefaultExtension = ".arc",
			ShowOverwritePrompt = true
		});

		if (file == null)
			return null;

		string? localFile = file.TryGetLocalPath();
		if (localFile == null)
			return null;

		return localFile;

		// 		if (folderPaths == null || folderPaths.Count < 1) return null;
		// 		string? localPath = folderPaths[0].TryGetLocalPath();
		//
		// 		if (localPath == null || localPath == "") return null;
		// 		return localPath;
	}

	public static async Task<string?> AskSaveBCSVFile(IStorageProvider sp, string? folderPath, string cutsceneName)
	{
		IReadOnlyList<IStorageFolder> folders = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions
		{
			Title = $"Export '{cutsceneName}' to BCSV files",
			SuggestedFileName = folderPath
		});

		if (folders.Count < 1)
			return null;

		string? localFolder = folders[0].TryGetLocalPath();
		if (localFolder == null)
			return null;

		return localFolder;
	}

	public static async Task<ButtonResult> SendMessage(MainWindow win, string title, string body, ButtonEnum btnType)
	{
		return await MessageBoxManager.GetMessageBoxStandard(title, body, btnType).ShowWindowDialogAsync(win);
	}

	public static async Task<string?> AskName(MainWindow win, string title, string body, string? watermark, string? defaultName, List<string>? disabledNames, string? disabledNameMesage)
	{
		CutsceneNameDialog cnd = new(title, body, watermark, defaultName, disabledNames, disabledNameMesage);
		await cnd.ShowDialog(win);

		return cnd.CutsceneName;
	}

	public static async Task<string?> AskOpenImportArcBCSVFile(IStorageProvider sp)
	{
		FilePickerFileType arc_bcsv = new("*.arc/*Time.bcsv (Super Mario Galaxy Cutscene)") { Patterns = ["*.arc", "*Time.bcsv", "*time.bcsv"] };
		FilePickerFileType arc = new("*.arc (RARC Revolution Archive)") { Patterns = ["*.arc"] };
		FilePickerFileType bcsv = new("*.bcsv (Binary Comma Separated Values)") { Patterns = ["*Time.bcsv", "*time.bcsv"] };
		IReadOnlyList<IStorageFile> filePaths = await sp.OpenFilePickerAsync(new FilePickerOpenOptions { Title = "Select a Demo.arc/Time.bcsv file to import", FileTypeFilter = [arc_bcsv, arc, bcsv], AllowMultiple = false });

		if (filePaths == null || filePaths.Count < 1) return null;
		string? localPath = filePaths[0].TryGetLocalPath();

		if (localPath == null || localPath == "") return null;
		return localPath;
	}

}
