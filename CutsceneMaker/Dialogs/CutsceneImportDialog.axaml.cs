using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

using Abacus;

using CutsceneMakerUI;



namespace CutsceneMaker;

public partial class CutsceneImportDialog : Window
{
	public List<Cutscene> Cutscenes { get; private set; } = [];
	public bool IsSMG1 { get; private set; } = false;
	private CutsceneArchive? archive = null;

	private List<IDisposable> Subscribers = [];
	private List<Control> LoadedCutscenes = [];
	private IDisposable? _searchBoxTextSubscription = null;
	private int WarningNumber = 0;


	public CutsceneImportDialog()
	{
		InitializeComponent();
	}

	public CutsceneImportDialog(string filePath)
	{
		InitializeComponent();
		archive = CutsceneArchive.LoadArchive(filePath).GetResult();
		IsSMG1 = archive.IsSMG1;
		Title = $"Importing cutscenes from '{filePath}'";

		_searchBoxTextSubscription?.Dispose();
		_searchBoxTextSubscription = SearchBox.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				CutsceneList.Children.Clear();
				if (text == null)
				{
					foreach (Control imp in LoadedCutscenes)
						CutsceneList.Children.Add(imp);
					return;
				}

				foreach (Control im in LoadedCutscenes)
				{
					if (im is ImporterCutscene imp)
						if (imp.CutsceneName.Text!.ToLower().Contains(text.ToLower()))
							CutsceneList.Children.Add(imp);
					if (im is ImporterCutsceneWarning impp)
						if (impp.CutsceneName.Text!.ToLower().Contains(text.ToLower()))
							CutsceneList.Children.Add(impp);
				}
			});
		CutsceneList.Children.Clear();
		foreach (string cutsceneName in archive.CutsceneNames)
		{
			Cutscene cutscene = archive.GetCutsceneWithName(cutsceneName);

			if (MainWindow.Instance!.Core.GetArchive().IsSMG1 && !archive.IsSMG1)
			{
				string warn = "";

				foreach (Cutscene.Part part in cutscene.Parts)
				{

					string partWarning = "";
					if (part.TimeEntry.WaitUserInputFlag > 0)
					{
						partWarning = $"The part '{part.PartName}' has:\n  · The 'WaitUserInputFlag' enabled which is not supported in SMG1";
					}

					if (part.ActionEntry != null && part.ActionEntry.ActionType > 13)
					{
						string warning = $"The action type {part.ActionEntry.ActionType} is not supported in SMG1";
						if (partWarning == "")
							partWarning = $"The part '{part.PartName}' has:\n  · " + warning;
						else
							partWarning += ";\n  · " + warning;
					}

					string sPartWarning = "";
					if (part.SubPartEntries != null)
						foreach (SubPart sPart in part.SubPartEntries)
						{
							if (sPart.ActionEntry != null && sPart.ActionEntry.ActionType > 13)
							{
								string warning = $"the action type {sPart.ActionEntry.ActionType} is not supported in SMG1";
								if (sPartWarning == "")
									sPartWarning = $"The sub part '{sPart.SubPartName}' has " + warning;
								else
									sPartWarning += $";\n      · The sub part '{sPart.SubPartName}' has " + warning;
							}
						}

					if (sPartWarning != "")
					{
						if (partWarning == "")
							partWarning = $"The part '{part.PartName}' has:\n  · Some SubParts:\n      · " + sPartWarning;
						else
							partWarning += ";\n  · Some SubParts:\n      · " + sPartWarning;
					}

					if (partWarning != "")
						warn += partWarning + ".\n\n";
				}

				if (warn.Trim() != "")
				{
					ImporterCutsceneWarning cImportWarning = new(cutsceneName, warn.Trim());
					Subscribers.Add(cImportWarning.SelectedCheck.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
						{
							SelectWarningCutscene(cImportWarning);
						}
					));
					LoadedCutscenes.Add(cImportWarning);
					CutsceneList.Children.Add(cImportWarning);
					continue;
				}
			}

			ImporterCutscene cImport = new(cutsceneName);
			Subscribers.Add(cImport.SelectedCheck.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
			{
				SelectCutscene(cImport);
			}
			));
			LoadedCutscenes.Add(cImport);
			CutsceneList.Children.Add(cImport);
		}
	}

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		foreach (IDisposable disp in Subscribers)
			disp.Dispose();
		if (!e.IsProgrammatic)
			Cutscenes.Clear();
	}


	public void OnCancel(object? sender, RoutedEventArgs e)
	{
		Cutscenes.Clear();
		Close();
	}

	public void OnImport(object? sender, RoutedEventArgs e)
	{
		ImportSubmit();
	}

	public void ImportSubmit()
	{


		Close();
	}

	private void OnSelectAll(object sender, RoutedEventArgs e)
	{
		foreach (Control ctrl in LoadedCutscenes)
			if (ctrl is ImporterCutscene imp)
				imp.SelectedCheck.IsChecked = true;
			else if (ctrl is ImporterCutsceneWarning impp)
				impp.SelectedCheck.IsChecked = true;
	}

	private void OnSelectAllViewable(object sender, RoutedEventArgs e)
	{
		foreach (Control ctrl in CutsceneList.Children)
			if (ctrl is ImporterCutscene imp)
				imp.SelectedCheck.IsChecked = true;
			else if (ctrl is ImporterCutsceneWarning impp)
				impp.SelectedCheck.IsChecked = true;
	}

	private void OnDeselectAll(object sender, RoutedEventArgs e)
	{
		foreach (Control ctrl in LoadedCutscenes)
			if (ctrl is ImporterCutscene imp)
				imp.SelectedCheck.IsChecked = false;
			else if (ctrl is ImporterCutsceneWarning impp)
				impp.SelectedCheck.IsChecked = false;
	}

	private void OnDeselectAllViewable(object sender, RoutedEventArgs e)
	{
		foreach (Control ctrl in CutsceneList.Children)
			if (ctrl is ImporterCutscene imp)
				imp.SelectedCheck.IsChecked = false;
			else if (ctrl is ImporterCutsceneWarning impp)
				impp.SelectedCheck.IsChecked = false;
	}



	private void SelectCutscene(ImporterCutscene cImport)
	{
		if (cImport.SelectedCheck.IsChecked == null || cImport.CutsceneName.Text == null)
			return;

		bool isChecked = cImport.SelectedCheck.IsChecked == true;
		string cutsceneName = cImport.CutsceneName.Text!;

		if (isChecked)
			Cutscenes.Add(archive!.GetCutsceneWithName(cutsceneName));
		else
			Cutscenes.Remove(archive!.GetCutsceneWithName(cutsceneName));

		PrintSelectedCutscenes();
	}

	private void SelectWarningCutscene(ImporterCutsceneWarning cImport)
	{
		if (cImport.SelectedCheck.IsChecked == null || cImport.CutsceneName.Text == null)
			return;

		bool isChecked = cImport.SelectedCheck.IsChecked == true;
		string cutsceneName = cImport.CutsceneName.Text!;

		if (isChecked)
		{
			WarningNumber++;
			WarningSelected.Foreground = Avalonia.Media.Brushes.OrangeRed;
			WarningSelectedImage.IsVisible = true;
			Cutscenes.Add(archive!.GetCutsceneWithName(cutsceneName));
		}
		else
		{
			WarningNumber--;
			if (WarningNumber < 1)
			{
				WarningNumber = 0;
				WarningSelected.Foreground = Avalonia.Media.Brushes.Transparent;
				WarningSelectedImage.IsVisible = false;
			}
			Cutscenes.Remove(archive!.GetCutsceneWithName(cutsceneName));
		}

		PrintSelectedCutscenes();
	}

	private void PrintSelectedCutscenes()
	{
		if (Cutscenes.Count < 1)
			ImpCutscenes.Text = "Selected cutscenes (0): None.";
		else
		{
			string cutscenesShort = "";

			int i = 0;
			foreach (Cutscene cuts in Cutscenes)
			{
				i++;
				if (i > 7)
					break;
				cutscenesShort += $"{cuts.CutsceneName}', ";

			}

			string hoverMe = "";
			if (i > 7)
				hoverMe = ", (And more...)";
			ImpCutscenes.Text = $"Selected cutscenes ({Cutscenes.Count}): '{cutscenesShort.Substring(0, cutscenesShort.Length-2)}{hoverMe}.";
		}
	}
}
