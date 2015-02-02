using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Frontiers.World.Gameplay;
using Frontiers.Data;
using Frontiers.Story;
using Frontiers.World.BaseWIScripts;

namespace Frontiers.World
{
		public class TriggerDTS : WorldTrigger
		{
				public TriggerDTSState State = new TriggerDTSState();

				public override bool OnPlayerEnter()
				{
						Speech speech = null;
						Character character = null;
						Talkative talkative = null;
						if (Mods.Get.Runtime.LoadMod <Speech>(ref speech, "Speech", State.SpeechName)) {
								//good we have the speech now what about the character?
								if (State.RequireSpawnedCharacter) {
										if (!Characters.Get.SpawnedCharacter(State.CharacterName, out character)) {
												return false;
										}
										if (character.worlditem.Is <Talkative>(out talkative)) {
												talkative.SayDTS(speech);
												if (State.FocusOnPlayer) {	//wuhoo look at these nested IFs!
														character.LookAtPlayer();
												}
												return true;
										}
								} else {
										//just say the speech
										StartCoroutine(GiveAnonymousSpeech(speech, State.CharacterName, character));
										return true;
								}
						}
						return false;
				}

				protected IEnumerator GiveAnonymousSpeech(Speech speech, string characterName, Character character)
				{
						speech.StartSpeech(characterName);
						string pageText = string.Empty;
						float pageDuration = 0f;
						int lastSpeechPage = 0;
						while (speech.GetPage(ref pageText, ref pageDuration, ref lastSpeechPage, true)) {
								GUI.NGUIScreenDialog.AddSpeech(pageText, characterName, pageDuration);
								yield return WorldClock.WaitForSeconds(pageDuration);
						}
						speech.FinishSpeech(characterName);
						Mods.Get.Runtime.SaveMod <Speech>(speech, "Speech", speech.Name);

						if (!string.IsNullOrEmpty(State.MessageOnRemainInRange)) {
								//wait until the delay is over
								//if the player is still in the trigger
								//send the message to the target character
								yield return WorldClock.WaitForSeconds(State.MessageDelay);
								if (character != null) {
										if (Vector3.Distance(transform.position, Player.Local.Position) < State.FocusRange) {
												character.gameObject.SendMessage(State.MessageOnRemainInRange);
										}
								}
						}

						yield break;
				}
		}

		[Serializable]
		public class TriggerDTSState : WorldTriggerState
		{
				public string CharacterName = string.Empty;
				public bool RequireSpawnedCharacter = true;
				[FrontiersAvailableModsAttribute("Speech")]
				public string SpeechName = string.Empty;
				public bool FocusOnPlayer = false;
				public float MessageDelay = 0f;
				public string MessageOnRemainInRange;
				public float FocusRange = 10.0f;
		}
}