using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;

namespace CutsceneMaker;

public partial class Player : UserControl
{
    private IDisposable? _posNameSubscription;
    private IDisposable? _bckNameSubscription;
    private IDisposable? _visibleSubscription;

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

            SubscribeToChanges(part);
        }

        IsPlayerEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(isChecked =>
        {
            if (isChecked == true)
            {
                part.PlayerEntry ??= new Abacus.Player();
                SetControlsEnabled(true);

                part.PlayerEntry.PosName = PosName.Text ?? string.Empty;
                part.PlayerEntry.BckName = BckName.Text ?? string.Empty;
                part.PlayerEntry.Visible = Visible.IsChecked == true ? 1 : 0;

                SubscribeToChanges(part);
            }
            else
            {
                part.PlayerEntry = null;
                SetControlsEnabled(false);

                DisposeSubscriptions();
            }
        });
    }

    private void SetControlsEnabled(bool isEnabled)
    {
        PosName.IsEnabled = isEnabled;
        BckName.IsEnabled = isEnabled;
        Visible.IsEnabled = isEnabled;
    }

    private void SubscribeToChanges(ICommonEntries part)
    {
        DisposeSubscriptions();

        _posNameSubscription = PosName.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.PlayerEntry.PosName = text ?? string.Empty);

        _bckNameSubscription = BckName.GetObservable(TextBox.TextProperty)
            .Subscribe(text => part.PlayerEntry.BckName = text ?? string.Empty);

        _visibleSubscription = Visible.GetObservable(CheckBox.IsCheckedProperty)
            .Subscribe(isChecked => part.PlayerEntry.Visible = isChecked == true ? 1 : 0);
    }

    private void DisposeSubscriptions()
    {
        _posNameSubscription?.Dispose();
        _bckNameSubscription?.Dispose();
        _visibleSubscription?.Dispose();
    }
}
