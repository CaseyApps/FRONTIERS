using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;

namespace Frontiers.GUI {
	public class InventoryEnabler : InventorySquare
	{
		public override bool IsEnabled {
			get {
				return HasStack;
			}
		}

		public override void OnClickSquare ()
		{
	//		////Debug.Log ("INVENTORYENABLER: Is enabled?" + IsEnabled.ToString ( ));
	//		if (IsEnabled) {
	//			////Debug.Log ("INVENTORYENABLER: Has top item? " + mStack.HasTopItem.ToString ( ));
	//			if (mStack.HasTopItem) {
	//				////Debug.Log ("INVENTORYENABLER: Top item is stack container? " + mStack.TopItem.IsStackContainer.ToString ( ));
	//				if (mStack.TopItem.IsStackContainer) {
	//					////Debug.Log ("INVENTORYENABLER: Does stack item have more than 0 stacks? " + mStack.TopItem.StackContainer.NumStacks.ToString ( ));
	//				}
	//			}
	//		}
					
			if (!IsEnabled) {
				////Debug.Log ("INVENTORYENABLER: Wasn't enabled");
				return;
			}
			
			bool pickUp = false;
			bool playSound = false;
			bool showMenu = false;
			WIStackError error = WIStackError.None;

			if (UserActionManager.LastMouseClick == 1) {
				//Debug.Log ("Mouse button 1 is down");
				showMenu = true;
			} else {
				//Debug.Log ("Mouse button 1 is NOT down");
			}

			if (showMenu) {
				//Debug.Log ("Showing menu");
				if (mStack.HasTopItem) {
					WorldItem topItem = null;
					WorldItemUsable usable = null;
					if (!mStack.TopItem.IsWorldItem) {
						Stacks.Convert.TopItemToWorldItem (mStack, out topItem);
					} else {
						topItem = mStack.TopItem.worlditem;
					}
					if (mUsable != null) {
						mUsable.Finish ();
						mUsable = null;
					}
					usable = topItem.MakeUsable ();
					usable.ShowDoppleganger = false;
					usable.TryToSpawn (true, out mUsable);
					usable.ScreenTarget = transform;
					usable.ScreenTargetCamera = NGUICamrea;
					usable.RequirePlayerFocus = false;
					//the end result *should* affect the new item
					return;
				}
			}
			
			if (Player.Local.Inventory.SelectedStack.IsEmpty) {	////Debug.Log ("INVENTORYENABLER: Selected stack is empty");
				if (mStack.NumItems == 1) {	////Debug.Log ("INVENTORYENABLER: Adding items TO selected stack");
					playSound = Stacks.Add.Items (mStack, Player.Local.Inventory.SelectedStack, ref error);
					pickUp = true;
				} else {	////Debug.Log ("INVENTORYENABLER: Doing nothing, no items in stack");
					return;
				}
			} else if (Player.Local.Inventory.SelectedStack.NumItems == 1) {	////Debug.Log ("INVENTORYENABLER: Selected stack has 1 item");
				if (mStack.IsEmpty) {	////Debug.Log ("INVENTORYENABLER: Adding items FROM selected stack");
					playSound = Stacks.Add.Items (Player.Local.Inventory.SelectedStack, mStack, ref error);
				} else {	////Debug.Log ("INVENTORYENABLER: Swapping stacks");
					playSound = Stacks.Swap.Stacks (Player.Local.Inventory.SelectedStack, mStack, ref error);
				}
			}
			
			if (playSound) {
				if (pickUp) {
					MasterAudio.PlaySound (MasterAudio.SoundType.PlayerInterface, "InventoryPickUpStack");
				} else {
					MasterAudio.PlaySound (MasterAudio.SoundType.PlayerInterface, "InventoryPlaceStack");
				}
			} else {
				//we did nothing - show a help dialog
				GUIManager.PostIntrospection ("This is a spot for containers. Containers let me carry things.", true);
			}
			Refresh ();
		}

		public override void SetProperties ()
		{
			DisplayMode = SquareDisplayMode.Empty;
			ShowDoppleganger = false;

			if (IsEnabled) {
				if (mStack.HasTopItem) {
					ShowDoppleganger = true;
					IWIBase topItem = mStack.TopItem;
					DopplegangerProps.PrefabName = topItem.PrefabName;
					DopplegangerProps.PackName = topItem.PackName;
					DopplegangerProps.State = topItem.State;
					if (topItem.IsStackContainer) {
						DisplayMode = SquareDisplayMode.Success;
					} else {
						DisplayMode = SquareDisplayMode.Error;
					}
				}
			} else {
				DisplayMode = SquareDisplayMode.Disabled;
			}
		}

		public override void SetInventoryStackNumber ()
		{
			StackNumberLabel.enabled = false;
		}
	}
}