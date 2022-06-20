using CommonCore;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.Messaging;
using CommonCore.RpgGame.Dialogue;
using CommonCore.State;
using CommonCore.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lucidity.Intro2Scene
{

    /// <summary>
    /// Sequence script for Intro2 scene
    /// </summary>
    public class Intro2SequenceScript : MonoBehaviour
    {
        [SerializeField]
        private Image BackgroundImage = null;

        private Coroutine CurrentCoroutine = null;

        private void Start()
        {

            StartSequence();
        }

        private void StartSequence()
        {
            CurrentCoroutine = StartCoroutine(CoSequence());
        }

        private IEnumerator CoSequence()
        {
            //play music
            AudioPlayer.Instance.SetMusic("calm1", MusicSlot.Event, 0.5f, true, false); //reuse tension1 for now
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show opening
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage("intro_longshot");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 1.0f, false, false, false);
            AudioPlayer.Instance.PlaySound("cine_swordclash", SoundType.Sound, false);
            //QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Castle Brukton", 10.0f));
            yield return new WaitForSeconds(5.0f);

            SetBackgroundImage("intro_smirk");
            yield return new WaitForSeconds(3.0f);

            SetBackgroundImage("intro_crossedswords");
            AudioPlayer.Instance.PlaySound("cine_swordclatter", SoundType.Sound, false);
            yield return new WaitForSeconds(1.0f);

            SetBackgroundImage("intro_swordfly");
            yield return new WaitForSeconds(1.0f);
            AudioPlayer.Instance.PlaySound("cine_hitground", SoundType.Sound, false);
            SetBackgroundImage("intro_downed");

            yield return SkippableWait.WaitForSeconds(5.0f);

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("PromiseFight", false, () => {
                doContinue = true;
            });
            SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            //fadeout
            //ScreenFader.FadeTo(Color.black, 3.0f, false, false, false);
            //yield return new WaitForSeconds(3.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 10);

            //exit
            SharedUtils.ChangeScene("BedroomScene");

        }

        private void SetBackgroundImage(string background)
        {
            //holy fuck this is some halfassed code
            //and yes it is copy/pasted 3 or 4 times

            if (string.IsNullOrEmpty(background))
            {
                BackgroundImage.color = Color.black;
                BackgroundImage.sprite = null;
                return;
            }

            try
            {
                var spr = CoreUtils.LoadResource<Sprite>("Dialogue/bg/" + background);
                if (spr == null)
                    throw new KeyNotFoundException();
                BackgroundImage.color = Color.white;
                BackgroundImage.sprite = spr;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to set background image because of {e.GetType().Name}");
                if (ConfigState.Instance.UseVerboseLogging)
                    Debug.LogException(e);
            }
        }
    }
}