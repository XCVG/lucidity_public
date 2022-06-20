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

namespace Lucidity.WarQuietScene
{

    public class WarQuietSequenceScript : MonoBehaviour
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
            AudioPlayer.Instance.SetMusic("tension1", MusicSlot.Event, 0.5f, true, false); //reuse tension1 for now
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //show opening
            ScreenFader.FadeTo(Color.black, 0.01f, true, false, false);
            SetBackgroundImage("castle_outside");
            yield return null;
            ScreenFader.FadeFrom(Color.black, 1.0f, false, false, false);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Castle Brukton", 10.0f));
            yield return SkippableWait.WaitForSeconds(5.0f);

            //show inside
            SetBackgroundImage("castle_council1");
            yield return SkippableWait.WaitForSeconds(5.0f);
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("", 0));

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("WarEnd", false, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            //fadeout
            ScreenFader.FadeTo(Color.black, 6.0f, false, false, false);
            yield return new WaitForSeconds(6.0f);

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);

            //set quest stage
            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 340);

            //exit
            SharedUtils.ChangeScene("DanceOpenScene");

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