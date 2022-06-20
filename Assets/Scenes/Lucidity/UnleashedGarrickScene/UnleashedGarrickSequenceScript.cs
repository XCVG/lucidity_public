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

namespace Lucidity.UnleashedGarrickScene
{

    public class UnleashedGarrickSequenceScript : MonoBehaviour
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
            AudioPlayer.Instance.SetMusic("tension3", MusicSlot.Event, 0.5f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show opening
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage("garricktower_outside");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 1.0f, false, false, false);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Castle Garrick", 10.0f));
            yield return SkippableWait.WaitForSeconds(5.0f);            

            //show inside
            SetBackgroundImage("garricktower_wide");
            yield return SkippableWait.WaitForSeconds(5.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            SetBackgroundImage("garrickmap");
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Lord Brukton's lands, ripe for the taking.", 5.0f));
            yield return SkippableWait.WaitForSeconds(5.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("UnleashedGarrick", false, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            //fadeout
            ScreenFader.FadeTo(Color.black, 3.0f, false, false, false);
            yield return new WaitForSeconds(3.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 270);

            //exit
            SharedUtils.ChangeScene("WarTimeskipScene");

        }

        private void SetBackgroundImage(string background)
        {
            //holy fuck this is some halfassed code

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