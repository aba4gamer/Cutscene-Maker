using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;

using Abacus;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

using CutsceneMaker;



namespace CutsceneMakerUI;

public partial class MainWindow : Window
{

	public static MainWindow? Instance;

	public CutsceneCore Core;
	public ArchiveView ArchiveUI;
	public bool HasEdited = false;


	public MainWindow()
	{
		// Setting the instance
		Instance = this;

		// Initializing.
		InitializeComponent();
		Core = new();
		ArchiveUI = new();

		// Disabling buttons.
		BtnsLayer_MainMenu();

		// Give the grid an empty main menu.
		MainMenu.Children.Add(new MainMenu(OnClickNewArchive, OnClickOpenArchive));

		// Update the timeline's steps when the window is resized
		ClientSizeProperty.Changed.Subscribe(size =>
		{
			if (!Core.HasCutsceneSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null)
				return;

			ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
		});

		// Initialization complete.
		// Set the title and the status to ready
		Title = "CutsceneMaker - Main Menu";
		StatusText.Text = "Ready!";
	}



	#region HelperFunctions
	public void BtnsLayer_MainMenu()
	{
		SaveBtn.IsEnabled = false;
		SaveAsBtn.IsEnabled = false;
		ReloadArchiveBtn.IsEnabled = false;

		ArchiveMenu.IsEnabled = false;
		DeleteCutsceneBtn.IsEnabled = false;
		RenameCutsceneBtn.IsEnabled = false;

		CutsceneMenu.IsEnabled = false;
		SubPartBtn.IsEnabled = false;
		DeletePartBtn.IsEnabled = false;
		RenamePartBtn.IsEnabled = false;
		DeleteSubPartBtn.IsEnabled = false;
		RenameSubPartBtn.IsEnabled = false;
	}

	public void BtnsLayer_ArchiveOpen()
	{
		SaveBtn.IsEnabled = true;
		SaveAsBtn.IsEnabled = true;
		ReloadArchiveBtn.IsEnabled = true;

		ArchiveMenu.IsEnabled = true;
		DeleteCutsceneBtn.IsEnabled = false;
		RenameCutsceneBtn.IsEnabled = false;

		CutsceneMenu.IsEnabled = false;
		SubPartBtn.IsEnabled = false;
		DeletePartBtn.IsEnabled = false;
		RenamePartBtn.IsEnabled = false;
		DeleteSubPartBtn.IsEnabled = false;
		RenameSubPartBtn.IsEnabled = false;
	}

	public void BtnsLayer_CutsceneOpen()
	{
		SaveBtn.IsEnabled = true;
		SaveAsBtn.IsEnabled = true;
		ReloadArchiveBtn.IsEnabled = true;

		ArchiveMenu.IsEnabled = true;
		DeleteCutsceneBtn.IsEnabled = true;
		RenameCutsceneBtn.IsEnabled = true;

		CutsceneMenu.IsEnabled = true;
		SubPartBtn.IsEnabled = false;
		DeletePartBtn.IsEnabled = false;
		RenamePartBtn.IsEnabled = false;
		DeleteSubPartBtn.IsEnabled = false;
		RenameSubPartBtn.IsEnabled = false;
	}

	public void BtnsLayer_CutscenePartSelected()
	{
		SaveBtn.IsEnabled = true;
		SaveAsBtn.IsEnabled = true;
		ReloadArchiveBtn.IsEnabled = true;

		ArchiveMenu.IsEnabled = true;
		DeleteCutsceneBtn.IsEnabled = true;
		RenameCutsceneBtn.IsEnabled = true;

		CutsceneMenu.IsEnabled = true;
		SubPartBtn.IsEnabled = true;
		DeletePartBtn.IsEnabled = true;
		RenamePartBtn.IsEnabled = true;
		DeleteSubPartBtn.IsEnabled = false;
		RenameSubPartBtn.IsEnabled = false;
	}

	public void BtnsLayer_CutsceneSubPartSelected()
	{
		SaveBtn.IsEnabled = true;
		SaveAsBtn.IsEnabled = true;
		ReloadArchiveBtn.IsEnabled = true;

		ArchiveMenu.IsEnabled = true;
		DeleteCutsceneBtn.IsEnabled = true;
		RenameCutsceneBtn.IsEnabled = true;

		CutsceneMenu.IsEnabled = true;
		SubPartBtn.IsEnabled = true;
		DeletePartBtn.IsEnabled = true;
		RenamePartBtn.IsEnabled = true;
		DeleteSubPartBtn.IsEnabled = true;
		RenameSubPartBtn.IsEnabled = true;
	}


	public async Task<ButtonResult> MessageBox(string title, string body, ButtonEnum btn = ButtonEnum.Ok)
	{
		return await MsgBox.SendMessage(this, title, body, btn);
	}

	public async Task<bool> AskDiscardChanges()
	{
		return await MessageBox("Discard changes?", "You have unsaved changes, if you continue your progress will be lost, are you sure?", ButtonEnum.YesNo) == ButtonResult.Yes;
	}
	#endregion


	#region ActionFunctions
	// ============================
	// New Archive & Open Archive

	private void OnClickNewArchive(object? sender, RoutedEventArgs e)
	{
		Ask_NewArchive();
	}

	private void OnClickOpenArchive(object? sender, RoutedEventArgs e)
	{
		Ask_OpenArchive();
	}



	// ============================
	// Save, SaveAs & Reload

	private void OnClickSave(object? sender, RoutedEventArgs e)
	{
		Save();
	}

	private void OnClickSaveAs(object? sender, RoutedEventArgs e)
	{
		Ask_SaveAs();
	}

	private void OnReloadArchive(object? sender, RoutedEventArgs e)
	{
		Ask_ReloadArchive();
	}


	// ============================
	// Wiki & Github

	private void OnWiki(object? sender, RoutedEventArgs e)
	{
		Process.Start(new ProcessStartInfo("https://lumasworkshop.com/wiki/Cutscenes") {UseShellExecute = true} );
	}

	private void OnGitHub(object? sender, RoutedEventArgs e)
	{
		Process.Start(new ProcessStartInfo("https://github.com/aba4gamer/Cutscene-Maker") {UseShellExecute = true} );
	}



	// ============================
	// New, Rename & Delete
	// Cutscenes

	private void OnCreateNewCutscene(object? sender, RoutedEventArgs e)
	{
		CreateNewCutscene();
	}

	private async void OnRenameCutscene(object? sender, RoutedEventArgs e)
	{
		// Only run this if we have a cutscene selected
		if (!Core.HasCutsceneSelected() || ArchiveUI == null)
			return;

		// Rename the cutscene
		await RenameCutscene(Core.GetCutscene().CutsceneName);
	}

	private void OnDeleteCutscene(object? sender, RoutedEventArgs e)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasCutsceneSelected()  || ArchiveUI == null)
			return;

		// Delete the cutscene
		DeleteCutscene(Core.GetArchive().GetSelectedCutsceneName());
	}



	// ============================
	// New, Rename & Delete
	// Parts

	private void OnNewPart(object? sender, RoutedEventArgs e)
	{
		NewPart();
	}

	private void OnRenamePart(object? sender, RoutedEventArgs e)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasPartSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		RenamePart(Core.GetSelectedPart().PartName);
	}

	private void OnDeletePart(object? sender, RoutedEventArgs e)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasPartSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		DeletePart(Core.GetSelectedPart().PartName);
	}



	// ============================
	// New, Rename & Delete
	// Sub Parts

	private void OnNewSubPart(object? sender, RoutedEventArgs e)
	{
		NewSubPart();
	}

	private void OnRenameSubPart(object? sender, RoutedEventArgs e)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasSubPartSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		RenameSubPart(Core.GetSelectedSubPart().SubPartName);
	}

	private void OnDeleteSubPart(object? sender, RoutedEventArgs e)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasSubPartSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		DeleteSubPart(Core.GetSelectedSubPart().SubPartName);
	}
	#endregion





	#region ArchiveHandler
	// ============================
	// Ask user input

	public async void Ask_NewArchive()
	{
		// Before creating a new archive let's check if the user has another archive open.
		// If they have, we'll ask if they want to discard the changes.
		if (Core.HasArchiveOpen() && HasEdited && !await AskDiscardChanges())
		{
			StatusText.Text = "New archive aborted.";
			return;
		}

		// Set edited to false to avoid asking discard the saves again
		HasEdited = false;

		// Ask the user where to save the file.
		// If the string is null it means that the user aborted the saving
		string? filePath = await MsgBox.AskSaveArcFile(StorageProvider, "DemoMyCoolCutscene.arc");
		if (filePath == null)
		{
			StatusText.Text = $"New archive aborted";
			return;
		}

		NewArchive(filePath);
	}

	public async void Ask_OpenArchive()
	{
		// Before opening a new archive let's check if the user has another archive open.
		// If they have, we'll ask if they want to discard the changes.
		if (Core.HasArchiveOpen() && HasEdited && !await AskDiscardChanges())
		{
			StatusText.Text = "Open aborted.";
			return;
		}

		// Set edited to false to avoid asking discard the saves again
		HasEdited = false;

		// Ask the user to open a .arc file.
		// If it's null, then the user aborted the open (and we should end the function as well).
		string? arcPathName = await MsgBox.AskOpenArcFile(StorageProvider);
		if (arcPathName == null)
		{
			StatusText.Text = "Open aborted.";
			return;
		}

		OpenArchive(arcPathName);
	}

	private async void Ask_ReloadArchive()
	{
		// Only run this if we have an archive open
		if (!Core.HasArchiveOpen())
			return;

		if (!await AskDiscardChanges())
		{
			StatusText.Text = "Reload aborted.";
			return;
		}

		// Reload the already opened archive
		string filePath = Core.GetArchive().FilePath;
		OpenArchive(filePath);

		// Update the status & set the title.
		StatusText.Text = $"Successfully reloaded '{filePath}'!";
		Title = $"CutsceneMaker - [{filePath}]";
	}

	public async void Ask_SaveAs()
	{
		if (!Core.HasArchiveOpen())
			return;

		// Finally ask the user where to save the file.
		// If the string is null it means that the user aborted the saving
		string? filePath = await MsgBox.AskSaveArcFile(StorageProvider, Core.GetArchive().FilePath);
		if (filePath == null)
		{
			StatusText.Text = $"Save aborted!";
			return;
		}

		SaveAs(filePath);
	}



	// ============================
	// Backend

	public async void NewArchive(string filePath)
	{
		// If has changed, ask to discard before continuing
		if (Core.HasArchiveOpen() && HasEdited && !await AskDiscardChanges())
		{
			StatusText.Text = "New archive aborted.";
			return;
		}

		// Copying the arc template to the new location
		File.Copy("./Templates/TemplateDemo.arc", filePath);

		// Open the archive.
		// And check for errors.
		CutsceneArchiveReadWrapper archiveWrapper = CutsceneArchive.LoadArchive(filePath);
		if (archiveWrapper.IsError)
		{
			await MsgBox.SendMessage(this, "Error while creating the .arc file", $"Couldn't open the newly made '{filePath}' file because of an error:\n\n{archiveWrapper.GetErrorMessage()}", ButtonEnum.Ok);
			StatusText.Text = $"Failed to create '{filePath}'.";
			return;
		}
		Core.LoadArchive(archiveWrapper.GetResult());

		// Update the menu buttons & update the UI
		ArchiveUI.LoadCutsceneList(Core.GetArchive().CutsceneNames);

		BtnsLayer_ArchiveOpen();
		MainMenu.Children.Clear();
		MainMenu.Children.Add(ArchiveUI);

		// Update the status & set the title.
		StatusText.Text = $"Successfully created '{filePath}'!";
		Title = $"CutsceneMaker - [{filePath}]";
	}

	public async void OpenArchive(string arcPathName)
	{
		// If has changed, ask to discard before continuing
		if (Core.HasArchiveOpen() && HasEdited && !await AskDiscardChanges())
		{
			StatusText.Text = "Open aborted.";
			return;
		}

		// Open the archive.
		// And check for errors.
		CutsceneArchiveReadWrapper archiveWrapper = CutsceneArchive.LoadArchive(arcPathName);
		if (archiveWrapper.IsError)
		{
			await MsgBox.SendMessage(this, "Error while opening the .arc file", $"Couldn't open '{arcPathName}' file because of an error:\n\n{archiveWrapper.GetErrorMessage()}", ButtonEnum.Ok);
			StatusText.Text = $"Failed to open '{arcPathName}'.";
			return;
		}
		Core.LoadArchive(archiveWrapper.GetResult());

		// Load other archives for auto completion
		Program.Utility.LoadRarcs(arcPathName);

		// Update the menu buttons & update the UI
		ArchiveUI.LoadCutsceneList(Core.GetArchive().CutsceneNames);

		BtnsLayer_ArchiveOpen();
		MainMenu.Children.Clear();
		MainMenu.Children.Add(ArchiveUI);

		// Update the status & set the title.
		StatusText.Text = $"Successfully opened '{arcPathName}'!";
		Title = $"CutsceneMaker - [{arcPathName}]";
	}


	public void Save()
	{
		if (!Core.HasArchiveOpen())
			return;

		Core.SaveArchive();
		StatusText.Text = $"Successfully saved the archive!";
	}

	public void SaveAs(string filePath)
	{
		if (!Core.HasArchiveOpen())
			return;

		Core.SaveArchiveTo(filePath);
		StatusText.Text = $"Successfully saved the archive to {filePath}!";
		Title = $"CutsceneMaker - [{filePath}]";
	}
	#endregion



	#region CutsceneHandler
	// ============================
	// Ask user input

	public void SelectCutscene(string cutsceneName)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		// Load the selected cutscene
		Core.LoadCutscene(cutsceneName);

		// Update the menu buttons & update the UI
		BtnsLayer_CutsceneOpen();

		ArchiveUI.LoadCutscene(Core.GetArchive().GetLoadedCutscene());
		if (ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
		ArchiveUI.CutsceneUI.TimelineUI.Part_RenderAll();

		// Update the status & set the title.
		StatusText.Text = $"Selected '{cutsceneName}' Cutscne!";
		Title = $"CutsceneMaker - [{Core.GetArchive().FilePath}] [{cutsceneName}]";
	}
	#endregion



	#region ActionHandlers_Part
	public void Part_Select(string partName)
	{
		if (!Core.HasCutsceneSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Set selected
		Core.SetSelectedPart(partName);

		// Update the combo box in the timeline
		Cutscene.Part part = Core.GetSelectedPart();

		// Load the part in the CutsceneView.
		// Also set the function that allows the timeline part to change size
		// when the user modifies the steps.
		ArchiveUI.CutsceneUI.LoadPart(part);

		if (part.SubPartEntries != null)
			ArchiveUI.CutsceneUI.TimelineUI.ComboBox_AddSubParts(part.SubPartEntries);
		else
			ArchiveUI.CutsceneUI.TimelineUI.ComboBox_Reset();

		// Update the buttons
		BtnsLayer_CutscenePartSelected();

		// Update the status
		StatusText.Text = $"Selected '{partName}'!";
	}

	public async void Part_UpdateName(string name)
	{
		if (!Core.HasPartSelected())
			return;

		if (Core.PartNameAlreadyExists(name))
		{
			await MessageBox("Part name already exists", $"Can't change '{Core.GetSelectedPartName()}' to '{name}' because a part with '{name}' already exists!");
			return;
		}

		Core.SetSelectedPart(name);
		ArchiveUI.CutsceneUI.TimelineUI.Part_Selected_SetName(name);
	}

	public void Part_UpdateStep(int step)
	{
		if (!Core.HasPartSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		ArchiveUI.CutsceneUI.TimelineUI.Part_Selected_UpdateSteps(step);
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
	}

	public void Part_MoveBefore()
	{
		if (!Core.HasPartSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		Cutscene cutscene = Core.GetArchive().GetLoadedCutscene();
		Cutscene.Part part = Core.GetSelectedPart();

		int i = cutscene.Parts.IndexOf(part);
		cutscene.Parts.Remove(part);
		cutscene.Parts.Insert(i-1, part);

		ArchiveUI.CutsceneUI.TimelineUI.Part_RenderAll();
		ArchiveUI.CutsceneUI.TimelineUI.TimelinePart_SetSelectedByName(part.PartName);
	}

	public void Part_MoveAfter()
	{
		if (!Core.HasPartSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		Cutscene cutscene = Core.GetArchive().GetLoadedCutscene();
		Cutscene.Part part = Core.GetSelectedPart();

		int i = cutscene.Parts.IndexOf(part);
		cutscene.Parts.Remove(part);
		cutscene.Parts.Insert(i+1, part);

		ArchiveUI.CutsceneUI.TimelineUI.Part_RenderAll();
		ArchiveUI.CutsceneUI.TimelineUI.TimelinePart_SetSelectedByName(part.PartName);
	}
	#endregion



	#region ActionHandlers_SubPart
	public void SubPart_Select(string subPartName)
	{
		if (!Core.HasCutsceneSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Set selected
		Core.SetSelectedSubPart(subPartName);

		SubPart part = Core.GetSelectedSubPart();

		// Load the part in the CutsceneView.
		// Also set the function that allows the timeline part to change size
		// when the user modifies the steps.
		ArchiveUI.CutsceneUI.LoadSubPart(part);

		// Update the buttons
		BtnsLayer_CutsceneSubPartSelected();

		// Update the status
		StatusText.Text = $"Selected '{subPartName}'!";
	}

	public async void SubPart_UpdateName(string name)
	{
		if (!Core.HasSubPartSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		if (Core.PartNameAlreadyExists(name))
		{
			await MessageBox("SubPart name already exists", $"Can't change '{Core.GetSelectedPartName()}' to '{name}' because a part with '{name}' already exists!");
			return;
		}

		Core.SetSelectedSubPart(name);
		ArchiveUI.CutsceneUI.TimelineUI.SubPart_Selected_SetName(name);
	}

	public void SubPart_UpdateStep(int step)
	{
		if (!Core.HasSubPartSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		ArchiveUI.CutsceneUI.TimelineUI.SubPart_Selected_UpdateSteps(step);
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
	}

	public void SubPart_Deselect(string partName, string subPartName)
	{
		if (!Core.HasCutsceneSelected() || ArchiveUI == null || ArchiveUI.CutsceneUI == null)
			return;

		// Deselect the subpart
		Core.DeselectSubPart();

		// Update the buttons
		BtnsLayer_CutscenePartSelected();

		// Re-select the part
		Core.SetSelectedPart(partName);
		ArchiveUI.CutsceneUI.LoadPart(Core.GetSelectedPart());
	}
	#endregion






	#region OutsideActions
	private async void OnRenameCutsceneContext(object? sender, RoutedEventArgs e)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		if (sender != null && sender is MenuItem btnItem)
		{
			if (btnItem.Parent!.Parent!.Parent!.Parent! is CutsceneBtn btn)
			{
				await RenameCutscene(btn.CutsceneName);
			}
		}
	}

	private void OnDeleteCutsceneContext(object? sender, RoutedEventArgs e)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		if (sender != null && sender is MenuItem btnItem)
		{
			if (btnItem.Parent!.Parent!.Parent!.Parent! is CutsceneBtn btn)
			{
				DeleteCutscene(btn.CutsceneName);
			}
		}
	}

	private void OnNewCutsceneContext(object? sender, RoutedEventArgs e)
	{
		CreateNewCutscene();
	}

	private void OnReloadCutsceneContext(object? sender, RoutedEventArgs e)
	{
		Ask_ReloadArchive();
	}

	private void OnNewPartContext(object? sender, RoutedEventArgs e)
	{
		NewPart();
	}

	private void OnRenamePartContext(object? sender, RoutedEventArgs e)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		if (sender != null && sender is MenuItem partItem)
		{
			if (partItem.Parent!.Parent!.Parent!.Parent! is TimelinePart part)
			{
				RenamePart(part.PartName);
			}
		}
	}

	private void OnDeletePartContext(object? sender, RoutedEventArgs e)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		if (sender != null && sender is MenuItem partItem)
		{
			if (partItem.Parent!.Parent!.Parent!.Parent! is TimelinePart part)
			{
				DeletePart(part.PartName);
			}
		}
	}

	private void OnNewSubPartContext(object? sender, RoutedEventArgs e)
	{
		NewSubPart();
	}

	private void OnRenameSubPartContext(object? sender, RoutedEventArgs e)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		if (sender != null && sender is MenuItem partItem)
		{
			if (partItem.Parent!.Parent!.Parent!.Parent! is TimelinePart part)
			{
				RenameSubPart(part.PartName);
			}
		}
	}

	private void OnDeleteSubPartContext(object? sender, RoutedEventArgs e)
	{
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		if (sender != null && sender is MenuItem partItem)
		{
			if (partItem.Parent!.Parent!.Parent!.Parent! is TimelinePart part)
			{
				DeleteSubPart(part.PartName);
			}
		}
	}
	#endregion



	#region ActionHelpers
	private async Task<string?> RenameCutscene(string cutsceneName)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return null;

		string? selectedCutsceneName = null;
		if (Core.HasCutsceneSelected())
			selectedCutsceneName = Core.GetArchive().GetSelectedCutsceneName();

		// Ask for the new cutscene name
		// And also send a list of already existing cutscenes name so we avoid replacing cutscenes
		string? newCutsceneName = await MsgBox.AskName(this, "Rename Cutscene", $"Type a new name for the cutscene '{cutsceneName}'", cutsceneName, cutsceneName, Core.GetArchive().CutsceneNames, "A cutscene with this name already exists!");
		// If this value is null it means that the user aborted
		if (newCutsceneName == null)
		{
			StatusText.Text = $"Aborted rename cutscene.";
			return null;
		}

		// Rename the cutscene
		Core.GetArchive().RenameCutscene(cutsceneName, newCutsceneName);

		// Reload the cutscene list
		if (selectedCutsceneName != null && selectedCutsceneName == cutsceneName)
			ArchiveUI.LoadCutsceneListAndSelect(Core.GetArchive().CutsceneNames, newCutsceneName);
		else if (selectedCutsceneName != null && selectedCutsceneName != cutsceneName)
			ArchiveUI.LoadCutsceneListAndSelect(Core.GetArchive().CutsceneNames, selectedCutsceneName);
		else
			ArchiveUI.LoadCutsceneList(Core.GetArchive().CutsceneNames);

		// Update the status & set the title.
		StatusText.Text = $"Successfully renamed the cutscene '{cutsceneName}' to '{newCutsceneName}'!";

		return newCutsceneName;
	}

	private async void DeleteCutscene(string cutsceneName)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		string? selectedCutsceneName = null;
		if (Core.HasCutsceneSelected())
			selectedCutsceneName = Core.GetArchive().GetSelectedCutsceneName();

		// Ask the user if they really want to delete the cutscene
		if (await MsgBox.SendMessage(this, "Cutscene deletion confirm", $"Are you sure you want to delete the cutscene '{cutsceneName}'?\nThis operation can't be undone!!", ButtonEnum.YesNo) == ButtonResult.No)
		{
			// If not, abort
			StatusText.Text = $"Abort cutscene deletion.";
			return;
		}

		// Delete the cutscene
		Core.GetArchive().DeleteCutscene(cutsceneName);

		// Reload the cutscene list
		if (selectedCutsceneName != null && selectedCutsceneName != cutsceneName)
			ArchiveUI.LoadCutsceneListAndSelect(Core.GetArchive().CutsceneNames, selectedCutsceneName);
		else
		{
			ArchiveUI.LoadCutsceneList(Core.GetArchive().CutsceneNames);
			BtnsLayer_ArchiveOpen();
		}


		// Update the status & set the title.
		StatusText.Text = $"Successfully deleted the cutscene with name '{cutsceneName}'!";
	}

	private async void CreateNewCutscene()
	{
		// Only run this if we have an archive open
		if (!Core.HasArchiveOpen() || ArchiveUI == null)
			return;

		// Ask for a cutscene name
		// And also send a list of already existing cutscenes name so we avoid replacing cutscenes
		string? cutsceneName = await MsgBox.AskName(this, "New Cutscene", "Type a name of your new cutscene", "DemoMyCoolCutscene", null, Core.GetArchive().CutsceneNames, "A cutscene with this name already exists!");
		// If this value is null it means that the user aborted
		if (cutsceneName == null)
		{
			StatusText.Text = $"Aborted new cutscene.";
			return;
		}

		// Create the new cutscene with name
		Core.GetArchive().CreateNewCutscene(cutsceneName);

		// Reload the cutscene list & select the cutscene
		ArchiveUI.LoadCutsceneListAndSelect(Core.GetArchive().CutsceneNames, cutsceneName);


		// Update the status & set the title.
		StatusText.Text = $"Successfully created new cutscene with name '{cutsceneName}'!";
	}

	private async void NewPart()
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasCutsceneSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Ask the user the name of the part
		// Also send a list of all part names so we avoid replacing parts
		string? partName = await MsgBox.AskName(this, "New Part", "Type a name of your new part", "MyCoolPart", null, Core.GetAllPartNames(), "A part/subpart with this name already exists!");
		// If this value is null it means that the user aborted
		if (partName == null)
		{
			StatusText.Text = $"Aborted new part.";
			return;
		}

		// Add the part
		Cutscene.Part part = new Cutscene.Part(partName);
		part.TimeEntry.TotalStep = 40;
		Core.GetArchive().GetLoadedCutscene().Parts.Add(part);

		// Set selected
		Core.SetSelectedPart(partName);

		// Re-render the parts, update the steps & set the new part as selected
		ArchiveUI.CutsceneUI.TimelineUI.Part_RenderAll();
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
		ArchiveUI.CutsceneUI.TimelineUI.TimelinePart_SetSelectedByName(partName);

		// Update the status
		StatusText.Text = $"Successfully created a new part named '{partName}'!";
	}

	public async void RenamePart(string oldPartName)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasCutsceneSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Get part
		Cutscene.Part part = Core.GetArchive().GetLoadedCutscene().GetPartByName(oldPartName);

		string? selectedPartName = null;
		if (Core.HasPartSelected())
			selectedPartName = Core.GetSelectedPart().PartName;

		// Ask the user the name of the part
		// Also send a list of all part names so we avoid replacing parts
		string? partName = await MsgBox.AskName(this, "Rename SubPart", $"Type a new name for the part '{part.PartName}'", part.PartName, part.PartName, Core.GetAllPartNames(), "A part/subpart with this name already exists!");
		// If this value is null it means that the user aborted
		if (partName == null)
		{
			StatusText.Text = $"Aborted rename part.";
			return;
		}

		// Rename the part
		part.PartName = partName;

		// Re-render the parts, update the steps & set the new part as selected
		ArchiveUI.CutsceneUI.TimelineUI.Part_RenderAll();
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();

		if (selectedPartName == oldPartName)
			ArchiveUI.CutsceneUI.TimelineUI.TimelinePart_SetSelectedByName(partName);

		// Update the status
		StatusText.Text = $"Successfully renamed the part to '{partName}'!";
	}

	public async void DeletePart(string partName)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasCutsceneSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Get part
		Cutscene.Part part = Core.GetArchive().GetLoadedCutscene().GetPartByName(partName);

		string? selectedPartName = null;
		if (Core.HasPartSelected())
			selectedPartName = Core.GetSelectedPart().PartName;

		// Ask the user if they really want to delete the part
		if (await MsgBox.SendMessage(this, "Part deletion confirm", $"Are you sure you want to delete the part '{part.PartName}'?\nThis operation can't be undone!!", ButtonEnum.YesNo) == ButtonResult.No)
		{
			// If not, abort
			StatusText.Text = $"Abort part deletion.";
			return;
		}

		// Remove the part
		Cutscene cutscene = Core.GetCutscene();
		cutscene.Parts.Remove(part);

		// Re-render the parts, update the steps & set the new part as selected
		ArchiveUI.CutsceneUI.TimelineUI.Part_RenderAll();
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();

		// De-select the part
		if (selectedPartName == partName)
			ArchiveUI.CutsceneUI.LoadPart(null);

		// Update the status
		StatusText.Text = $"Successfully removed the part to '{part.PartName}'!";
	}

	public async void NewSubPart()
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasCutsceneSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Ask the user the name of the part
		// Also send a list of all part names so we avoid replacing parts
		string? subPartName = await MsgBox.AskName(this, "New SubPart", "Type a name of your new sub part", "MyCoolSubPart", null, Core.GetAllPartNames(), "A part/subpart with this name already exists!");
		// If this value is null it means that the user aborted
		if (subPartName == null)
		{
			StatusText.Text = $"Aborted new sub part.";
			return;
		}

		// Get the part
		Cutscene.Part part = Core.GetSelectedPart();

		// Add the sub part
		SubPart subPart = new SubPart(subPartName);
		subPart.SubPartTotalStep = 40;
		if (part.SubPartEntries == null)
			part.SubPartEntries = new List<SubPart>();
		part.SubPartEntries.Add(subPart);

		// Set selected
		Core.SetSelectedSubPart(subPartName);

		// Re-render the parts, update the steps & set the new part as selected
		ArchiveUI.CutsceneUI.TimelineUI.SubPart_Render(Core.GetSelectedSubPart());
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
		ArchiveUI.CutsceneUI.TimelineUI.TimelineSubPart_SetSelectedByName(subPartName);

		// Update the status
		StatusText.Text = $"Successfully created a new sub part named '{subPartName}'!";
	}

	public async void RenameSubPart(string oldSubPartName)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasPartSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Get part
		SubPart subPart = Core.GetCutscene().GetSubPartByName(oldSubPartName);

		string? selectedSubPartName = null;
		if (Core.HasPartSelected())
			selectedSubPartName = Core.GetSelectedSubPart().SubPartName;

		// Ask the user the name of the part
		// Also send a list of all part names so we avoid replacing parts
		string? subPartName = await MsgBox.AskName(this, "Rename SubPart", $"Type a new name for the sub part '{subPart.SubPartName}'", subPart.SubPartName, subPart.SubPartName, Core.GetAllPartNames(), "A part/subpart with this name already exists!");
		// If this value is null it means that the user aborted
		if (subPartName == null)
		{
			StatusText.Text = $"Aborted rename sub part.";
			return;
		}

		// Rename the part
		subPart.SubPartName = subPartName;

		// Re-render the parts, update the steps & set the new part as selected
		Cutscene.Part part = Core.GetSelectedPart();
		ArchiveUI.CutsceneUI.TimelineUI.SubPart_Render(subPart);
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
		ArchiveUI.CutsceneUI.TimelineUI.ComboBox_AddSubParts(part.SubPartEntries!);

		if (selectedSubPartName == oldSubPartName)
			ArchiveUI.CutsceneUI.TimelineUI.TimelineSubPart_SetSelectedByName(subPartName);

		// Update the status
		StatusText.Text = $"Successfully renamed the sub part to '{subPartName}'!";
	}

	public async void DeleteSubPart(string subPartName)
	{
		// Only run this if we have an archive & a cutscene open
		if (!Core.HasPartSelected()  || ArchiveUI == null || ArchiveUI.CutsceneUI == null || ArchiveUI.CutsceneUI.TimelineUI == null)
			return;

		// Get the selected sub part
		SubPart subPart = Core.GetCutscene().GetSubPartByName(subPartName);

		string? selectedSubPartName = null;
		if (Core.HasPartSelected())
			selectedSubPartName = Core.GetSelectedSubPart().SubPartName;

		// Ask the user if they really want to delete the sub part
		if (await MsgBox.SendMessage(this, "SubPart deletion confirm", $"Are you sure you want to delete the sub part '{subPartName}'?\nThis operation can't be undone!!", ButtonEnum.YesNo) == ButtonResult.No)
		{
			// If not, abort
			StatusText.Text = $"Abort sub part deletion.";
			return;
		}

		// Remove the sub part
		Cutscene.Part part = Core.GetSelectedPart();
		part.SubPartEntries!.Remove(subPart);

		// Re-render the parts, update the steps & set the new part as selected
		if (selectedSubPartName == subPartName)
			ArchiveUI.CutsceneUI.TimelineUI.SubPart_Render(null);
		else if (selectedSubPartName != null)
			ArchiveUI.CutsceneUI.TimelineUI.SubPart_Render(Core.GetSelectedSubPart());
		ArchiveUI.CutsceneUI.TimelineUI.Timeline_UpdateSteps();
		ArchiveUI.CutsceneUI.TimelineUI.ComboBox_AddSubParts(part.SubPartEntries!);

		if (selectedSubPartName == subPartName)
			ArchiveUI.CutsceneUI.TimelineUI.TimelineSubPart_SetSelectedByName(subPartName);

		// Update the status
		StatusText.Text = $"Successfully removed the sub part to '{subPartName}'!";
	}
	#endregion





	// 		private void EnableSaveButtons(bool enabled) {
	// 			SaveButton.IsEnabled = enabled;
	// 			SaveAsButton.IsEnabled = enabled;
	// 		}
	//
	//
	//
	//
	//
	//
	// 		private async Task<bool> AskLoseUnsavedChanges() {
	// 			return await CMMsgBox.AskLoseUnsavedChanges(this);
	// 		}
	//
	//
	// 		private void OnClickNewArchive(object sender, RoutedEventArgs e) {
	// 			// TODO: Create a cutscene archive maker
	//
	// // 			if (hasDialogWindowOpen) return;
	// // 			if (!(await AskLoseUnsavedChanges())) return;
	// //
	// // 			hasDialogWindowOpen = true;
	// // 			string? CutsceneName = await CMMsgBox.AskBCSVName(this, null);
	// // 			hasDialogWindowOpen = false;
	// // 			if (string.IsNullOrEmpty(CutsceneName)) return;
	// //
	// // 			cutscene = Cutscene.NewCutsceneFromTemplate(CutsceneName);
	// // 			// LoadTree(cutscene);
	// //
	// // 			folderPath = null;
	// // 			fileName = CutsceneName;
	// //
	// // 			UpdateUILoad();
	// //
	// // 			MainWindowX.Title = $"CutsceneMaker - [New Cutscene \"{CutsceneName}\"]";
	// // 			StatusText.Text = $"New cutscene \"{CutsceneName}\" created!";
	// 		}
	//
	// // 		private async void OnClickOpen(object? sender, RoutedEventArgs e) {
	// // 			if (hasDialogWindowOpen) return;
	// // 			if (!(await AskLoseUnsavedChanges())) return;
	// //
	// // 			hasDialogWindowOpen = true;
	// // 			string? FilePath = await CMMsgBox.AskOpenBCSVTimeFile(StorageProvider);
	// // 			hasDialogWindowOpen = false;
	// // 			if (FilePath == null) {
	// // 				StatusText.Text = "Open aborted";
	// // 				return;
	// // 			}
	// //
	// // 			cutscene = Cutscene.NewCutsceneFromFiles(FilePath);
	// // 			// LoadTree(cutscene);
	// // 			folderPath = Path.GetDirectoryName(FilePath)!;
	// // 			fileName = FilePath.Substring(folderPath.Length, FilePath.Length - folderPath.Length - 9);
	// //
	// // 			UpdateUILoad();
	// //
	// // 			string TitleString = FilePath.Substring(0, FilePath.Length - 9);
	// //
	// // 			MainWindowX.Title = $"CutsceneMaker - [{TitleString}]";
	// // 			StatusText.Text = $"Loaded \"{TitleString}\" successfully!";
	// // 		}
	//
	// 		private async void OnClickOpenArchive(object? sender, RoutedEventArgs e) {
	// 			if (hasDialogWindowOpen) return;
	// 			// if (!(await AskLoseUnsavedChanges())) {
	// 			// 	StatusText.Text = $"Aborted Open";
	// 			// 	return;
	// 			// }
	//
	// 			hasDialogWindowOpen = true;
	// 			string? FilePath = await CMMsgBox.AskOpenArcFile(StorageProvider);
	// 			hasDialogWindowOpen = false;
	//
	// 			if (FilePath == null) {
	// 				StatusText.Text = $"Aborted Open";
	// 				return;
	// 			}
	//
	// 			CutsceneArchiveReadWrapper res = CutsceneArchive.LoadArchive(FilePath);
	// 			if (res.IsError()) {
	// 				await CMMsgBox.SendMessage(this, "Error", $"Couldn't open the file {FilePath} because of an error:\n\n{res.GetErrorMessage()}", ButtonEnum.Ok);
	// 				StatusText.Text = $"Failed opened \"{FilePath}\"!";
	// 				return;
	// 			}
	//
	// 			SelectCutscenePart(null);
	//
	// 			archive = res.GetResult();
	// 			PopulateArchiveSidebar();
	//
	// 			MainViewEmpty.IsVisible = false;
	// 			MainViewLoaded.IsVisible = true;
	//
	// 			CutscenePanelEmpty.IsVisible = true;
	// 			CutscenePanelPopulation.IsVisible = false;
	//
	// 			CutscenePanelNoParts.IsVisible = false;
	// 			CutscenePanelTabs.IsVisible = false;
	//
	// 			ArchiveMenu.IsEnabled = true;
	// 			CutsceneMenu.IsEnabled = false;
	//
	// 			// TODO: Select the first element. Might not going to add it tho.
	//
	// 			StatusText.Text = $"Successfully opened \"{FilePath}\"!";
	// 		}
	//
	// 		private void PopulateArchiveSidebar() {
	// 			// if (cutscene == null) return;
	// 			if (archive == null) return;
	//
	// 			ArchiveSidebarCutscenePool.Children.Clear();
	//
	// 			if (archive.CutsceneNames.Count() < 1) {
	// 				ArchiveSidebarEmpty.IsVisible = true;
	// 				ArchiveSidebarPopulation.IsVisible = false;
	// 				return;
	// 			}
	//
	// 			ArchiveSidebarEmpty.IsVisible = false;
	// 			ArchiveSidebarPopulation.IsVisible = true;
	//
	// 			foreach (string cutsceneName in archive.CutsceneNames) {
	//
	// 				ArchiveSidebarCutscenePool.Children.Add(BuildArchiveItem(cutsceneName));
	// 			}
	// 		}
	//
	// 		private Button BuildArchiveItem(string text) {
	//
	// 			Button btn = new();
	// 			btn.HorizontalAlignment = HorizontalAlignment.Stretch;
	// 			btn.Click += OnClickSelectCutsceneArchive;
	//
	// 			StackPanel stack = new StackPanel();
	// 			stack.Orientation = Orientation.Horizontal;
	// 			stack.VerticalAlignment = VerticalAlignment.Center;
	//
	// 			Image image = new Image();
	// 			image.Source = new Bitmap("Assets/placeholder.png");
	// 			image.Width = 14;
	// 			image.Height = 14;
	//
	// 			Label lbl = new Label();
	// 			lbl.FontSize = 12;
	// 			lbl.Content = text;
	//
	// 			stack.Children.Add(image);
	// 			stack.Children.Add(lbl);
	//
	// 			btn.Content = stack;
	//
	// 			return btn;
	// 		}
	//
	// 		private void EnableAllArchiveSidebarButtons() {
	// 			// if (cutscene == null) return;
	// 			if (!ArchiveSidebarCutscenePool.IsVisible) return;
	//
	// 			foreach (object control in ArchiveSidebarCutscenePool.Children) {
	// 				Button btn = (Button) control;
	// 				btn.IsEnabled = true;
	// 			}
	// 		}
	//
	// 		private void OnClickSelectCutsceneArchive(object? sender, RoutedEventArgs e) {
	// 			// if (cutscene == null) return;
	// 			if (archive == null || sender == null) return;
	//
	// 			Button btn = (Button) sender;
	// 			if (btn.Content == null) return;
	//
	// 			StackPanel stack = (StackPanel) btn.Content;
	// 			if (stack.Children[1] == null) return;
	//
	// 			Label label = (Label) stack.Children[1];
	// 			if (label.Content == null) return;
	//
	//
	// 			archive.LoadCutscene((string) label.Content);
	// 			SelectCutscenePart(null);
	//
	//
	// 			EnableAllArchiveSidebarButtons();
	// 			btn.IsEnabled = false;
	//
	// 			if (CutscenePanelPopulation.IsVisible == false) {
	// 				CutscenePanelEmpty.IsVisible = false;
	// 				CutscenePanelPopulation.IsVisible = true;
	// 			}
	// 			if (!CutsceneMenu.IsEnabled) CutsceneMenu.IsEnabled = true;
	//
	// 			RenderTimelineParts();
	//
	// 			CutscenePanelNoParts.IsVisible = true;
	// 			CutscenePanelTabs.IsVisible = false;
	//
	// 			StatusText.Text = $"Successfully selected \"{archive.SelectedCutsceneName}\"!";
	// 		}
	//
	// 		public void RenderTimelineParts() {
	// 			if (archive == null || archive.SelectedCutsceneName == null) return;
	//
	// 			CutsceneMainTimeline.Children.Clear();
	//
	// 			CreateTimelineSecondStamps();
	// 			foreach (Cutscene.Part part in archive.GetLoadedCutscene().Parts) {
	// 				// Console.WriteLine($"{part.PartName}: {part.TimeEntry.TotalStep}");
	// 				CutsceneMainTimeline.Children.Add(GenerateTimelinePartSection(part.TimeEntry.TotalStep, part.PartName));
	// 			}
	// 		}
	//
	// 		private void OnClickSave(object? sender, RoutedEventArgs e) {
	// // 			// if (cutscene == null) return;
	// // 			if (folderPath == null || fileName == null) {
	// // 				OnClickSaveAs(sender, e);
	// // 				return;
	// // 			}
	// //
	// // 			// cutscene.SaveAll(Path.Combine(folderPath, fileName));
	// //
	// // 			StatusText.Text = $"Saved to \"{folderPath}\" as \"{fileName}\" successfully!";
	// 		}
	//
	// 		private void OnClickSaveAs(object? sender, RoutedEventArgs e) {
	// // 			// if (cutscene == null) return;
	// //
	// // 			string? ReturnedFolderPath = await CMMsgBox.AskSaveBCSVFile(StorageProvider, folderPath);
	// // 			if (ReturnedFolderPath == null) return;
	// //
	// // 			// string FileName = "";
	// // 			// if (fileName != null) {
	// // 			// 	FileName = fileName;
	// // 			// }
	// // 			string? NewName = await CMMsgBox.AskBCSVName(this, FileName);
	// // 			if (string.IsNullOrEmpty(NewName)) {
	// // 				StatusText.Text = "Aborted saving.";
	// // 				return;
	// // 			}
	// //
	// // 			string CombinedPath = Path.Combine(ReturnedFolderPath, NewName);
	// // 			if (Path.Exists(Path.Join(CombinedPath, "Time.bcsv"))) {
	// // 				ButtonResult Choice = await CMMsgBox.SendMessage(this, "Overwrite", $"A cutscene with the name {NewName} already exists.\n\nDo you want to overwrite it?", ButtonEnum.YesNo);
	// // 				if (Choice == ButtonResult.No) {
	// // 					StatusText.Text = "Aborted saving.";
	// // 					return;
	// // 				}
	// // 			}
	// //
	// // 			// cutscene.SaveAll(CombinedPath);
	// //
	// // 			fileName = NewName;
	//
	// 			// MainWindowX.Title = $"CutsceneMaker - [{CombinedPath}]";
	// 			// StatusText.Text = $"Saved to \"{ReturnedFolderPath}\" as \"{NewName}\" successfully!";
	// 		}
	//
	// 		private void CreateTimelineSecondStamps() {
	// 			if (archive == null || archive.SelectedCutsceneName == null || !CutscenePanelPopulation.IsVisible) return;
	//
	// 			CutsceneMainTimelineTime.Children.Clear();
	// 			CutsceneSubTimelineTime.Children.Clear();
	//
	// 			int limit = archive.GetLoadedCutscene().GetMaxTotalSteps() + 40;
	// 			int minWidth = ((int) (MainWindowX.Width * 0.125)) - 30;
	// 			if (limit < minWidth) limit = minWidth;
	// 			int diff = limit % 5;
	// 			limit -= diff - 1;
	//
	// 			// Console.WriteLine("Limit: {0}", limit);
	//
	// 			for (int i = 1; i < limit; i++) {
	// 				if (i % 5 == 0 && i % 10 != 0) {
	// 					CutsceneMainTimelineTime.Children.Add(GenerateTimelineFractionSecondStampPanel());
	// 					CutsceneSubTimelineTime.Children.Add(GenerateTimelineFractionSecondStampPanel());
	// 				}
	//
	// 				if (i % 10 == 0) {
	// 					CutsceneMainTimelineTime.Children.Add(GenerateTimelineSecondStampPanel(i));
	// 					CutsceneSubTimelineTime.Children.Add(GenerateTimelineSecondStampPanel(i));
	// 				}
	// 			}
	// 		}
	//
	// 		public DockPanel GenerateTimelineSecondStampPanel(int second) {
	// 			// <DockPanel Width="40" Height="15" HorizontalAlignment="Left" VerticalAlignment="Bottom">
	// 			// 	<Border BorderThickness="0, 0, 1, 0" BorderBrush="#8f8f8f">
	// 			// 		<Label FontSize="6" Foreground="#8f8f8f" HorizontalAlignment="Right" VerticalAlignment="Center">10</Label>
	// 			// 	</Border>
	// 			// 	</DockPanel>
	// 			// 	<DockPanel Width="40" Height="10" HorizontalAlignment="Left" VerticalAlignment="Bottom">
	// 			// 	<Border BorderThickness="0, 0, 1, 0" BorderBrush="#6f6f6f"></Border>
	// 			// </DockPanel>
	//
	// 			DockPanel panel = new();
	// 			panel.Width = 40;
	// 			panel.Height = 15;
	// 			panel.HorizontalAlignment = HorizontalAlignment.Left;
	// 			panel.VerticalAlignment = VerticalAlignment.Bottom;
	//
	// 			Border border = new();
	// 			border.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
	// 			border.BorderBrush = Brush.Parse("#8f8f8f");
	//
	// 			Label label = new();
	// 			label.FontSize = 8;
	// 			label.Foreground = Brush.Parse("#8f8f8f");
	// 			label.HorizontalAlignment = HorizontalAlignment.Right;
	// 			label.VerticalAlignment = VerticalAlignment.Center;
	// 			label.Content = $"{second}";
	//
	// 			border.Child = label;
	// 			panel.Children.Add(border);
	//
	// 			return panel;
	// 		}
	//
	// 		public DockPanel GenerateTimelineFractionSecondStampPanel() {
	// 			// <DockPanel Width="40" Height="10" HorizontalAlignment="Left" VerticalAlignment="Bottom">
	// 			// 	<Border BorderThickness="0, 0, 1, 0" BorderBrush="#6f6f6f"></Border>
	// 			// </DockPanel>
	//
	// 			DockPanel panel = new();
	// 			panel.Width = 40;
	// 			panel.Height = 10;
	// 			panel.HorizontalAlignment = HorizontalAlignment.Left;
	// 			panel.VerticalAlignment = VerticalAlignment.Bottom;
	//
	// 			Border border = new();
	// 			border.BorderThickness = new Thickness(0.0, 0.0, 1.0, 0.0);
	// 			border.BorderBrush = Brush.Parse("#8f8f8f");
	//
	// 			panel.Children.Add(border);
	//
	// 			return panel;
	// 		}
	//
	// 		public Border GenerateTimelinePartSection(int steps, string partName) {
	// 			// <Border BorderThickness="1" BorderBrush="#cce" Height="50" Width="120">
	// 			// 	<Grid ColumnDefinitions="1*" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Background="#668">
	// 			// 		<Label VerticalAlignment="Center">Test1</Label>
	// 			// 	</Grid>
	// 			// </Border>
	//
	// 			Border border = new();
	// 			border.BorderThickness = new Thickness(1.0);
	// 			border.BorderBrush = Brush.Parse("#cce");
	// 			border.HorizontalAlignment = HorizontalAlignment.Left;
	// 			border.VerticalAlignment = VerticalAlignment.Center;
	// 			border.Height = 50;
	// 			border.Width = steps * 8;
	// 			border.AddHandler(PointerPressedEvent, OnClickSelectPart, RoutingStrategies.Tunnel);
	//
	// 			Grid grid = new();
	// 			grid.ColumnDefinitions = new ColumnDefinitions("1*");
	// 			grid.HorizontalAlignment = HorizontalAlignment.Stretch;
	// 			grid.VerticalAlignment = VerticalAlignment.Stretch;
	// 			grid.Background = Brush.Parse("#668");
	//
	// 			Label label = new();
	// 			label.VerticalAlignment = VerticalAlignment.Center;
	// 			label.Content = partName;
	//
	// 			grid.Children.Add(label);
	// 			border.Child = grid;
	//
	// 			return border;
	// 		}
	//
	// 		public Border GenerateTimelineSubPartSection(int steps, string partName) {
	// 			// <Border BorderThickness="1" BorderBrush="#cce" Height="50" Width="120">
	// 			// 	<Grid ColumnDefinitions="1*" HorizontalAlignment="Stretch" VerticalAlignment="Stretch" Background="#668">
	// 			// 		<Label VerticalAlignment="Center">Test1</Label>
	// 			// 	</Grid>
	// 			// </Border>
	//
	// 			Border border = new();
	// 			border.BorderThickness = new Thickness(1.0);
	// 			border.BorderBrush = Brush.Parse("#9999a9");
	// 			border.HorizontalAlignment = HorizontalAlignment.Left;
	// 			border.VerticalAlignment = VerticalAlignment.Center;
	// 			border.Height = 50;
	// 			border.Width = steps * 8;
	//
	// 			Grid grid = new();
	// 			grid.ColumnDefinitions = new ColumnDefinitions("1*");
	// 			grid.HorizontalAlignment = HorizontalAlignment.Stretch;
	// 			grid.VerticalAlignment = VerticalAlignment.Stretch;
	// 			grid.Background = Brush.Parse("#595979");
	//
	// 			Label label = new();
	// 			label.VerticalAlignment = VerticalAlignment.Center;
	// 			label.Content = partName;
	//
	// 			grid.Children.Add(label);
	// 			border.Child = grid;
	//
	// 			return border;
	// 		}
	//
	// 		private void EnableAllPartsInTimeline() {
	// 			foreach (object obj in CutsceneMainTimeline.Children) {
	// 				Border border = (Border) obj;
	// 				object? gridObj = border.Child;
	//
	// 				if (gridObj == null) return;
	// 				Grid grid = (Grid) gridObj;
	// 				grid.Background = Brush.Parse("#595979");
	// 			}
	//  		}
	//
	// 		public void OnClickSelectPart(object? sender, RoutedEventArgs e) {
	// 			if (sender == null) return;
	// 			Border border = (Border) sender;
	//
	// 			object? gridObj = border.Child;
	// 			if (gridObj == null) return;
	//
	// 			Grid grid = (Grid) gridObj;
	// 			object? labelObj = grid.Children[0];
	//
	// 			if (labelObj == null) return;
	// 			Label label = (Label) labelObj;
	//
	// 			object? selectedPartObj = label.Content;
	// 			if (selectedPartObj == null) return;
	//
	//
	// 			EnableAllPartsInTimeline();
	// 			grid.Background = Brush.Parse("#446");
	//
	// 			SelectCutscenePart((string) selectedPartObj);
	//
	// 			StatusText.Text = $"Selected part \"{selectedPart}\"!";
	// 		}
	//
	// 		public void SelectCutscenePart(string? selPart) {
	// 			if (selPart == null || archive == null || archive.SelectedCutsceneName == null) {
	// 				selectedPart = null;
	// 				DisableSubPartSelectionComboBox();
	// 				return;
	// 			}
	//
	// 			selectedPart = selPart;
	//
	// 			CutscenePanelNoParts.IsVisible = false;
	// 			CutscenePanelTabs.IsVisible = true;
	//
	// 			Cutscene cs = archive.GetLoadedCutscene();
	// 			Cutscene.Part part = cs.GetPartByName(selPart);
	//
	// 			if (part.SubPartEntries == null) {
	// 				DisableSubPartSelectionComboBox();
	// 				return;
	// 			}
	//
	// 			if (part.SubPartEntries.Count() < 1) {
	// 				DisableSubPartSelectionComboBox();
	// 				return;
	// 			}
	//
	// 			CutsceneTimelineSubPartBox.Items.Clear();
	// 			CutsceneTimelineSubPartBox.SelectedIndex = 0;
	// 			CutsceneTimelineSubPartBox.IsEnabled = true;
	// 			foreach (Abacus.SubPart subPart in part.SubPartEntries) {
	// 				CutsceneTimelineSubPartBox.Items.Add(new ComboBoxItem { Content = subPart.SubPartName });
	// 			}
	// 		}
	//
	// 		public void DisableSubPartSelectionComboBox() {
	// 			CutsceneTimelineSubPartBox.Items.Clear();
	// 			CutsceneTimelineSubPartBox.Items.Add(new ComboBoxItem { Content = "No SubParts" });
	// 			CutsceneTimelineSubPartBox.SelectedIndex = 0;
	// 			CutsceneTimelineSubPartBox.IsEnabled = false;
	// 		}

	// private void LoadTree(Cutscene cutscene) {
	//     TreeParts.Items.Clear();
	//
	//     foreach (var part in cutscene.Parts) {
	//         TreeViewItem item = new() { Header = part.PartName, Tag = part, ContextFlyout = PartsFlyout };
	//
	//         if (part.SubPartEntries != null) {
	//             foreach (Abacus.SubPart subPart in part.SubPartEntries) {
	//                 item.Items.Add(new TreeViewItem() { Header = subPart.SubPartName, Tag = subPart, ContextFlyout = PartsFlyout });
	//             }
	//         }
	//         TreeParts.Items.Add(item);
	//     }
	// }
	//
	// private void LoadTabControl(ICommonEntries part) {
	//     var playertab = new Player(part);
	//     var cameratab = new Camera(part);
	//     var actiontab = new Action(part);
	//     var soundtab = new Sound(part);
	//     var wipetab = new Wipe(part);
	//
	//     if (part is Cutscene.Part part1) {
	//         MainTab.Header = "Time";
	//         MainTab.Content = new Time(part1);
	//     } else {
	//         MainTab.Header = "SubPart";
	//         MainTab.Content = new SubPart((Abacus.SubPart)part);
	//     }
	//
	//     PlayerTab.Content = playertab;
	//     CameraTab.Content = cameratab;
	//     ActionTab.Content = actiontab;
	//     SoundTab.Content = soundtab;
	//     WipeTab.Content = wipetab;
	// }
	//
	// private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem selectedItem) {
	//         LoadTabControl((ICommonEntries)selectedItem.Tag!);
	//     }
	// }

	// private void OnClickCreatePart(object? sender, RoutedEventArgs e) {
	//     PartNameBox.Clear();
	//
	//     if (cutscene != null) {
	//         PartNameButton.IsVisible = false;
	//         PartNameBox.IsVisible = true;
	//         ArrowUp.IsVisible = false;
	//         ArrowDown.IsVisible = false;
	//         PartNameBox.Focus();
	//     }
	// }
	//
	// private void OnEnterClick(object? sender, KeyEventArgs e) {
	//     if (e.Key == Key.Enter && !string.IsNullOrEmpty(PartNameBox.Text)) {
	//         if (cutscene!.Parts.Any(p => p.PartName == PartNameBox.Text!)) {
	//             MessageBoxManager.GetMessageBoxStandard("Error", "PartName already exists!", MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
	//             return;
	//         }
	//
	//         cutscene!.Parts.Add(new Cutscene.Part(PartNameBox.Text!));
	//         LoadTree(cutscene); // I'm not going to manually add the Part.
	//         PartNameButton.IsVisible = true;
	//         PartNameBox.IsVisible = false;
	//     }
	// }
	//
	// private void OnLostFocusCreatePart(object? sender, RoutedEventArgs e) {
	//     PartNameButton.IsVisible = true;
	//     PartNameBox.IsVisible = false;
	//     ArrowUp.IsVisible = true;
	//     ArrowDown.IsVisible = true;
	// }
	//
	// private void OnMoveUp(object? sender, RoutedEventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is Cutscene.Part part) {
	//         var index = cutscene!.Parts.IndexOf(part);
	//
	//         if (index > 0) {
	//             cutscene.Parts.Move(index, index - 1);
	//             LoadTree(cutscene);
	//             TreeParts.SelectedItem = selectedItem;
	//             foreach (TreeViewItem? item in TreeParts.Items.Cast<TreeViewItem?>()) // Where in the world can this be null? And why is intellisense recommending me to cast this way?
	//                 if (item!.Tag == part)
	//                     item.IsSelected = true;
	//         }
	//     }
	// }
	//
	// private void OnMoveDown(object? sender, RoutedEventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is Cutscene.Part part) {
	//         var index = cutscene!.Parts.IndexOf(part);
	//
	//         if (index < cutscene.Parts.Count - 1) {
	//             cutscene.Parts.Move(index, index + 1);
	//             LoadTree(cutscene);
	//             TreeParts.SelectedItem = selectedItem;
	//
	//             foreach (TreeViewItem? item in TreeParts.Items.Cast<TreeViewItem?>()) {
	//                 if (item!.Tag == part) {
	//                     item.IsSelected = true;
	//                 }
	//             }
	//         }
	//     }
	// }
	//
	// private void OnRename(object? sender, RoutedEventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem item) {
	//         var oldstatus = item.Header;
	//         var box = new TextBox() { Text = oldstatus!.ToString() };
	//         var renamed = false;
	//         item.Header = box;
	//         box.Focus();
	//
	//         box.KeyDown += (s, e) => { // Uhh, too many new things.
	//             if (e.Key == Key.Enter) {
	//                 renamed = true;
	//
	//                 if (cutscene!.Parts.Any(p => p.PartName == box.Text || (p.SubPartEntries != null && p.SubPartEntries!.Any(s => s.SubPartName == box.Text)))) {
	//                     if (box.Text == oldstatus.ToString()) {
	//                         item.Header = oldstatus;
	//                         return;
	//                     }
	//
	//                     MessageBoxManager.GetMessageBoxStandard("Error", "PartName already exists!", MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
	//                     return;
	//                 }
	//
	//                 if (item.Tag is ICommonEntries part && !string.IsNullOrEmpty(box.Text)) {
	//                     if (item.Tag is Cutscene.Part) {
	//                         ((Cutscene.Part)item.Tag!).PartName = box.Text!;
	//                     } else if (item.Tag is Abacus.SubPart) {
	//                         ((Abacus.SubPart)item.Tag!).SubPartName = box.Text!;
	//                     }
	//
	//                     oldstatus = item.Header = box.Text;
	//                     item.IsSelected = true;
	//                 }
	//             }
	//         };
	//
	//         box.LostFocus += (s, e) => {
	//             if (!renamed) { // This is because a little bit of time is needed after pressing Enter, otherwise you'll always see the first name when renaming.
	//                 item.Header = oldstatus;
	//             } else {
	//                 renamed = false;
	//             }
	//         };
	//     }
	// }
	//
	// private void OnAddSubPart(object? sender, RoutedEventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem item && item.Tag is Cutscene.Part part) {
	//         part.SubPartEntries ??= [];
	//         while (part.SubPartEntries.Any(s => s.SubPartName == "New SubPart" + counter)) {// Nice.
	//             counter++;
	//         }
	//         part.SubPartEntries.Add(new Abacus.SubPart("New SubPart" + counter));
	//         counter = 0;
	//         LoadTree(cutscene!); // I have to do this.
	//     }
	// }
	//
	// private void OnDelete(object? sender, RoutedEventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem item) {
	//
	//         var part = (ICommonEntries)item.Tag!;
	//         if (part is Abacus.SubPart part1) {
	//             var parent = (TreeViewItem?)item.Parent;
	//             var realpart = (Cutscene.Part)parent?.Tag!;
	//             realpart.SubPartEntries?.Remove(part1);
	//             parent?.Items.Remove(item);
	//         } else {
	//             cutscene!.Parts.Remove((Cutscene.Part)part);
	//             TreeParts.Items.Remove(item);
	//         }
	//
	//     }
	// }
	//
	// private void CheckPart(object? sender, EventArgs e) {
	//     if (TreeParts.SelectedItem is TreeViewItem item) {
	//         if (item.Tag is Cutscene.Part) {
	//             AddSubPartButton.IsVisible = true;
	//         } else {
	//             AddSubPartButton.IsVisible = false;
	//         }
	//     }
	// }

	// private void ExpandTrees(TreeViewItem parent) {
	// 	// TO DO: When you rename a part it will close the tree so the idea is to avoid that.
	// }

}
