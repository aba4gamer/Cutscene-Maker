using System;
using System.Collections.Generic;

using Abacus;

using Avalonia;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Interactivity;



namespace CutsceneMakerUI;

public partial class TimelineView : UserControl
{
	private TimelinePart? SelectedTimelinePart = null;
	private TimelinePart? SelectedTimelineSubPart = null;
	private IDisposable? _SubPartBoxSubscription;
	private bool _IsEditingSubPartIndex = false;
	private int _ZoomTimeline = 2;


	public TimelineView()
	{
		InitializeComponent();

		MainTimeline.ContextMenu = (ContextMenu) MainWindow.Instance!.FindResource("PartPanelCtx")!;
		ComboBox_Reset();

		MovePartBefore.IsEnabled = false;
		MovePartAfter.IsEnabled = false;

		_SubPartBoxSubscription?.Dispose();
		_SubPartBoxSubscription = SubPartComboBox.GetObservable(ComboBox.SelectedIndexProperty)
			.Subscribe(value => {
				if (_IsEditingSubPartIndex)
					return;

				int i = SubPartComboBox.SelectedIndex;

				if (i < 1)
				{
					SubTimeline.Children.Clear();
					if (MainWindow.Instance!.Core.HasSubPartSelected())
						SubPart_Selected_Deselect();
					return;
				}

				string subPartName = ComboBox_GetContent(i);
				if (SelectedTimelinePart != null)
				{
					SubPart subPart = MainWindow.Instance!.Core.GetSubPartByName(subPartName);

					SubPart_Render(subPart);
					MainWindow.Instance!.SubPart_Select(subPartName);
					TimelineSubPart_SetSelected((TimelinePart) SubTimeline.Children[1]);
				}
			});
	}





	#region Timeline
	public void Timeline_UpdateSteps(bool forced = false)
	{
		// Oh so good math, I won't even try to explain what this does, just know that it gets the job done.
		double windowWidth = MainWindow.Instance!.Width;
		int totalSteps = MainWindow.Instance!.Core.GetCutscene().GetMaxTotalSteps();

		int zoom = _ZoomTimeline * 5;
		int limit = totalSteps + zoom;
		int minWidth = ((int) (windowWidth * (1 / ((double) _ZoomTimeline)))) - 30;
		if (limit < minWidth)
			limit = minWidth;
		int diff = limit % 5;
		limit -= diff - 1;

		// For performance optimization, let's not draw 300+ elements each time
		// someone resizes the window or opens a menu item (because according
		// to Avalonia, that counts as a resize as well).
		int toAddChildren = (int) Math.Floor((limit * 0.1) * 2);
		if (toAddChildren == MainTimelineSteps.Children.Count && !forced)
			return;

		if (forced)
		{
			MainTimelineSteps.Children.Clear();
			SubTimelineSteps.Children.Clear();
		}

		if (toAddChildren > MainTimelineSteps.Children.Count || forced)
		{
			int count = MainTimelineSteps.Children.Count;
			int child = 0;

			int half = (int) (5 * (8 / (double) _ZoomTimeline));
			int full = (int) (10 * (8 / (double) _ZoomTimeline));

			half -= half % 5;
			full -= full % 10;
			full -= full % 5;
			for (int i = 1; i < limit; i++) {
				if (i % half == 0 && i % full != 0) {
					child++;

					if (child <= count)
						continue;

					MainTimelineSteps.Children.Add(new TimelineStepFrac(40));
					SubTimelineSteps.Children.Add(new TimelineStepFrac(40));
				}

				if (i % full == 0) {
					child++;

					if (child <= count)
						continue;

					MainTimelineSteps.Children.Add(new TimelineStep(i, 40));
					SubTimelineSteps.Children.Add(new TimelineStep(i, 40));
				}
			}
		}

		if (toAddChildren < MainTimelineSteps.Children.Count || forced)
		{
			int ii = MainTimelineSteps.Children.Count - 1;
			while (toAddChildren <= ii)
			{
				MainTimelineSteps.Children.RemoveAt(ii);
				SubTimelineSteps.Children.RemoveAt(ii);
				ii--;
			}
		}
	}
	#endregion





	#region PartHandling
	public void Part_RenderAll()
	{
		List<Cutscene.Part> parts = MainWindow.Instance!.Core.GetCutscene().Parts;
		MainTimeline.Children.Clear();

		if (!MainWindow.Instance!.Core.HasPartSelected())
		{
			MovePartBefore.IsEnabled = false;
			MovePartAfter.IsEnabled = false;
		}

		if (parts.Count < 1)
		{
			ComboBox_Reset();
			SubPart_Render(null);
			SubTimeline.ContextMenu = null;
			return;
		}

		foreach (Cutscene.Part part in parts)
		{
			int max = 8 / _ZoomTimeline;
			TimelinePart timelinePart = new(part, part.PartName, Math.Max(part.TimeEntry.TotalStep, max), false, _ZoomTimeline);

			if (MainWindow.Instance!.Core.HasPartSelected() && MainWindow.Instance!.Core.GetSelectedPartName() == part.PartName)
			{
				TimelinePart_SetSelected(timelinePart);
				timelinePart.Select(true);
			}
			if (MainWindow.Instance!.Core.HasSubPartSelected() && MainWindow.Instance!.Core.GetSelectedPartName() == part.PartName)
			{
				TimelinePart_SetSelected(timelinePart);
				timelinePart.SelectedSubPart();
			}

			MainTimeline.Children.Add(timelinePart);
		}

		if (!MainWindow.Instance!.Core.HasPartSelected())
			ComboBox_Reset();

		if (SubPartComboBox.SelectedIndex > 0 && MainWindow.Instance!.Core.HasSubPartSelected())
		{
			SubPart_Render(MainWindow.Instance!.Core.GetSubPartByName(ComboBox_GetContent(SubPartComboBox.SelectedIndex)));
			SelectedTimelineSubPart = (TimelinePart) SubTimeline.Children[1];
		}
		else if (SubPartComboBox.SelectedIndex > 0)
			SubPart_Render(MainWindow.Instance!.Core.GetSubPartByName(ComboBox_GetContent(SubPartComboBox.SelectedIndex)));
		else
			SubPart_Render(null);
	}

	public void TimelinePart_SetSelected(TimelinePart? timelinePart)
	{
		Part_ActivateAll();

		if (timelinePart == null)
		{
			ComboBox_Reset();
			SubPart_Render(null);
			SubTimeline.ContextMenu = null;
			return;
		}

		timelinePart.Select(true);
		SelectedTimelinePart = timelinePart;

		MainWindow.Instance!.Core.SetSelectedPart(timelinePart.PartName);
		SubTimeline.ContextMenu = (ContextMenu) MainWindow.Instance!.FindResource("SubPartPanelCtx")!;

		if (timelinePart.Parent! is DockPanel timelinePartParent)
		{
			int i = timelinePartParent.Children.IndexOf(timelinePart);
			int max = timelinePartParent.Children.Count - 1;

			MovePartBefore.IsEnabled = true;
			MovePartAfter.IsEnabled = true;

			if (i == 0)
				MovePartBefore.IsEnabled = false;
			if (i == max)
				MovePartAfter.IsEnabled = false;
		}

		if (SelectedTimelineSubPart != null)
			SubPart_Selected_Deselect();
		MainWindow.Instance!.Part_Select(timelinePart.PartName);
	}

	public void TimelinePart_SetSelectedByName(string partName)
	{
		foreach (object timelinePartObj in MainTimeline.Children)
		{
			if (timelinePartObj is TimelinePart timelinePart)
			{
				if (timelinePart.PartName == partName)
				{
					TimelinePart_SetSelected(timelinePart);
					break;
				}
			}
		}
	}

	private void Part_ActivateAll()
	{
		foreach (object obj in MainTimeline.Children)
		{
			if (obj is TimelinePart timelinePart)
				timelinePart.Select(false);
		}
	}

	public void Part_Selected_Deselect()
	{
		if (SelectedTimelinePart != null)
		{
			SelectedTimelinePart.Select(false);
			SelectedTimelinePart = null;
		}
	}

	public void Part_Selected_UpdateSteps(int step)
	{
		if (SelectedTimelinePart == null)
			return;

		SelectedTimelinePart.Border.Width = Math.Max(step * _ZoomTimeline, ((8 / _ZoomTimeline) * _ZoomTimeline));
	}

	public void Part_Selected_SetName(string name)
	{
		if (SelectedTimelinePart == null)
			return;

		SelectedTimelinePart.ChangeName(name);
	}
	#endregion





	#region SubPartHandling
	public void SubPart_Render(SubPart? subPart)
	{
		SubTimeline.Children.Clear();
		if (subPart == null)
			return;

		int space = MainWindow.Instance!.Core.GetStepUntilSelectedPart();

		SubTimeline.ContextMenu = (ContextMenu) MainWindow.Instance!.FindResource("SubPartPanelCtx")!;
		SubTimeline.Children.Add(new TimelineSubPartSpacer((space + subPart.MainPartStep) * _ZoomTimeline));
		int max = 8 / _ZoomTimeline;
		TimelinePart timelinePart = new(subPart, subPart.SubPartName, Math.Max(subPart.SubPartTotalStep, max), true, _ZoomTimeline);

		if (MainWindow.Instance!.Core.HasSubPartSelected() && MainWindow.Instance!.Core.GetSelectedSubPartName() == subPart.SubPartName)
			timelinePart.Select(true);

		SubTimeline.Children.Add(timelinePart);
	}

	public void TimelineSubPart_SetSelected(TimelinePart timelinePart)
	{
		if (SelectedTimelinePart == null)
			return;

		SubPart_ActivateAll();
		timelinePart.Select(true);
		SelectedTimelinePart.SelectedSubPart();
		SelectedTimelineSubPart = timelinePart;
		MainWindow.Instance!.SubPart_Select(timelinePart.PartName);
	}

	public void TimelineSubPart_SetSelectedByName(string subPartName)
	{
		foreach (object timelinePartObj in SubTimeline.Children)
		{
			if (timelinePartObj is TimelinePart timelinePart)
			{
				if (timelinePart.PartName == subPartName)
				{
					TimelineSubPart_SetSelected(timelinePart);
					ComboBox_SelectByName(subPartName);
					break;
				}
			}
		}
	}

	private void SubPart_ActivateAll()
	{
		foreach (object obj in SubTimeline.Children)
		{
			if (obj is TimelinePart)
			{
				TimelinePart timelinePart = (TimelinePart) obj;
				timelinePart.Select(false);
			}
		}
	}

	public void SubPart_Selected_Deselect()
	{
		if (SelectedTimelinePart == null || SelectedTimelineSubPart == null)
			return;

		SelectedTimelineSubPart.Select(false);
		MainWindow.Instance!.SubPart_Deselect(SelectedTimelinePart.PartName, SelectedTimelineSubPart.PartName);
		SelectedTimelinePart.Select(true);
		SelectedTimelineSubPart = null;
	}

	public void SubPart_Selected_UpdateSteps(int step)
	{
		if (SelectedTimelineSubPart == null)
			return;

		SelectedTimelineSubPart.Border.Width = Math.Max(step * _ZoomTimeline, ((8 / _ZoomTimeline) * _ZoomTimeline));
	}

	public void SubPart_Selected_UpdateSpaceSteps(int step)
	{
		if (SelectedTimelineSubPart == null)
			return;

		int space = MainWindow.Instance!.Core.GetStepUntilSelectedPart();
		if (SubTimeline.Children[0]! is TimelineSubPartSpacer timelineSpacer)
			timelineSpacer.Spacer.Width = (space + MainWindow.Instance!.Core.GetSelectedSubPart().MainPartStep) * _ZoomTimeline;
	}

	public void SubPart_Selected_SetName(string name)
	{
		if (SelectedTimelineSubPart == null)
			return;

		_IsEditingSubPartIndex = true;
		SelectedTimelineSubPart.ChangeName(name);
		int i = SubPartComboBox.SelectedIndex;
		ComboBox_SetContent(i, name);
		SubPartComboBox.SelectedIndex = 0;
		SubPartComboBox.SelectedIndex = i; // This is for updating the string shown in the selected box. Idk why it doesn't change when I change the Content.
		_IsEditingSubPartIndex = false;
	}
	#endregion





	#region ComboBoxHandling
	private string ComboBox_GetContent(int i)
	{
		object? maybeComboBox = SubPartComboBox.Items[i];
		if (maybeComboBox == null)
			throw new Exception($"There is no item at index {i}");
		if (!(maybeComboBox is ComboBoxItem))
			throw new Exception($"The item at index {i} is not a ComboBoxItem");

		ComboBoxItem comboItemPart = (ComboBoxItem) maybeComboBox;
		object? content = comboItemPart.Content;
		if (content == null)
			throw new Exception($"The content of the ComboBoxItem at {i} is null!");
		return (string) content;
	}

	private void ComboBox_SetContent(int i, string partName)
	{
		object? maybeComboBox = SubPartComboBox.Items[i];
		if (maybeComboBox == null)
			throw new Exception($"There is no item at index {i}");
		if (!(maybeComboBox is ComboBoxItem))
			throw new Exception($"The item at index {i} is not a ComboBoxItem");

		ComboBoxItem comboItemPart = (ComboBoxItem) maybeComboBox;
		comboItemPart.Content = partName;
	}

	public void ComboBox_AddSubParts(List<SubPart> subParts)
	{
		if (subParts.Count < 1)
		{
			ComboBox_Reset();
			return;
		}

		_IsEditingSubPartIndex = true;
		SubPartComboBox.Items.Clear();
		SubPartComboBox.Items.Add(new ComboBoxItem { Content = "-" });
		SubPartComboBox.IsEnabled = true;

		foreach (SubPart subPart in subParts)
		{
			SubPartComboBox.Items.Add(new ComboBoxItem { Content = subPart.SubPartName });
		}

		SubPartComboBox.SelectedIndex = 1;
		_IsEditingSubPartIndex = false;


		string subPartName = ComboBox_GetContent(1);
		if (SelectedTimelinePart != null)
			SubPart_Render(MainWindow.Instance!.Core.GetSubPartByName(subPartName));
		else
			SubPart_Render(null);
	}

	public void ComboBox_Reset()
	{
		SubPartComboBox.Items.Clear();
		SubPartComboBox.Items.Add(new ComboBoxItem { Content = "No SubParts" });
		SubPartComboBox.SelectedIndex = 0;
		SubPartComboBox.IsEnabled = false;
	}

	public void ComboBox_SelectByName(string subPartName)
	{
		int i = 0;
		foreach (object? obj in SubPartComboBox.Items)
		{
			if (obj != null && obj is ComboBoxItem itm)
			{
				if (((string) itm.Content!) == subPartName)
				{
					SubPartComboBox.SelectedIndex = i;
					break;
				}
			}
			i++;
		}
	}
	#endregion





	#region ButtonHandlers
	private void OnDeselectSubpart(object? obj, RoutedEventArgs e)
	{
		SubPart_Selected_Deselect();
	}

	private void OnMovePartBeforeClick(object sender, RoutedEventArgs e)
	{
		MainWindow.Instance!.Part_MoveBefore();
	}

	private void OnMovePartAfterClick(object sender, RoutedEventArgs e)
	{
		MainWindow.Instance!.Part_MoveAfter();
	}

	private void OnZoomInClick(object sender, RoutedEventArgs e)
	{
		ZoomOutBtn.IsEnabled = true;
		switch (_ZoomTimeline)
		{
			case 1:
				_ZoomTimeline = 2;
				break;
			case 2:
				_ZoomTimeline = 4;
				break;
			case 4:
				_ZoomTimeline = 8;
				ZoomInBtn.IsEnabled = false;
				break;
			default:
				return;
		}

		Part_RenderAll();
		Timeline_UpdateSteps(true);
	}

	private void OnZoomOutClick(object sender, RoutedEventArgs e)
	{
		ZoomInBtn.IsEnabled = true;
		switch (_ZoomTimeline)
		{
			case 8:
				_ZoomTimeline = 4;
				break;
			case 4:
				_ZoomTimeline = 2;
				break;
			case 2:
				_ZoomTimeline = 1;
				ZoomOutBtn.IsEnabled = false;
				break;
			default:
				return;
		}

		Part_RenderAll();
		Timeline_UpdateSteps(true);
	}
	#endregion
}
