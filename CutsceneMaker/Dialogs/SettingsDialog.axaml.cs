using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CutsceneMakerUI;



namespace CutsceneMaker;

public partial class SettingsDialog : Window
{
	private IDisposable? _baseGamePathTextSubscription;
	public Settings settings = new();
	public SettingsDialog()
	{
		InitializeComponent();
		BaseGamePathBox.Text = settings.VanillaGamePath;
        _baseGamePathTextSubscription = BaseGamePathBox.GetObservable(TextBox.TextProperty).Subscribe(val =>
		{
			validatePath(val!);
        });
    }

	protected override void OnClosing(WindowClosingEventArgs e)
	{
		MainWindow.Instance!.IsSettingsOpen = false;
	}

	public void OnKeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
	{
		if (e.Key == Avalonia.Input.Key.Escape)
		{
			Close();
		}
		if (e.Key == Avalonia.Input.Key.Enter)
		{
			Apply();
		}

	}

	public void OnCancel(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Close();
	}

	public void OnApply(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		Apply();
	}

	public void Apply()
	{
		settings.VanillaGamePath = BaseGamePathBox.Text;
		settings.saveSettings();
		Close();
	}
	public async void OnBrowse(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		FolderPickerOpenOptions ff = new() { Title = "Select the base game directory", AllowMultiple = false };
		var result = await StorageProvider.OpenFolderPickerAsync(ff);
		BaseGamePathBox.Text = result[0].Path.LocalPath;
	}
	void validatePath(string path)
	{
		if (Directory.Exists(path) && File.Exists(System.IO.Path.Combine(path, "SystemData/ObjNameTable.arc")))
		{

			PathValidatorBlock.Foreground = Avalonia.Media.Brushes.LimeGreen;
			PathValidatorBlock.Text = "Certified Vanilla Directory";
		}
		else if (string.IsNullOrEmpty(path))
		{
			PathValidatorBlock.Foreground = Avalonia.Media.Brushes.White;
            PathValidatorBlock.Text = "Put the path of the vanilla files. Usually they'll be inside \"DATA/files\"";
        }
		else
		{
			PathValidatorBlock.Foreground = Avalonia.Media.Brushes.OrangeRed;
			PathValidatorBlock.Text = "This doesn't seem to be a valid base game directory. But you decide";
		}
    }
}