using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.Collections.Generic;
using Abacus;
using System.Diagnostics;
using System.IO;
using Tmds.DBus.Protocol;
using MsBox.Avalonia;
using Avalonia.Input;
using System.Linq;
using Hack.io.Utility;


namespace CutsceneMaker
{
    public partial class MainWindow : Window
    {
        private string folderPath = string.Empty;
        private Cutscene? cut;
        private bool isNew;
        private MenuFlyout? PartsFlyout;
        private int counter;
        public MainWindow()
        {
            InitializeComponent();
            PartsFlyout = this.FindResource("PartsFlyout") as MenuFlyout;
        }
        private async void OnClickNew(object sender, RoutedEventArgs e)
        {
            if (cut != null)
            {
                var result = await MessageBoxManager.GetMessageBoxStandard("Are you sure?", "Are you sure you want to create a new cutscene? All unsaved changes will be lost.", MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowAsync();
                if (result == MsBox.Avalonia.Enums.ButtonResult.No || result == MsBox.Avalonia.Enums.ButtonResult.None)
                    return;
            }
            var d = new CutsceneNameDialog();
            await d.ShowDialog(this);
            if (string.IsNullOrEmpty(d.CutsceneName))
                return;
            cut = Cutscene.NewCutsceneFromTemplate(d.CutsceneName!);
            LoadTree(cut!);
            await MessageBoxManager.GetMessageBoxStandard("Success", "Created new cutscene: " + cut.CutsceneName, MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
            isNew = true;
            SaveButton.IsEnabled = false;
        }
        private async void OnClickLoad(object? sender, RoutedEventArgs e)
        {
            if (cut != null)
            {
                var result = await MessageBoxManager.GetMessageBoxStandard("Are you sure?", "Are you sure you want to load a new cutscene? All unsaved changes will be lost.", MsBox.Avalonia.Enums.ButtonEnum.YesNo).ShowAsync();
                if (result == MsBox.Avalonia.Enums.ButtonResult.No || result == MsBox.Avalonia.Enums.ButtonResult.None)
                    return;
            }
            FilePickerFileType ff = new("(Time) Binary Comma Separated Value") { Patterns = ["*Time.bcsv"] };
            var filePath = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { FileTypeFilter = [ff], AllowMultiple = false });

            if (filePath != null && filePath.Count > 0)
            {
                var localPath = filePath[0].TryGetLocalPath();
                if (!string.IsNullOrEmpty(localPath))
                {
                    cut = Cutscene.NewCutsceneFromFiles(localPath);
                    LoadTree(cut);
                    folderPath = Path.GetDirectoryName(localPath)!;
                    SaveButton.IsEnabled = true;
                }
            }
        }

        private void OnClickSave(object? sender, RoutedEventArgs e)
        {
            if (folderPath != null && cut != null)
            {
                cut.SaveAll(folderPath);
                MessageBoxManager.GetMessageBoxStandard("Saved", "Saved all files to " + folderPath, MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
            }
        }
        private async void OnClickSaveAs(object? sender, RoutedEventArgs e)
        {
            if (cut != null)
            {
                var folderPathResult = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions { AllowMultiple = false });
                if (folderPathResult != null && folderPathResult.Count > 0)
                {
                    var selectedPath = folderPathResult[0].TryGetLocalPath();
                    if (!string.IsNullOrEmpty(selectedPath))
                    {
                        cut.SaveAll(selectedPath);
                        await MessageBoxManager.GetMessageBoxStandard("Saved", "Saved all files to " + selectedPath, MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                        if (isNew)
                        {
                            folderPath = selectedPath;
                            isNew = false;
                            SaveButton.IsEnabled = true;
                        }
                    }
                }
            }
        }

        private void LoadTree(Cutscene cut)
        {
            TreeParts.Items.Clear();
            foreach (var part in cut.Parts)
            {
                TreeViewItem item = new() { Header = part.PartName, Tag = part, ContextFlyout = PartsFlyout };
                if (part.SubPartEntries != null)
                    foreach (Abacus.SubPart subPart in part.SubPartEntries)
                        item.Items.Add(new TreeViewItem() { Header = subPart.SubPartName, Tag = subPart, ContextFlyout = PartsFlyout });
                TreeParts.Items.Add(item);
            }
        }
        private void LoadTabControl(ICommonEntries part)
        {
            var playertab = new Player(part);
            var cameratab = new Camera(part);
            var actiontab = new Action(part);
            var soundtab = new Sound(part);
            var wipetab = new Wipe(part);
            if (part is Cutscene.Part part1)
            {
                MainTab.Header = "Time";
                MainTab.Content = new Time(part1);
            }
            else
            {
                MainTab.Header = "SubPart";
                MainTab.Content = new SubPart((Abacus.SubPart)part);
            }

            PlayerTab.Content = playertab;
            CameraTab.Content = cameratab;
            ActionTab.Content = actiontab;
            SoundTab.Content = soundtab;
            WipeTab.Content = wipetab;
        }
        private void OnTreeViewSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem selectedItem)
            {
                LoadTabControl((ICommonEntries)selectedItem.Tag!);
            }
        }
        private void OnClickCreatePart(object? sender, RoutedEventArgs e)
        {
            PartNameBox.Clear();
            if (cut != null)
            {
                PartNameButton.IsVisible = false;
                PartNameBox.IsVisible = true;
                ArrowUp.IsVisible = false;
                ArrowDown.IsVisible = false;
                PartNameBox.Focus();
            }
        }
        private void OnEnterClick(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrEmpty(PartNameBox.Text))
            {
                if (cut!.Parts.Any(p => p.PartName == PartNameBox.Text!))
                {
                    MessageBoxManager.GetMessageBoxStandard("Error", "PartName already exists!", MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                    return;
                }
                cut!.Parts.Add(new Cutscene.Part(PartNameBox.Text!));
                LoadTree(cut); // I'm not going to manually add the Part.
                PartNameButton.IsVisible = true;
                PartNameBox.IsVisible = false;
            }
        }
        private void OnLostFocusCreatePart(object? sender, RoutedEventArgs e)
        {
            PartNameButton.IsVisible = true;
            PartNameBox.IsVisible = false;
            ArrowUp.IsVisible = true;
            ArrowDown.IsVisible = true;
        }
        private void OnMoveUp(object? sender, RoutedEventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is Cutscene.Part part)
            {
                var index = cut!.Parts.IndexOf(part);
                if (index > 0)
                {
                    cut.Parts.Move(index, index - 1);
                    LoadTree(cut);
                    TreeParts.SelectedItem = selectedItem;
                    foreach (TreeViewItem? item in TreeParts.Items.Cast<TreeViewItem?>()) // Where in the world can this be null? And why is intellisense recommending me to cast this way?
                        if (item!.Tag == part)
                            item.IsSelected = true;
                }
            }
        }
        private void OnMoveDown(object? sender, RoutedEventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem selectedItem && selectedItem.Tag is Cutscene.Part part)
            {
                var index = cut!.Parts.IndexOf(part);
                if (index < cut.Parts.Count - 1)
                {
                    cut.Parts.Move(index, index + 1);
                    LoadTree(cut);
                    TreeParts.SelectedItem = selectedItem;
                    foreach (TreeViewItem? item in TreeParts.Items.Cast<TreeViewItem?>())
                        if (item!.Tag == part)
                            item.IsSelected = true;
                }
            }
        }
        private void OnRename(object? sender, RoutedEventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem item)
            {
                var oldstatus = item.Header;
                var box = new TextBox() { Text = oldstatus!.ToString() };
                var renamed = false;
                item.Header = box;
                box.Focus();
                box.KeyDown += (s, e) => // Uhh, too many new things.
                {
                    if (e.Key == Key.Enter)
                    {
                        renamed = true;
                        if (cut!.Parts.Any(p => p.PartName == box.Text || (p.SubPartEntries != null && p.SubPartEntries!.Any(s => s.SubPartName == box.Text))))
                        {
                            if (box.Text == oldstatus.ToString())
                            {
                                item.Header = oldstatus;
                                return;
                            }
                            MessageBoxManager.GetMessageBoxStandard("Error", "PartName already exists!", MsBox.Avalonia.Enums.ButtonEnum.Ok).ShowAsync();
                            return;
                        }
                        if (item.Tag is ICommonEntries part && !string.IsNullOrEmpty(box.Text))
                        {
                            if (item.Tag is Cutscene.Part)
                                ((Cutscene.Part)item.Tag!).PartName = box.Text!;
                            else if (item.Tag is Abacus.SubPart)
                                ((Abacus.SubPart)item.Tag!).SubPartName = box.Text!;

                            oldstatus = item.Header = box.Text;
                            item.IsSelected = true;
                        }
                    }
                };
                box.LostFocus += (s, e) =>
                {
                    if (!renamed)   // This is because a little bit of time is needed after pressing Enter, otherwise you'll always see the first name when renaming.
                        item.Header = oldstatus;
                    else
                        renamed = false;
                };
            }
        }
        private void OnAddSubPart(object? sender, RoutedEventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem item && item.Tag is Cutscene.Part part)
            {
                part.SubPartEntries ??= [];
                while (part.SubPartEntries.Any(s => s.SubPartName == "New SubPart" + counter)) // Nice.
                    counter++;
                part.SubPartEntries.Add(new Abacus.SubPart("New SubPart" + counter));
                counter = 0;
                LoadTree(cut!); // I have to do this.
            }
        }
        private void OnDelete(object? sender, RoutedEventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem item)
            {
                var part = (ICommonEntries)item.Tag!;
                if (part is Abacus.SubPart part1)
                {
                    var parent = (TreeViewItem?)item.Parent;
                    var realpart = (Cutscene.Part)parent?.Tag!;
                    realpart.SubPartEntries?.Remove(part1);
                    parent?.Items.Remove(item);
                }
                else
                {
                    cut!.Parts.Remove((Cutscene.Part)part);
                    TreeParts.Items.Remove(item);
                }
            }
        }
        private void CheckPart(object? sender, EventArgs e)
        {
            if (TreeParts.SelectedItem is TreeViewItem item)
                if (item.Tag is Cutscene.Part)
                    AddSubPartButton.IsVisible = true;
                else AddSubPartButton.IsVisible = false;
        }
        private void ExpandTrees(TreeViewItem parent)
        {
            // TO DO: When you rename a part it will close the tree so the idea is to avoid that.
        }

    }
}