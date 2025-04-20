using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;

namespace CutsceneMaker;

public partial class Player : UserControl
{
    /// <summary>
    /// ONLY USED FOR THE DESIGNER. DON'T USE!
    /// </summary>
    public Player()
    {
        InitializeComponent();
    }
    public Player(ICommonEntries part)
    {
        InitializeComponent();
        if (part.PlayerEntry != null)
        {
            IsPlayerEnabled.IsChecked = true;
            PosName.Text = part.PlayerEntry.PosName;
            BckName.Text = part.PlayerEntry.BckName;
            Visible.IsChecked = part.PlayerEntry.Visible == 1;

            // I really have NO IDEA of how does this work... but if it works it's useful.
            BckName.GetObservable(TextBox.TextProperty)
                .Subscribe(Observer.Create<string?>(text => part.PlayerEntry.BckName = text ?? string.Empty));
            PosName.GetObservable(TextBox.TextProperty)
                .Subscribe(Observer.Create<string?>(text => part.PlayerEntry.PosName = text ?? string.Empty));
            Visible.GetObservable(CheckBox.IsCheckedProperty)
                .Subscribe(Observer.Create<bool?>(isChecked => part.PlayerEntry.Visible = isChecked == true ? 1 : 0));
        }

        IsPlayerEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(Observer.Create<bool?>(isChecked =>
        {
            if (isChecked == true)
            {
                part.PlayerEntry ??= new Abacus.Player();
                PosName.IsEnabled = true;
                BckName.IsEnabled = true;
                Visible.IsEnabled = true;

                part.PlayerEntry.PosName = PosName.Text ?? string.Empty;
                part.PlayerEntry.BckName = BckName.Text ?? string.Empty;
                part.PlayerEntry.Visible = Visible.IsChecked == true ? 1 : 0;
            }
            else
            {
                part.PlayerEntry = null;
                PosName.IsEnabled = false;
                BckName.IsEnabled = false;
                Visible.IsEnabled = false;
            }
        }));
    }
}
