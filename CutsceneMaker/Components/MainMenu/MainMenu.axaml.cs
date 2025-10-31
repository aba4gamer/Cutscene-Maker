using System;

using Avalonia;
using Avalonia.Controls;



namespace CutsceneMakerUI;

public partial class MainMenu : UserControl
{

	public MainMenu()
	{
		InitializeComponent();
	}

	public MainMenu(EventHandler<Avalonia.Interactivity.RoutedEventArgs> OnNewArchiveClick, EventHandler<Avalonia.Interactivity.RoutedEventArgs> OnOpenArchiveClick)
	{
		InitializeComponent();

		NewArchiveBtn.Click += OnNewArchiveClick;
		OpenArchiveBtn.Click += OnOpenArchiveClick;
	}
}
