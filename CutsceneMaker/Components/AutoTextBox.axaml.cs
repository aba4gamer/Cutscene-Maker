using System;
using System.Collections.Generic;

using Avalonia;
using Avalonia.Input;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class AutoTextBox : UserControl
{

	private IDisposable? _textBoxSubscription;
	public List<string> AutoCompletion = [];


	public AutoTextBox()
	{
		InitializeComponent();

		Main.AddHandler(KeyDownEvent, TextBox_OnKeyDown, RoutingStrategies.Tunnel);
		Main.AddHandler(PointerPressedEvent, TextBox_OnPointerPressed, RoutingStrategies.Tunnel);

		_textBoxSubscription?.Dispose();
		_textBoxSubscription = Main.GetObservable(TextBox.TextProperty)
			.Subscribe(text =>
			{
				if (text == null)
					return;

				FlyoutStackPanel.Children.Clear();
				foreach (string option in AutoCompletion)
				{
					if (option.ToLower().StartsWith(text.ToLower()))
					{
						ComboBoxItem itm = new ComboBoxItem { Content = option };
						itm.AddHandler(PointerPressedEvent, ComboBoxItem_OnPointerPressed, RoutingStrategies.Tunnel);
						itm.AddHandler(KeyDownEvent, ComboBoxItem_OnKeyDown, RoutingStrategies.Tunnel);
						FlyoutStackPanel.Children.Add(itm);
					}
				}

				if (FlyoutStackPanel.Children.Count > 0)
					FlyoutBase.ShowAttachedFlyout(Main);
				else
					FlyoutBase.GetAttachedFlyout(Main)?.Hide();
				Main.Focus();
			});
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		FlyoutBase.GetAttachedFlyout(Main)?.Hide();

		if (FlyoutStackPanel.Children.Count > 0)
			return;

		foreach (string option in AutoCompletion)
		{
			ComboBoxItem itm = new ComboBoxItem { Content = option };
			itm.AddHandler(PointerPressedEvent, ComboBoxItem_OnPointerPressed, RoutingStrategies.Tunnel);
			itm.AddHandler(KeyDownEvent, ComboBoxItem_OnKeyDown, RoutingStrategies.Tunnel);
			FlyoutStackPanel.Children.Add(itm);
		}
	}


	private void ChoseComboBox(ComboBoxItem combo)
	{
		string name = (string) combo.Content!;
		Main.Text = name;
		Main.SelectionStart = name.Length;
		Main.SelectionEnd = name.Length;

		FlyoutBase.GetAttachedFlyout(Main)?.Hide();
	}

	private void ComboBoxItem_OnPointerPressed(object? sender, RoutedEventArgs e)
	{
		if (sender != null && sender is ComboBoxItem combo)
			ChoseComboBox(combo);
	}

	private void ComboBoxItem_OnKeyDown(object? sender, RoutedEventArgs e)
	{
		if (sender != null && sender is ComboBoxItem combo)
		{
			switch (((KeyEventArgs) e).Key)
			{
				case Key.Enter:
				case Key.Space:
					e.Handled = true;
					ChoseComboBox(combo);
					break;
				case Key.Down:
					e.Handled = true;
					int i = FlyoutStackPanel.Children.IndexOf(combo)+1;
					int count = FlyoutStackPanel.Children.Count-1;

					if (i <= count && i >= 0)
						FlyoutStackPanel.Children[i].Focus(NavigationMethod.Tab);
					break;
				case Key.Up:
					e.Handled = true;
					i = FlyoutStackPanel.Children.IndexOf(combo)-1;
					count = FlyoutStackPanel.Children.Count-1;

					if (i <= count && i >= 0)
						FlyoutStackPanel.Children[i].Focus(NavigationMethod.Tab);
					break;
				case Key.Tab:
				case Key.Escape:
					break;

				default:
					Main.Focus();
					Main.RaiseEvent(e);
					break;
			}
		}
	}

	private void TextBox_OnKeyDown(object? sender, RoutedEventArgs e)
	{
		switch (((KeyEventArgs) e).Key)
		{
			case Key.Down:
			case Key.Tab:
				if (FlyoutStackPanel.Children.Count > 0 && FlyoutBase.GetAttachedFlyout(Main)!.IsOpen)
				{
					e.Handled = true;
					FlyoutStackPanel.Children[0].Focus(NavigationMethod.Tab);
				}
				break;
			case Key.Up:
				if (FlyoutStackPanel.Children.Count > 0 && FlyoutBase.GetAttachedFlyout(Main)!.IsOpen)
				{
					e.Handled = true;
					FlyoutStackPanel.Children[FlyoutStackPanel.Children.Count-1].Focus(NavigationMethod.Tab);
				}
				break;
			case Key.Escape:
				if (FlyoutStackPanel.Children.Count > 0 && FlyoutBase.GetAttachedFlyout(Main)!.IsOpen)
				{
					e.Handled = true;
					FlyoutBase.GetAttachedFlyout(Main)!.Hide();
				}
				break;
		}
	}

	private void TextBox_OnPointerPressed(object? sender, RoutedEventArgs e)
	{
		FlyoutStackPanel.MinWidth = Main.Bounds.Width - 24;

		if (FlyoutStackPanel.Children.Count > 0)
			return;

		foreach (string option in AutoCompletion)
		{
			ComboBoxItem itm = new ComboBoxItem { Content = option };
			itm.AddHandler(PointerPressedEvent, ComboBoxItem_OnPointerPressed, RoutingStrategies.Tunnel);
			itm.AddHandler(KeyDownEvent, ComboBoxItem_OnKeyDown, RoutingStrategies.Tunnel);
			FlyoutStackPanel.Children.Add(itm);
		}

		FlyoutBase.ShowAttachedFlyout(Main);
	}
}
