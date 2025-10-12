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
		// IReadOnlyList<IStorageFolder> folderPaths;
		// if (!string.IsNullOrEmpty(folderPath)) {
		// 	folderPaths = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false, SuggestedStartLocation = await sp.TryGetFolderFromPathAsync(folderPath) });
		// } else {
		// 	folderPaths = await sp.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
		// }

		IStorageFile? file = await sp.SaveFilePickerAsync(new FilePickerSaveOptions
		{
			Title = "Save Demo .arc file",
			SuggestedFileName = "DemoMyCutscene.arc",
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

	public static async Task<ButtonResult> SendMessage(MainWindow win, string title, string body, ButtonEnum btnType)
	{
		return await MessageBoxManager.GetMessageBoxStandard(title, body, btnType).ShowWindowDialogAsync(win);
	}

// 	public static async Task<string?> AskBCSVName(MainWindow Win, string DefaultName)
// 	{
// 		CutsceneNameDialog CNDialog = new(DefaultName);
// 		await CNDialog.ShowDialog(Win);
//
// 		return CNDialog.CutsceneName;
// 	}

	public static async Task<string?> AskName(MainWindow win, string title, string body, string? watermark, string? defaultName)
	{
		CutsceneNameDialog cnd = new(title, body, watermark, defaultName);
		await cnd.ShowDialog(win);

		return cnd.CutsceneName;
	}

}
