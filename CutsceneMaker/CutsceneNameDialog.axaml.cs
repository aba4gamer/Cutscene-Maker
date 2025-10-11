using System;

using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMaker;

public partial class CutsceneNameDialog : Window
{
	public string? CutsceneName = "";


	public CutsceneNameDialog()
	{
		InitializeComponent();
		KeyDown += OnKeyDown;
	}

	public CutsceneNameDialog(string title, string body, string? watermark, string? defaultName)
	{
		InitializeComponent();
		KeyDown += OnKeyDown;

		Title = title;
		Body.Text = body;
		CutsceneName = NameBox.Text = defaultName;
		NameBox.Watermark = watermark;
	}

	private void OnKeyDown(object? sender, KeyEventArgs e)
	{
		switch (e.Key) {
			case Key.Escape:
				CloseWin();
				break;
			case Key.Enter:
				DoSubmit();
				break;
		}
	}



	private void OnSubmit(object sender, RoutedEventArgs e)
	{
		DoSubmit();
	}

	private void OnCancel(object sender, RoutedEventArgs e)
	{
		CloseWin();
	}



	private void CloseWin()
	{
		CutsceneName = "";
		Close();
	}

	private void DoSubmit()
	{
		CutsceneName = NameBox.Text;

		if (string.IsNullOrEmpty(CutsceneName) || string.IsNullOrWhiteSpace(CutsceneName))
			return;
		Close();
	}

}
