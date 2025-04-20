using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Abacus;
using System.Reactive;
using System;
using System.Reactive.Linq;

namespace CutsceneMaker;

public partial class Action : UserControl
{
    /// <summary>
    /// ONLY USED FOR THE DESIGNER. DON'T USE!
    /// </summary>
    public Action()
    {
        InitializeComponent();
    }
    public Action(ICommonEntries part)
    {
        InitializeComponent();
        if (part.ActionEntry != null)
        {
            IsActionEnabled.IsChecked = true;
            CastName.Text = part.ActionEntry.CastName;
            CastID.Value = part.ActionEntry.CastID;
            ActionType.Value = part.ActionEntry.ActionType;
            PosName.Text = part.ActionEntry.PosName;
            AnimName.Text = part.ActionEntry.AnimName;

            // I really have NO IDEA of how does this work... but if it works it's useful.
            CastName.GetObservable(TextBox.TextProperty).Subscribe(text => part.ActionEntry.CastName = text ?? string.Empty);
            CastID.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => part.ActionEntry.CastID = value.HasValue ? (int)value.Value : -1);
            ActionType.GetObservable(NumericUpDown.ValueProperty).Subscribe(value => part.ActionEntry.ActionType = value.HasValue ? (int)value.Value : -1);
            PosName.GetObservable(TextBox.TextProperty).Subscribe(text => part.ActionEntry.PosName = text ?? string.Empty);
            AnimName.GetObservable(TextBox.TextProperty).Subscribe(text => part.ActionEntry.AnimName = text ?? string.Empty);
        }
        IsActionEnabled.GetObservable(CheckBox.IsCheckedProperty).Subscribe(Observer.Create<bool?>(isChecked =>
        {
            if (isChecked == true)
            {
                part.ActionEntry ??= new Abacus.Action();
                CastName.IsEnabled = true;
                CastID.IsEnabled = true;
                ActionType.IsEnabled = true;
                PosName.IsEnabled = true;
                AnimName.IsEnabled = true;

                part.ActionEntry.CastName = CastName.Text ?? string.Empty;
                part.ActionEntry.CastID = CastID.Value.HasValue ? (int)CastID.Value.Value : -1;
                part.ActionEntry.ActionType = ActionType.Value.HasValue ? (int)ActionType.Value.Value : -1;
                part.ActionEntry.PosName = PosName.Text ?? string.Empty;
                part.ActionEntry.AnimName = AnimName.Text ?? string.Empty;
            }
            else
            {
                part.ActionEntry = null;
                CastName.IsEnabled = false;
                CastID.IsEnabled = false;
                ActionType.IsEnabled = false;
                PosName.IsEnabled = false;
                AnimName.IsEnabled = false;
            }
        }));
    }
}
