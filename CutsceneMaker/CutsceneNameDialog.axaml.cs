using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.Threading.Tasks;

namespace CutsceneMaker;

public partial class CutsceneNameDialog : Window
{
    public string? CutsceneName;

    public CutsceneNameDialog()
    {
        InitializeComponent();
    }

    private void ButtonClick(object? sender, RoutedEventArgs e) => GetName();

    private void EnterClick(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
            GetName();
    }
    void GetName()
    {
        if (!string.IsNullOrEmpty(InputBox.Text))
        {
            CutsceneName = InputBox.Text;
            Close();
        }
    }

}
