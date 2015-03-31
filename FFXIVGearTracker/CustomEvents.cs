using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FFXIVGearTracker
{
	#region Delegate Only
	public delegate void ActivateCalcButtons(bool active);
	public delegate void PopGearSetBox(GroupBox popBox, GearSet popSet);
	public delegate void UpdGearSetValues(int jobIndex);
	public delegate void UpdGearValues();
	public delegate void PopFoodBoxes();
	public delegate void SelectTabPage(TabPage tab);
	#endregion

	#region Event Delegates
	public delegate void PopOutFormClosing();
	public delegate void PopOutSlotFilterChanged(string slot);
	public delegate void PopOutHighestTurnFilterChanged(int highTurn);
	public delegate void PopOutChangeCharacter(Character c);
	public delegate void PopOutUpdateCharacter(Character c);

	public delegate void StartProgressBar();
	public delegate void SetProgressBarMax(int maximum);
	public delegate void IncrementProgressBar();
	public delegate void StopProgressBar();
	public delegate void ChangeOwnedFlag(object sender, Item i, bool isOwned);
	#endregion

	public static class CustomEvents
	{
		public static event PopOutFormClosing ClosePopOutFormEvent;

		public static event PopOutSlotFilterChanged SlotFilterChangedEvent;
		public static event PopOutHighestTurnFilterChanged HighestTurnFilterChangedEvent;
		public static event PopOutChangeCharacter CharacterChangedEvent;
		public static event PopOutUpdateCharacter CharacterUpdatedEvent;

		public static event ChangeOwnedFlag ItemOwnedChangeEvent;

		public static void ChangeItemOwned(object sender, Item i, bool isOwned)
		{
			if (ItemOwnedChangeEvent != null)
			{
				ItemOwnedChangeEvent(sender, i, isOwned);
			}
		}

		public static void ClosePopOutForm()
		{
			if (ClosePopOutFormEvent != null)
			{
				ClosePopOutFormEvent();
			}
		}

		public static void ChangeSlotFilter(string slot)
		{
			if (SlotFilterChangedEvent != null)
			{
				SlotFilterChangedEvent(slot);
			}
		}

		public static void ChangeHighestTurnFilter(int highTurn)
		{
			if (HighestTurnFilterChangedEvent != null)
			{
				HighestTurnFilterChangedEvent(highTurn);
			}
		}

		public static void ChangeCharacter(Character c)
		{
			if (CharacterChangedEvent != null)
			{
				CharacterChangedEvent(c);
			}
		}

		public static void UpdateCharacter(Character c)
		{
			if (CharacterUpdatedEvent != null)
			{
				CharacterUpdatedEvent(c);
			}
		}
	}
}
