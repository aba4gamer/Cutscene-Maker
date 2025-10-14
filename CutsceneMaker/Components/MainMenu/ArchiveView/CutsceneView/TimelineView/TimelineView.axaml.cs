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
	public Action<string> OnSelectPart = (string partName) => {};
	public Action<string, string> OnSelectSubPart = (string partName, string subPartName) => {};
	public Action<string, string> OnDeselectSubPart = (string partName, string subPartName) => {};
	public System.Action RequestReRender = () => {};
	public System.Action OnMovePartBefore = () => {};
	public System.Action OnMovePartAfter = () => {};
	public Func<string, string, SubPart>? RequestSubPart;
	public Func<int>? RequestPartStep;
	public ContextMenu? PartCtx = null;
	public ContextMenu? SubPartCtx = null;
	public ContextMenu? PartEditCtx = null;
	public ContextMenu? SubPartEditCtx = null;
	private TimelinePart? SelectedTimelinePart = null;
	private TimelinePart? SelectedTimelineSubPart = null;
	private IDisposable? _SubPartBoxSubscription;
	private bool _IsEditingSubPartIndex = false;
	private int _ZoomTimeline = 2;


	public TimelineView()
	{
		InitializeComponent();
		ResetSubPartComboBox();

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
					DeselectSubPart();
					return;
				}

				string subPartName = GetContentFromComboBoxItem(i);
				if (SelectedTimelinePart != null && RequestSubPart != null && RequestPartStep != null)
					RenderSubPart(RequestPartStep(), RequestSubPart(SelectedTimelinePart.PartName, subPartName));
			});
	}

	public void LoadContextMenus()
	{
		if (PartCtx != null)
			MainTimeline.ContextMenu = PartCtx;
		if (SubPartCtx != null && SelectedTimelinePart != null)
			SubTimeline.ContextMenu = SubPartCtx;
		else
			SubTimeline.ContextMenu = null;
	}

	private string GetContentFromComboBoxItem(int i)
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

	private void SetContentToComboBoxItem(int i, string partName)
	{
		object? maybeComboBox = SubPartComboBox.Items[i];
		if (maybeComboBox == null)
			throw new Exception($"There is no item at index {i}");
		if (!(maybeComboBox is ComboBoxItem))
			throw new Exception($"The item at index {i} is not a ComboBoxItem");

		ComboBoxItem comboItemPart = (ComboBoxItem) maybeComboBox;
		comboItemPart.Content = partName;
	}



	public void UpdateSteps(double windowWidth, int totalSteps, bool forced = false)
	{
		// Oh so good math, I won't even try to explain what this does, just know that it gets the job done.
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

	public void RenderParts(List<Cutscene.Part> parts)
	{
		MainTimeline.Children.Clear();

		foreach (Cutscene.Part part in parts)
		{
			int max = 8 / _ZoomTimeline;
			TimelinePart timelinePart = new(part, part.PartName, Math.Max(part.TimeEntry.TotalStep, max), false, _ZoomTimeline);
			timelinePart.Click = SelectedPart;
			timelinePart.Ctx = PartEditCtx;
			timelinePart.LoadContextMenus();
			MainTimeline.Children.Add(timelinePart);
		}
	}

	private void ActivateAllParts()
	{
		foreach (object obj in MainTimeline.Children)
		{
			if (obj is TimelinePart)
			{
				TimelinePart timelinePart = (TimelinePart) obj;
				timelinePart.Select(false);
			}
		}
	}

	private void ActivateAllSubParts()
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

	private void SelectedPart(TimelinePart timelinePart)
	{
		ActivateAllParts();
		timelinePart.Select(true);
		SelectedTimelinePart = timelinePart;

		object? timelinePartParentObj = timelinePart.Parent;
		if (timelinePartParentObj != null && timelinePartParentObj is DockPanel timelinePartParent)
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

		LoadContextMenus();
		OnSelectPart(timelinePart.PartName);
	}

	public void DeselectPart()
	{
		if (SelectedTimelinePart != null)
		{
			SelectedTimelinePart.Select(false);
			SelectedTimelinePart = null;
		}
		LoadContextMenus();
	}

	private void SelectedSubPart(TimelinePart timelinePart)
	{
		if (SelectedTimelinePart == null)
			return;

		ActivateAllSubParts();
		timelinePart.Select(true);
		SelectedTimelinePart.SelectedSubPart();
		SelectedTimelineSubPart = timelinePart;
		OnSelectSubPart(SelectedTimelinePart.PartName, timelinePart.PartName);
	}

	public void SetSelectedPart(string partName)
	{
		foreach (object timelinePartObj in MainTimeline.Children)
		{
			if (timelinePartObj is TimelinePart timelinePart)
			{
				if (timelinePart.PartName == partName)
				{
					timelinePart.Click(timelinePart);
					break;
				}
			}
		}
	}

	public void SetSelectedSubPart(string subPartName)
	{
		foreach (object timelinePartObj in SubTimeline.Children)
		{
			if (timelinePartObj is TimelinePart timelinePart)
			{
				if (timelinePart.PartName == subPartName)
				{
					timelinePart.Click(timelinePart);
					break;
				}
			}
		}
	}

	public void SetSubPartsComboBox(List<SubPart> subParts)
	{
		if (subParts.Count < 1)
		{
			ResetSubPartComboBox();
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
		_IsEditingSubPartIndex = false;

		SubPartComboBox.SelectedIndex = 1;

		string subPartName = GetContentFromComboBoxItem(1);
		if (SelectedTimelinePart != null && RequestSubPart != null && RequestPartStep != null)
			RenderSubPart(RequestPartStep(), RequestSubPart(SelectedTimelinePart.PartName, subPartName));
	}

	public void ResetSubPartComboBox()
	{
		SubPartComboBox.Items.Clear();
		SubPartComboBox.Items.Add(new ComboBoxItem { Content = "No SubParts" });
		SubPartComboBox.SelectedIndex = 0;
		SubPartComboBox.IsEnabled = false;
	}

	public void UpdateSelectedPartStep(int step)
	{
		if (SelectedTimelinePart == null)
			return;

		SelectedTimelinePart.Border.Width = step * _ZoomTimeline;
	}

	public void UpdateSelectedSubPartStep(int step)
	{
		if (SelectedTimelineSubPart == null)
			return;

		SelectedTimelineSubPart.Border.Width = step * _ZoomTimeline;
	}

	public void UpdateSelectedPartName(string name)
	{
		if (SelectedTimelinePart == null)
			return;

		SelectedTimelinePart.ChangeName(name);
	}

	public void UpdateSelectedSubPartName(string name)
	{
		if (SelectedTimelineSubPart == null)
			return;

		_IsEditingSubPartIndex = true;
		SelectedTimelineSubPart.ChangeName(name);
		int i = SubPartComboBox.SelectedIndex;
		SetContentToComboBoxItem(i, name);
		SubPartComboBox.SelectedIndex = 0;
		SubPartComboBox.SelectedIndex = i; // This is for updating the string shown in the selected box. Idk why it doesn't change when I change the Content.
		_IsEditingSubPartIndex = false;
	}

	public void RenderSubPart(int space, SubPart? subPart)
	{
		SubTimeline.Children.Clear();

		if (subPart == null)
			return;
		SubTimeline.Children.Add(new TimelineSubPartSpacer((space + subPart.MainPartStep) * _ZoomTimeline));
		int max = 8 / _ZoomTimeline;
		TimelinePart timelinePart = new(subPart, subPart.SubPartName, Math.Max(subPart.SubPartTotalStep, max), true, _ZoomTimeline);
		timelinePart.Click = SelectedSubPart;
		timelinePart.Ctx = SubPartEditCtx;
		timelinePart.LoadContextMenus();
		SubTimeline.Children.Add(timelinePart);
	}

	public void DeselectSubPart()
	{
		if (SelectedTimelinePart == null || SelectedTimelineSubPart == null)
			return;

		SelectedTimelineSubPart.Select(false);
		OnDeselectSubPart(SelectedTimelinePart.PartName, SelectedTimelineSubPart.PartName);
		SelectedTimelineSubPart = null;
	}

	private void OnDeselectSubpart(object? obj, RoutedEventArgs e)
	{
		DeselectSubPart();
	}

	private void OnMovePartBeforeClick(object sender, RoutedEventArgs e)
	{
		OnMovePartBefore();
	}

	private void OnMovePartAfterClick(object sender, RoutedEventArgs e)
	{
		OnMovePartAfter();
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
		}
		RequestReRender();
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
		}
		RequestReRender();
	}
}
