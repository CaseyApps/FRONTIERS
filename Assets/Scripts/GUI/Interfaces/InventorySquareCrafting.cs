using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Frontiers;
using Frontiers.World;
using Frontiers.World.Gameplay;
using System;

namespace Frontiers.GUI
{
		public class InventorySquareCrafting : InventorySquare
		{
				public override bool IsEnabled {
						get {
								return HasStack;
						}
				}

				public bool AreRequirementsMet {
						get {
								mRequirementsMet = false;
								if (EnabledForBlueprint && HasStack && mStack.HasTopItem) {
										mRequirementsMet = CraftSkill.AreRequirementsMet(Stack.TopItem, RequiredItemTemplate, Strictness, Stack.NumItems, out mNumCraftableItems);
								}
								return mRequirementsMet;
						}
				}

				public Action BlueprintPotentiallyChanged;
				public bool HasBlueprint = false;
				public GenericWorldItem RequiredItemTemplate;
				public bool EnabledForBlueprint {
						get {
								return mEnabledForBlueprint;
						}
				}
				public BlueprintStrictness Strictness = BlueprintStrictness.Default;
				public string RequirementBlueprint;

				public void DisableForBlueprint ( ) {
						mEnabledForBlueprint = false;
				}

				public void EnableForBlueprint (GenericWorldItem requiredItemTemplate, BlueprintStrictness strictness) {
						mEnabledForBlueprint = true;
						Strictness = strictness;
						RequiredItemTemplate.CopyFrom(requiredItemTemplate);
				}

				public bool RequirementCanBeCrafted {
						get {
								return !string.IsNullOrEmpty(RequirementBlueprint);
						}
				}

				public int NumCraftableItems {
						get {
								if (EnabledForBlueprint && mRequirementsMet) {
										return mNumCraftableItems;
								}
								return 0;
						}
				}

				public override void OnClickSquare()
				{
						//keep the top item handy to see if we've changed
						IWIBase oldTopItem = null;
						IWIBase newTopItem = null;
						if (HasStack && Stack.HasTopItem) {
								oldTopItem = Stack.TopItem;
						}

						base.OnClickSquare();

						if (HasStack && Stack.HasTopItem) {
								newTopItem = Stack.TopItem;
						}

						if (newTopItem != oldTopItem) {
								if (!HasBlueprint || !EnabledForBlueprint) {
										//if we don't have a blueprint yet
										//or if we have one but aren't enabled yet
										//tell the crafting interface to look for a new blueprint
										BlueprintPotentiallyChanged.SafeInvoke();
								}
						}
				}

				public void OnSelectBlueprint (System.Object result)
				{
						UsingMenu = false;

						WIListResult dialogResult = result as WIListResult;
						switch (dialogResult.Result) {
								case "Craft":
										//we want to select a new blueprint
										WIBlueprint blueprint = null;
										if (Blueprints.Get.Blueprint (RequirementBlueprint, out blueprint)) {
												GUIInventoryInterface.Get.CraftingInterface.OnSelectBlueprint(blueprint);
										}
										break;

								default:
										break;
						}
				}

				protected override void OnRightClickSquare()
				{
						//right clicking a blueprint square opens a menu
						//where you can select the blueprint used to create the item
						if (EnabledForBlueprint && RequirementCanBeCrafted) {
								SpawnOptionsList optionsList = gameObject.GetOrAdd <SpawnOptionsList>();
								optionsList.MessageType = string.Empty;//"Take " + mSkillUseTarget.DisplayName;
								optionsList.Message = "This item can be crafted";
								optionsList.FunctionName = "OnSelectBlueprint";
								optionsList.RequireManualEnable = false;
								optionsList.OverrideBaseAvailabilty = true;
								optionsList.FunctionTarget = gameObject;
								optionsList.AddOption(new WIListOption("Craft " + DopplegangerProps.DisplayName, "Craft"));
								optionsList.AddOption(new WIListOption("Cancel"));
								optionsList.ShowDoppleganger = false;
								GUIOptionListDialog dialog = null;
								if (optionsList.TryToSpawn(true, out dialog)) {
										UsingMenu = true;
										optionsList.ScreenTarget = transform;
										optionsList.ScreenTargetCamera = NGUICamera;
								}
						}
				}

				public override void SetProperties()
				{
						DisplayMode = SquareDisplayMode.Disabled;
						ShowDoppleganger = false;
						MouseoverHover = false;
						DopplegangerMode = WIMode.Crafting;

						if (!EnabledForBlueprint || !HasBlueprint) {
								if (HasStack && mStack.HasTopItem) {
										MouseoverHover = true;
										ShowDoppleganger = true;
										DopplegangerMode = WIMode.Stacked;
										IWIBase topItem = mStack.TopItem;
										DopplegangerProps.CopyFrom(topItem);
										DisplayMode = SquareDisplayMode.Enabled;
										//?
								} else {
										DopplegangerProps.Clear();
										RequirementBlueprint = string.Empty;
								}
						} else {
								MouseoverHover = true;
								ShowDoppleganger = true;
								DopplegangerProps.Clear();
								if (HasStack && mStack.HasTopItem) {
										DopplegangerMode = WIMode.Stacked;
										IWIBase topItem = mStack.TopItem;
										DopplegangerProps.CopyFrom (topItem);
										if (AreRequirementsMet) {
												DisplayMode = SquareDisplayMode.Success;
										} else {
												DisplayMode = SquareDisplayMode.Error;
										}
								} else { 
										//we know this because we can't meet requirements without items
										mRequirementsMet = false;
										mNumCraftableItems = 0;
										DopplegangerProps.CopyFrom (RequiredItemTemplate);
										DisplayMode = SquareDisplayMode.Enabled;
								}
						}
				}

				public void SetRequiredItem(GenericWorldItem newRequirement)
				{
						if (newRequirement != RequiredItemTemplate) {
								RequiredItemTemplate = newRequirement;
								RefreshRequest();
						}
				}

				protected bool mEnabledForBlueprint = false;
				protected int mNumCraftableItems = 0;
				protected bool mRequirementsMet = false;
		}
}