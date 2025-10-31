using System;
using System.Reactive;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Markup.Xaml;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;



namespace CutsceneMaker;

public partial class CutsceneNameDialog : Window
{
	public string? CutsceneName = null;
	private bool IsDisabledName = false;
	private IDisposable? _nameBoxTextSubscription;


	public CutsceneNameDialog()
	{
		InitializeComponent();
		KeyDown += OnKeyDown;
	}

	public CutsceneNameDialog(string title, string body, string? watermark, string? defaultName, List<string>? disabledNames, string? disabledNameMessage)
	{
		InitializeComponent();
		KeyDown += OnKeyDown;

		Title = title;
		Body.Text = body;
		CutsceneName = NameBox.Text = defaultName;
		NameBox.Watermark = watermark;

		DisabledName.Foreground = Brush.Parse("#0000");
		if (disabledNameMessage != null)
			DisabledName.Text = disabledNameMessage;
		else
			DisabledName.Text = "You can't add this name!";
		_nameBoxTextSubscription?.Dispose();
		_nameBoxTextSubscription = NameBox.GetObservable(TextBox.TextProperty)
			.Subscribe(val =>
			{
				if (disabledNames != null && NameBox.Text != null)
				{
					foreach (string disabledName in disabledNames)
					{
						if (disabledName.Trim() == NameBox.Text.Trim())
						{
							IsDisabledName = true;
							NameBox.Foreground = Brush.Parse("#942828");
							DisabledName.Foreground = Brush.Parse("#bf5f5f");
							SubmitButton.IsEnabled = false;
							break;
						}
						else
						{
							IsDisabledName = false;
							NameBox.Foreground = Brush.Parse("#fff");
							DisabledName.Foreground = Brush.Parse("#0000");
							SubmitButton.IsEnabled = true;
						}
					}
				}
			});
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		NameBox.Focus();
		NameBox.SelectAll();
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
		CutsceneName = null;
		Close();
	}

	private void DoSubmit()
	{
		if (NameBox.Text == null)
			return;

		string cutsceneName = NameBox.Text.Trim();
		if (string.IsNullOrEmpty(cutsceneName) || string.IsNullOrWhiteSpace(cutsceneName) || IsDisabledName)
			return;

		CutsceneName = cutsceneName;
		Close();
	}

}
