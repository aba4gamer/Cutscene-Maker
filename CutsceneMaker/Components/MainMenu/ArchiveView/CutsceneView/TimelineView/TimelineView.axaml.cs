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
	public Func<string, string, List<SubPart>>? RequestSubParts;
	private TimelinePart? SelectedTimelinePart = null;
	private TimelinePart? SelectedTimelineSubPart = null;
	private IDisposable? _SubPartBoxSubscription;
	private bool _IsEditingSubPartIndex = false;


	public TimelineView()
	{
		InitializeComponent();
		ResetSubPartComboBox();

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




				string subPartName = GetContentFromComboBoxItem(i); // (string) (((ComboBoxItem) SubPartComboBox.Items[i]).Content);
				if (SelectedTimelinePart != null && RequestSubParts != null)
					RenderSubParts(RequestSubParts(SelectedTimelinePart.PartName, subPartName));
			});
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



	public void UpdateSteps(double windowWidth, int totalSteps)
	{
		// Oh so good math, I won't even try to explain what this does, just know that it gets the job done.
		int limit = totalSteps + 40;
		int minWidth = ((int) (windowWidth * 0.125)) - 30;
		if (limit < minWidth) limit = minWidth;
		int diff = limit % 5;
		limit -= diff - 1;

		// For performance optimization, let's not draw 300+ elements each time
		// someone resizes the window or opens a menu item (because according
		// to Avalonia, that counts as a resize as well).
		if (Math.Floor((limit * 0.1) * 2) == MainTimelineSteps.Children.Count)
			return;

		MainTimelineSteps.Children.Clear();
		SubTimelineSteps.Children.Clear();


		for (int i = 1; i < limit; i++) {
			if (i % 5 == 0 && i % 10 != 0) {
				MainTimelineSteps.Children.Add(new TimelineStepFrac());
				SubTimelineSteps.Children.Add(new TimelineStepFrac());
			}

			if (i % 10 == 0) {
				MainTimelineSteps.Children.Add(new TimelineStep(i));
				SubTimelineSteps.Children.Add(new TimelineStep(i));
			}
		}
	}

	public void RenderParts(List<Cutscene.Part> parts)
	{
		MainTimeline.Children.Clear();

		foreach (Cutscene.Part part in parts)
		{
			TimelinePart timelinePart = new(part, part.PartName, part.TimeEntry.TotalStep, false, 0);
			timelinePart.Click = SelectedPart;
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
		OnSelectPart(timelinePart.PartName);
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
		if (SelectedTimelinePart != null && RequestSubParts != null)
			RenderSubParts(RequestSubParts(SelectedTimelinePart.PartName, subPartName));
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

		SelectedTimelinePart.Border.Width = step * 8;
	}

	public void UpdateSelectedSubPartStep(int step)
	{
		if (SelectedTimelineSubPart == null)
			return;

		SelectedTimelineSubPart.Border.Width = step * 8;
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

	public void RenderSubParts(List<SubPart> subParts)
	{
		SubTimeline.Children.Clear();

		foreach (SubPart subPart in subParts)
		{
			TimelinePart timelinePart = new(subPart, subPart.SubPartName, subPart.SubPartTotalStep, true, subPart.MainPartStep);
			timelinePart.Click = SelectedSubPart;
			SubTimeline.Children.Add(timelinePart);
		}
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
}
